using GlobeAuction.Models;
using NLog;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace GlobeAuction.Helpers
{
    public class BackgroundMaintenance
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private ApplicationDbContext db = new ApplicationDbContext();
        private DateTime _lastMaintenance = DateTime.MinValue;
        private static readonly TimeSpan TimeBetweenMaintenance = TimeSpan.FromMinutes(60);
        private readonly string _baseFilePath;

        public BackgroundMaintenance(string baseFilePath)
        {
            _baseFilePath = baseFilePath;
        }

        public void DoMaintenance(CancellationToken cancelToken)
        {
            var emailHelper = new EmailHelper(_baseFilePath);

            while (true)
            {
                if (cancelToken.IsCancellationRequested) return;

                if (DateTime.Now.Subtract(_lastMaintenance) > TimeBetweenMaintenance)
                {
                    _lastMaintenance = DateTime.Now;

                    SendBidderPaymentReminders(emailHelper);
                    SendDonationItemCertificates(emailHelper);
                    RecalculateRevenueTotals();
                }

                Thread.Sleep(TimeSpan.FromMinutes(60));
            }
        }        

        private void SendBidderPaymentReminders(EmailHelper emailHelper)
        {
            var oneDayAgo = DateTime.Now.AddDays(-1);

            var biddersThatHaveNotPaid = db.Bidders
                .Where(b => b.CreateDate < oneDayAgo
                        && b.AuctionGuests.Any(g => g.TicketPrice > 0) //only nudge if they have non-free tickets
                        && b.AuctionGuests.Any(g => g.TicketPricePaid.HasValue == false) //only nudge if at least one ticket isn't paid
                        && b.IsPaymentReminderSent == false)
                .ToList();

            foreach (var bidder in biddersThatHaveNotPaid)
            {
                emailHelper.SendBidderPaymentReminder(bidder);
                bidder.IsPaymentReminderSent = true; //just send this once per bidder
                db.SaveChanges();

                _logger.Info("Sent payment reminder to {0}, bidder ID {1}", bidder.Email, bidder.BidderId);
            }
        }

        private void SendDonationItemCertificates(EmailHelper emailHelper)
        {
            //send for auction winnings first
            var allWinnings = new ItemsRepository(db).GetWinningsByBidder();
            foreach(var winning in allWinnings)
            {
                var paidAuctionItems = winning.Winnings.Where(ai => ai.Invoice != null && ai.Invoice.IsPaid).ToList();

                foreach(var paidAi in paidAuctionItems)
                {
                    foreach(var donationItem in paidAi.DonationItems.Where(di => di.UseDigitalCertificateForWinner && di.HasWinnerBeenEmailed == false))
                    {
                        emailHelper.SendDonationItemCertificate(winning.Bidder, donationItem);

                        donationItem.HasWinnerBeenEmailed = true;
                        db.SaveChanges();
                    }
                }
            }

            //now do store purchases
            var invoicesWithDonationItemToSend = db.Invoices
                .Where(i => i.IsPaid && i.StoreItemPurchases.Any(sip => sip.StoreItem.DonationItem != null && sip.StoreItem.DonationItem.UseDigitalCertificateForWinner && sip.StoreItem.DonationItem.HasWinnerBeenEmailed == false))
                .ToList();

            foreach(var invoice in invoicesWithDonationItemToSend)
            {
                var sipToSend = invoice.StoreItemPurchases
                    .Where(sip => sip.StoreItem.DonationItem != null && sip.StoreItem.DonationItem.UseDigitalCertificateForWinner && sip.StoreItem.DonationItem.HasWinnerBeenEmailed == false)
                    .ToList();

                foreach (var donationItem in sipToSend.Select(sip => sip.StoreItem.DonationItem))
                {
                    emailHelper.SendDonationItemCertificate(invoice, donationItem);

                    donationItem.HasWinnerBeenEmailed = true;
                    db.SaveChanges();
                }
            }
        }

        private void RecalculateRevenueTotals()
        {
            var paidInvoices = db.Invoices
                .Include(i => i.AuctionItems)
                .Include(i => i.StoreItemPurchases)
                .Where(i => i.IsPaid)
                .ToList();

            var totalRevenue = paidInvoices.Sum(i => i.TotalPaid);

            var allBidders = db.Bidders
                .Include(b => b.AuctionGuests)
                .Include(b => b.StoreItemPurchases)
                .Where(b => b.IsDeleted == false)
                .ToList();

            totalRevenue += allBidders.Sum(b => b.TotalPaid);

            var unpaidAuctionItems = db.AuctionItems
                .Where(ai => ai.WinningBid.HasValue && (ai.Invoice == null || ai.Invoice.IsPaid == false))
                .Select(ai => ai.WinningBid.Value)
                .DefaultIfEmpty(0)
                .Sum();

            totalRevenue += unpaidAuctionItems;

            RevenueHelper.SetTotalRevenue(totalRevenue);
        }
    }
}
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
        private static readonly TimeSpan TimeBetweenMaintenance = TimeSpan.FromMinutes(10);
        private readonly string _baseFilePath;

        public BackgroundMaintenance(string baseFilePath)
        {
            _baseFilePath = baseFilePath;
        }

        public void DoMaintenance(CancellationToken cancelToken)
        {
            try
            {
                var emailHelper = new EmailHelper(_baseFilePath);

                while (true)
                {
                    if (cancelToken.IsCancellationRequested) return;

                    if (DateTime.Now.Subtract(_lastMaintenance) > TimeBetweenMaintenance)
                    {
                        _lastMaintenance = DateTime.Now;

                        SendBidderPaymentReminders(emailHelper);
                        SendInvoicePaymentReminders(emailHelper);
                        SendDonationItemCertificates(emailHelper);
                        RecalculateRevenueTotals();
                    }

                    Thread.Sleep(TimeSpan.FromMinutes(60));
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Background thread is crashing");

                var body = "Background thread is crashing" + Environment.NewLine + exc;

                EmailHelperFactory.Instance().SendEmail("williams.wes@gmail.com", null, "Auction Site Error", body, false, null, null);
            }
        }        

        private void SendBidderPaymentReminders(EmailHelper emailHelper)
        {
            var oneDayAgo = Utilities.GetEasternTimeNow().AddDays(-1);

            //look at invoices to know about unpaid bidders since we always have a registration invoice
            var biddersThatHaveNotPaid = db.Invoices
                .Where(i => i.InvoiceType == InvoiceType.BidderRegistration
                        && i.Bidder != null
                        && i.Bidder.IsPaymentReminderSent == false
                        && i.IsPaid == false
                        && i.CreateDate < oneDayAgo
                        && i.TicketPurchases.Any(g => g.TicketPrice > 0) //only nudge if they have non-free tickets
                        && i.TicketPurchases.Any(g => g.TicketPricePaid.HasValue == false)) //only nudge if at least one ticket isn't paid
                .Select(i => i.Bidder)
                .ToList();

            foreach (var bidder in biddersThatHaveNotPaid)
            {
                emailHelper.SendBidderPaymentReminder(bidder);
                bidder.IsPaymentReminderSent = true; //just send this once per bidder
                db.SaveChanges();

                _logger.Info("Sent payment reminder to {0}, bidder ID {1}", bidder.Email, bidder.BidderId);
            }
        }

        private void SendInvoicePaymentReminders(EmailHelper emailHelper)
        {
            var oneDayAgo = Utilities.GetEasternTimeNow().AddDays(-1);

            //email about invoices that are standalone store purchases not linked to a bidder registration or winning bids
            var invoicesNotPaid = db.Invoices
                .Where(i => i.InvoiceType == InvoiceType.AuctionCheckout
                        && i.IsPaid == false
                        && i.IsPaymentReminderSent == false
                        && i.CreateDate < oneDayAgo
                        && i.Bids.Count == 0)
                .ToList();

            foreach (var invoice in invoicesNotPaid)
            {
                emailHelper.SendInvoicePaymentReminder(invoice);
                invoice.IsPaymentReminderSent = true; //just send this once per bidder
                db.SaveChanges();

                _logger.Info("Sent payment reminder to {0} for invoice ID {1}", invoice.Email, invoice.InvoiceId);
            }
        }

        private void SendDonationItemCertificates(EmailHelper emailHelper)
        {
            //send for auction winnings first
            var allWinnings = new ItemsRepository(db).GetWinningsByBidder();
            foreach(var winning in allWinnings)
            {
                var paidForBids = winning.Winnings.Where(ai => ai.Invoice != null && ai.Invoice.IsPaid && ai.HasWinnerBeenEmailed == false).ToList();

                foreach(var paidBid in paidForBids)
                {
                    foreach(var donationItem in paidBid.AuctionItem.DonationItems.Where(di => di.UseDigitalCertificateForWinner))
                    {
                        emailHelper.SendDonationItemCertificate(winning.Bidder, donationItem, paidBid.BidId);
                    }

                    paidBid.HasWinnerBeenEmailed = true;
                    db.SaveChanges();
                }
            }

            //now do store purchases
            var invoicesWithDonationItemToSend = db.Invoices
                .Where(i => i.IsPaid && i.StoreItemPurchases.Any(sip => sip.StoreItem.DonationItem != null && sip.StoreItem.DonationItem.UseDigitalCertificateForWinner && sip.HasWinnerBeenEmailed == false))
                .ToList();

            foreach(var invoice in invoicesWithDonationItemToSend)
            {
                var sipToSend = invoice.StoreItemPurchases
                    .Where(sip => sip.StoreItem.DonationItem != null && sip.StoreItem.DonationItem.UseDigitalCertificateForWinner && sip.HasWinnerBeenEmailed == false)
                    .ToList();

                foreach (var sip in sipToSend)
                {
                    var donationItem = sip.StoreItem.DonationItem;
                    emailHelper.SendDonationItemCertificate(invoice, donationItem, sip.StoreItemPurchaseId);

                    sip.HasWinnerBeenEmailed = true;
                    db.SaveChanges();
                }
            }
        }

        private void RecalculateRevenueTotals()
        {
            var paidInvoices = db.Invoices
                .Include(i => i.Bids)
                .Include(i => i.StoreItemPurchases)
                .Include(i => i.TicketPurchases)
                .Where(i => i.IsPaid)
                .ToList();

            var totalRevenueFromPaidInvoices = paidInvoices.Sum(i => i.TotalPaid);
            
            var unpaidAuctionItems = db.Bids
                .Where(b => b.IsWinning && (b.Invoice == null || b.Invoice.IsPaid == false))
                .Select(b => b.BidAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var totalRevenue = totalRevenueFromPaidInvoices + unpaidAuctionItems;

            //_logger.Info($"Recalc Revenue:{totalRevenue:C} PaidInvoicesCnt={paidInvoices.Count} PaidInvoicesRev={totalRevenue:C} UnpaidBids:{unpaidAuctionItems:C}");
            RevenueHelper.SetTotalRevenue(totalRevenue);
        }
    }
}
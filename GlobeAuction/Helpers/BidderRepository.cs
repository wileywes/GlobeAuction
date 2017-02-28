using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class BidderRepository
    {
        private ApplicationDbContext db;

        public BidderRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public void ApplyTicketPaymentToBidder(PayPalTransaction ppTrans, Bidder bidder)
        {
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();

            //only apply the ticket payment if there are un-paid tickets and the PP trans was successful
            if (ppTrans.WasPaymentSuccessful && bidder.AuctionGuests.Any(g => g.TicketPricePaid.HasValue == false))
            {
                var paymentLeft = ppTrans.PaymentGross;

                foreach (var guest in bidder.AuctionGuests)
                {
                    var priceToUseUp = Math.Min(guest.TicketPrice, paymentLeft);
                    guest.TicketPricePaid = priceToUseUp;
                    guest.TicketTransaction = ppTrans;
                    paymentLeft -= priceToUseUp;
                }
                db.SaveChanges();

                new EmailHelper().SendBidderPaymentConfirmation(bidder, ppTrans);
            }
        }

        public static int? GetBidderIdFromTransaction(PayPalTransaction ppTrans)
        {
            var bidderIdStr = (ppTrans.Custom ?? string.Empty).Replace("Bidder:", "");
            int bidderId;
            if (int.TryParse(bidderIdStr, out bidderId)) return bidderId;
            return null;
        }
    }
}
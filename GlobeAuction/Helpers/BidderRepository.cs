using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using GlobeAuction.Models;

namespace GlobeAuction.Helpers
{
    public class BidderRepository
    {
        private ApplicationDbContext db;

        public BidderRepository(ApplicationDbContext context)
        {
            db = context;
        }

        private const string BidderIdCookieName = "GlobeBidder";

        public static int? GetBidderIdFromTransaction(PayPalTransaction ppTrans)
        {
            var bidderIdStr = (ppTrans.Custom ?? string.Empty).Replace("Bidder:", "");
            int bidderId;
            if (int.TryParse(bidderIdStr, out bidderId)) return bidderId;
            return null;
        }

        public static void SetBidderCookie(Bidder bidder)
        {
            var cookie = bidder.BidderId + ";" + bidder.BidderNumber + ";" + bidder.LastName + ";" + bidder.Email;
            var bytes = Encoding.UTF8.GetBytes(cookie);
            var encoded = WebUtility.UrlEncode(Convert.ToBase64String(bytes));

            HttpContext.Current.Response.Cookies[BidderIdCookieName].Value = encoded;
            HttpContext.Current.Response.Cookies[BidderIdCookieName].Expires = DateTime.Now.AddMonths(1);
        }

        public static void ClearBidderCookie()
        {
            HttpContext.Current.Response.Cookies[BidderIdCookieName].Expires = DateTime.Now.AddDays(-1);
        }

        public static bool TryGetBidderInfoFromCookie(out BidderCookieInfo bidderInfo)
        {
            bidderInfo = null;

            if (HttpContext.Current.Request.Cookies[BidderIdCookieName] != null)
            {
                var encodedCookie = HttpContext.Current.Request.Cookies[BidderIdCookieName].Value;
                var bytes = Convert.FromBase64String(WebUtility.UrlDecode(encodedCookie));
                var cookieToParse = Encoding.UTF8.GetString(bytes);

                var parts = cookieToParse.Split(';');
                if (parts == null || parts.Length != 4) return false;

                int bidIdFromCookie, bidderNumberFromCookie;
                string lastNameFromCooke, emailFromCookie;

                if (!int.TryParse(parts[0], out bidIdFromCookie)) return false;
                if (!int.TryParse(parts[1], out bidderNumberFromCookie)) return false;
                lastNameFromCooke = parts[2];
                emailFromCookie = parts[3];

                bidderInfo = new BidderCookieInfo
                {
                    BidderId = bidIdFromCookie,
                    BidderNumber = bidderNumberFromCookie,
                    LastName = lastNameFromCooke,
                    Email = emailFromCookie
                };
                return true;
            }

            return false;
        }

        public bool TryGetValidatedBidderFromCookie(out Bidder bidder)
        {
            bidder = null;
            BidderCookieInfo info;
            if (TryGetBidderInfoFromCookie(out info))
            { 
                bidder = db.Bidders.FirstOrDefault(b =>
                    b.IsDeleted == false &&
                    b.BidderId == info.BidderId &&
                    b.BidderNumber == info.BidderNumber &&
                    b.Email == info.Email &&
                    b.LastName.Equals(info.LastName, StringComparison.OrdinalIgnoreCase));

                return bidder != null;
            }

            return false;
        }

        public bool IsBidderAllowedToBid(Bidder bidder)
        {
            //bidder needs to have at least one ticket paid for to bid
            var invoiceForBidderIfPaid = db.Invoices
                .FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration
                        && i.Bidder != null
                        && i.Bidder.BidderId == bidder.BidderId
                        && i.TicketPurchases.Any(g => g.TicketPrice == 0 || g.TicketPricePaid.HasValue));

            if (invoiceForBidderIfPaid != null) return true;

            //allow bidder to bid if the payment is pending
            var invoiceWithPaymentPending = db.Invoices
                .FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration
                        && i.Bidder != null
                        && i.Bidder.BidderId == bidder.BidderId
                        && i.PaymentTransaction.PaymentStatus == "Pending");

            if (invoiceWithPaymentPending != null) return true;

            return false;
        }

        public bool HasBidderPaidForRegistration(Bidder bidder)
        {
            var paidInvoice = db.Invoices
                .FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration
                        && i.Bidder != null
                        && i.Bidder.BidderId == bidder.BidderId
                        && i.IsPaid);
            return paidInvoice != null;
        }
    }
}
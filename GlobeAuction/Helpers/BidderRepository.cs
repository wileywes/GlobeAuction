using GlobeAuction.Models;

namespace GlobeAuction.Helpers
{
    public class BidderRepository
    {
        public static int? GetBidderIdFromTransaction(PayPalTransaction ppTrans)
        {
            var bidderIdStr = (ppTrans.Custom ?? string.Empty).Replace("Bidder:", "");
            int bidderId;
            if (int.TryParse(bidderIdStr, out bidderId)) return bidderId;
            return null;
        }
    }
}
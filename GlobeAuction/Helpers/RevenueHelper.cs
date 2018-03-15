namespace GlobeAuction.Helpers
{
    public static class RevenueHelper
    {
        private static decimal _totalRevenue = 0m;
        private static object _revenueLock = new object();

        public static void SetTotalRevenue(decimal rev)
        {
            lock(_revenueLock)
            {
                _totalRevenue = rev;
            }
        }

        public static void IncrementTotalRevenue(decimal rev)
        {
            lock (_revenueLock)
            {
                _totalRevenue += rev;
            }
        }

        public static decimal GetTotalRevenue()
        {
            return _totalRevenue;
        }
    }
}
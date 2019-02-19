using GlobeAuction.Models;

namespace GlobeAuction.Helpers
{
    public static class CacheHelper
    {
        private static CatalogData _catalogData;

        public static CatalogData GetCatalogDataFromCache()
        {
            return _catalogData;
        }

        public static void SetCatalogDataInCache(CatalogData data)
        {
            _catalogData = data;
        }

        public static void ClearCatalogDataInCache()
        {
            _catalogData = null;
        }
    }
}
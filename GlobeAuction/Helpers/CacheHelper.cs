using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}
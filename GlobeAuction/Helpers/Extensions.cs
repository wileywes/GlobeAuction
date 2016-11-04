using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public static class Extensions
    {
        public static string TruncateTo(this string original, int length)
        {
            if (string.IsNullOrEmpty(original) || original.Length <= length) return original;
            return original.Substring(0, length) + "...";
        }
    }
}
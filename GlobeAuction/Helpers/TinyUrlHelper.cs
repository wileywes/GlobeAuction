using System;
using System.Net;

namespace GlobeAuction.Helpers
{
    public class TinyUrlHelper
    {
        public string GetTinyUrl(string fullUrl)
        {
            var address = new Uri("http://tinyurl.com/api-create.php?url=" + fullUrl);
            var client = new WebClient();
            return client.DownloadString(address);
        }
    }
}
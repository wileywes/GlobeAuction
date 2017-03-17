using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class ErrorHelper
    {
        public void ProcessSiteError(Exception lastException, string caller)
        {
            var request = HttpContext.Current.Request;
            var user = "N/A";
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null) user = HttpContext.Current.User.Identity.GetUserName();

            if (request.Url.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return;

            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            var msg = string.Format("User:{0} Url:{1} Error:{2}", user, request.Url, lastException);

            logger.Error(msg);

            try
            {
                var body = GetErrorParam("User", user) +
                    GetErrorParam("Url", request.Url) +
                    GetErrorParam("UserAgent", request.UserAgent) +
                    GetErrorParam("Source", caller) +
                    GetErrorParam("Referrer", request.UrlReferrer) +
                    GetErrorParam("Exception", lastException);

                new EmailHelper().SendEmail("williams.wes@gmail.com", "Auction Site Error", body, false);
            }
            catch (Exception)
            { }
        }

        private string GetErrorParam(string name, object val)
        {
            return string.Format("<b>{0}</b> : {1} <br />", name, val);
        }
    }
}
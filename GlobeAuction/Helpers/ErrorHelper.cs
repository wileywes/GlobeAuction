﻿using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            if (lastException.Message.Contains("was not found or does not implement IController.")) return;
            if (lastException.Message.Contains("The requested resource can only be accessed via SSL")) return;
            if (lastException.Message.Contains("A potentially dangerous Request.Path value was detected from the client")) return;
            if (lastException is HttpAntiForgeryException) return;

            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            if (lastException is DbEntityValidationException)
            {
                try
                {
                    var dbErrors = string.Join(Environment.NewLine,
                        (lastException as DbEntityValidationException).EntityValidationErrors.SelectMany(e => e.ValidationErrors).Select(e => "Name=" + e.PropertyName + ", Error=" + e.ErrorMessage));

                    lastException = new ApplicationException("DB Errors: " + dbErrors, lastException);
                }
                catch (Exception exc)
                {
                    logger.Warn("Unable to inspect DbEntityValidationException : " + exc);
                }
            }

            BidderCookieInfo bidInfo = null;
            try
            {
                BidderRepository.TryGetBidderInfoFromCookie(out bidInfo);
            }
            catch (Exception)
            {
            }

            var msg = string.Format("User:{0} Url:{1} Error:{2}", user, request.Url, lastException);

            logger.Error(msg);

            try
            {
                var body = GetErrorParam("Bidder No", bidInfo?.BidderNumber) +
                    GetErrorParam("Bidder Email", bidInfo?.Email) +
                    GetErrorParam("Bidder LName", bidInfo?.LastName) +
                    GetErrorParam("User", user) +
                    GetErrorParam("Url", request.Url) +
                    GetErrorParam("UserAgent", request.UserAgent) +
                    GetErrorParam("HttpMethod", request.HttpMethod) +
                    GetErrorParam("Source", caller) +
                    GetErrorParam("Referrer", request.UrlReferrer) +
                    GetErrorParam("Exception", lastException);

                EmailHelperFactory.Instance().SendEmail("williams.wes@gmail.com", null, "Auction Site Error", body, false, null, null);
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
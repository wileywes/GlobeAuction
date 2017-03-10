using GlobeAuction.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GlobeAuction
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            IdentityConfig.SetupIdentity();
            AutoMapperConfig.RegisterMappings();

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfHasValueAttribute), typeof(RequiredIfHasValueValidator));

            var basePath = HttpContext.Current.Server.MapPath("/");
            HostingEnvironment.QueueBackgroundWorkItem(t => new BackgroundMaintenance(basePath).DoMaintenance(t));
        }

        protected void Application_Error()
        {
            var lastException = Server.GetLastError();
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
                    GetErrorParam("Referrer", request.UrlReferrer) +
                    GetErrorParam("Exception", lastException);

                new EmailHelper().SendEmail("williams.wes@gmail.com", "Auction Site Error", body, false);
            }
            catch(Exception)
            { }
        }

        private string GetErrorParam(string name, object val)
        {
            return string.Format("<b>{0}</b> : {1} <br />", name, val);
        }
    }
}

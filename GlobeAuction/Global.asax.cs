using GlobeAuction.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfHasValueAttribute), typeof(RequiredIfHasValueValidator));

            var basePath = HttpContext.Current.Server.MapPath("/");
            HostingEnvironment.QueueBackgroundWorkItem(t => new BackgroundMaintenance(basePath).DoMaintenance(t));
        }

        protected void Application_Error()
        {
            var lastException = Server.GetLastError();
            var request = HttpContext.Current.Request;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            var msg = string.Format("{0} {1}", request.Url, lastException);

            logger.Error(msg);

            try
            {
                new EmailHelper().SendEmail("williams.wes@gmail.com", "Auction Site Error", request.Url + Environment.NewLine + lastException);
            }
            catch(Exception)
            { }
        }
    }
}

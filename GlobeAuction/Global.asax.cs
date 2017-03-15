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
            new ErrorHelper().ProcessSiteError(Server.GetLastError(), "Application_Error");
        }
    }
}

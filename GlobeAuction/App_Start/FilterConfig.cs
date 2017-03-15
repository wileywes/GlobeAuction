using GlobeAuction.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Web;
using System.Web.Mvc;

namespace GlobeAuction
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndLogErrorAttribute());
            filters.Add(new System.Web.Mvc.AuthorizeAttribute());
            filters.Add(new RequireHttpsAttribute());
        }
    }

    public class HandleAndLogErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            new ErrorHelper().ProcessSiteError(filterContext.Exception, "HandleAndLogErrorAttribute");
        }
    }
}

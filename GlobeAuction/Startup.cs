using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GlobeAuction.Startup))]
namespace GlobeAuction
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

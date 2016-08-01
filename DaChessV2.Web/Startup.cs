using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(DaChessV2.Web.Startup))]
namespace DaChessV2.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
using System.Web.Mvc;
using System.Web.Routing;

namespace DaChess.Ui
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CreateParty",
                url: "Party/Create/{id}",
                defaults: new { controller = "Party", action = "Create", id= UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "StartParty",
                url: "Party/{partyName}",
                defaults: new { controller = "Party", action = "Start", partyName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

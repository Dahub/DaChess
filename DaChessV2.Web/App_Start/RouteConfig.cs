using System.Web.Mvc;
using System.Web.Routing;

namespace DaChessV2.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "MakeMove",
                url: "Party/MakeMove/{partyName}/{move}/{token}",
                defaults: new { controller = "Party", action = "MakeMove", partyName = UrlParameter.Optional, move = UrlParameter.Optional, token = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AddPlayer",
                url: "Party/AddPlayerToParty/{model}",
                defaults: new { controller = "Party", action = "AddPlayerToParty", model = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "GetParty",
                url: "Party/GetParty/{partyName}",
                defaults: new { controller = "Party", action = "GetParty", partyName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PlayParty",
                url: "Party/Play/{partyName}",
                defaults: new { controller = "Party", action = "Play", partyName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

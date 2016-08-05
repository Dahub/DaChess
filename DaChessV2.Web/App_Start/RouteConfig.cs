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
                name: "TimeOver",
                url: "Party/TimeOver/{partyName}/{token}",
                defaults: new { controller = "Party", action = "TimeOver", partyName = UrlParameter.Optional, token = UrlParameter.Optional }
              );

            routes.MapRoute(
                 name: "Replay",
                 url: "Party/Replay/{partyName}/{token}",
                 defaults: new { controller = "Party", action = "Replay", partyName = UrlParameter.Optional, token = UrlParameter.Optional }
               );

            routes.MapRoute(
                name: "Drawn",
                url: "Party/Drawn/{partyName}/{token}",
                defaults: new { controller = "Party", action = "Drawn", partyName = UrlParameter.Optional, token = UrlParameter.Optional }
              );

            routes.MapRoute(
              name: "Resign",
              url: "Party/Resign/{partyName}/{token}",
              defaults: new { controller = "Party", action = "Resign", partyName = UrlParameter.Optional, token = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GetCookies",
                url: "Player/GetPlayerInfosFromCookies/{partyName}",
                defaults: new { controller = "Player", action = "GetPlayerInfosFromCookies", partyName = UrlParameter.Optional }
            );

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

﻿using System.Web.Mvc;
using System.Web.Routing;

namespace DaChess.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "MakeMove",
               url: "Party/MakeMove/{name}/{move}/{token}",
               defaults: new { controller = "Party", action = "MakeMove", name = UrlParameter.Optional, move = UrlParameter.Optional, token = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "GetPlayerInfo",
                url: "Player/GetPlayerInfo/{token}/{partyName}",
                defaults: new { controller = "Player", action = "GetPlayerInfo", token = UrlParameter.Optional, partyName = UrlParameter.Optional }
            );

                routes.MapRoute(
                name: "AddplayerToParty",
                url: "Party/AddPlayerToParty/{id}",
                defaults: new { controller = "Party", action = "AddPlayerToParty", id = UrlParameter.Optional }
            );

                routes.MapRoute(
                 name: "GetParty",
                 url: "Party/Get/{name}",
                 defaults: new { controller = "Party", action = "Get", name = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "PostParty",
               url: "Party/Post/{id}",
               defaults: new { controller = "Party", action = "Post", id = UrlParameter.Optional }
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

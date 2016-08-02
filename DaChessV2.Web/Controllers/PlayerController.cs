using DaChessV2.Business;
using DaChessV2.Dto;
using System;
using System.Web.Mvc;

namespace DaChessV2.Web.Controllers
{
    public class PlayerController : Controller
    {
        public void SetPlayerCookies(string partyName, string token)
        {
            Response.Cookies["DaChessParty"].Value = partyName;
            Response.Cookies["DaChessParty"].Expires = DateTime.Now.AddYears(10);
            Response.Cookies["DaChessToken"].Value = token;
            Response.Cookies["DaChessToken"].Expires = DateTime.Now.AddYears(10);
        }

        public JsonResult GetPlayerInfosFromCookies(string partyName)
        {
            PlayerModel model = new PlayerModel();
            if (Request.Cookies["DaChessToken"] != null && Request.Cookies["DaChessParty"] != null)
            {
                model = new PlayerManager().GetPlayerColor(Request.Cookies["DaChessToken"].Value, Request.Cookies["DaChessParty"].Value, partyName);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetPlayerInfo(string token, string partyName)
        //{
        //    PlayerModel toReturn = new PlayerModel();

        //    try
        //    {
        //        toReturn.PartyName = partyName;
        //        Colors c = Factory.Instance.GetPlayerManager().GetPlayerColor(token, partyName);
        //        toReturn.IsBlack = c == Colors.BLACK;
        //        toReturn.IsWhite = c == Colors.WHITE;
        //    }
        //    catch (DaChessException ex)
        //    {
        //        toReturn.IsError = true;
        //        toReturn.ErrorMessage = ex.Message;
        //    }
        //    catch (Exception)
        //    {
        //        toReturn.IsError = true;
        //        toReturn.ErrorMessage = "Erreur non gérée dans la récupération de du joueur";
        //    }

        //    return Json(toReturn, JsonRequestBehavior.AllowGet);
        //}
    }
}
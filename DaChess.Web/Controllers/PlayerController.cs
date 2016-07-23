using DaChess.Business;
using DaChess.Web.Models;
using System;
using System.Web.Mvc;

namespace DaChess.Web.Controllers
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

        public string GetPlayerPartyName()
        {
            if (Request.Cookies["DaChessParty"] != null)
                return Request.Cookies["DaChessParty"].Value;
            return String.Empty;
        }

        public string GetPlayerToken()
        {
            if (Request.Cookies["DaChessToken"] != null)
                return Request.Cookies["DaChessToken"].Value;
            return String.Empty;
        }

        public JsonResult GetPlayerInfo(string token, string partyName)
        {
            PlayerModel toReturn = new PlayerModel();

            try
            {
                toReturn.PartyName = partyName;
                Colors c = Factory.Instance.GetPlayerManager().GetPlayerColor(token, partyName);
                toReturn.IsBlack = c == Colors.BLACK;
                toReturn.IsWhite = c == Colors.WHITE;
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la récupération de du joueur";
            }

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }
    }
}
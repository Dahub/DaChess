using System;
using System.Web.Mvc;

namespace DaChess.Ui.Controllers
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
    }
}
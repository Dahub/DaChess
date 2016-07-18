using System;
using System.Web.Mvc;

namespace DaChess.Ui.Controllers
{
    public class PartyController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Start(string partyName)
        {
            return View((object)partyName);
        }        
    }
}
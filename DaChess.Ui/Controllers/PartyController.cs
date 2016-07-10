using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaChess.Ui.Controllers
{
    public class PartyController : Controller
    {
        // GET: Party
        public ActionResult Create()
        {
            return View();
        }
    }
}
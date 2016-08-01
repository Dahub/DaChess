using DaChessV2.Business;
using DaChessV2.Dto;
using System.Web.Mvc;

namespace DaChessV2.Web.Controllers
{
    public class HomeController : Controller
    {     
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpPost]
        public JsonResult NewParty()
        {
            PartyModel toReturn = new PartyManager().CreateNewParty();
            return Json(toReturn);
        }
    }
}
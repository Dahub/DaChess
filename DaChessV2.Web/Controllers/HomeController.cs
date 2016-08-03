using DaChessV2.Business;
using DaChessV2.Dto;
using System.Web.Mvc;

namespace DaChessV2.Web.Controllers
{
    public class HomeController : Controller
    {     
        public ActionResult Index()
        {
            return View(new PartyOptionManager().GetOptionModel());
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpPost]
        public JsonResult NewParty(PartyOptionModel model)
        {
            PartyModel toReturn = new PartyManager().CreateNewParty();
            return Json(toReturn);
        }
    }
}
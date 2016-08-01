using DaChessV2.Business;
using DaChessV2.Dto;
using System.Web.Mvc;

namespace DaChessV2.Web.Controllers
{
    public class PartyController : Controller
    {
        public ActionResult Play(string partyName)
        {            
            return View((object)partyName);
        }

        public JsonResult GetParty(string partyName)
        {
            PartyModel model = new PartyManager().GetParty(partyName);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddPlayerToParty(PlayerModel model)
        {
            model = new PartyManager().AddPlayerToParty(model.PlayerColor, model.PartyName);
            return Json(model);
        }

        public JsonResult MakeMove(string partyName, string move, string token)
        {
            PartyModel toReturn = new PartyManager().MakeMove(move, partyName, token);
            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }
    }
}
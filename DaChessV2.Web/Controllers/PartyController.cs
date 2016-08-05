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

        [HttpPost]
        public JsonResult MakeMove(MoveModel model)
        {
            PartyModel toReturn = new PartyManager().MakeMove(model);
            return Json(toReturn);
        }

        public JsonResult Resign(string partyName, string token)
        {
            PartyModel toReturn = new PartyManager().Resign(partyName, token);
            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Drawn(string partyName, string token)
        {
            PartyModel toReturn = new PartyManager().Drawn(partyName, token);
            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Replay(string partyName, string token)
        {
            PartyModel toReturn = new PartyManager().Replay(partyName, token);
            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TimeOver(string partyName, string token)
        {
            PartyModel toReturn = new PartyManager().TimeOver(partyName, token);
            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }
    }
}
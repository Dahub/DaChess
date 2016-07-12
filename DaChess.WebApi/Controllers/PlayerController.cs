using DaChess.Business;
using DaChess.WebApi.Models;
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace DaChess.WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PlayerController : ApiController
    {
        public JsonResult<PartyModel> Post(PartyModel party)
        {
            try
            {
                if (party.WhiteAskToPlay && String.IsNullOrEmpty(party.WhiteToken))
                {
                    var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.WHITE);
                    party.WhiteToken = myParty.WhiteLink;
                }
                else if (party.BlackAskToPlay && String.IsNullOrEmpty(party.BlackToken))
                {
                    var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.BLACK);
                    party.BlackToken = myParty.BlackLink;
                }
            }
            catch (Exception ex)
            {
                party.IsError = true;
                party.ErrorMessage = ex.Message;
            }

            return Json(party);
        }
    }
}

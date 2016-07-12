using DaChess.Api.Models;
using System.Web.Http;
using DaChess.Business;
using System;
using System.Web.Http.Cors;
using System.Net.Http;
using System.Linq;
using System.Web.Http.Results;

namespace DaChess.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PartyController : ApiController
    {       
        public JsonResult<PartyModel> Get(HttpRequestMessage request)
        {
            //   request.Get
            string partyName = request.GetQueryNameValuePairs().Where(k => k.Key.ToLower().Equals("name")).First().Value;
            PartyModel toReturn = new PartyModel();

            try
            {
                Party myParty = Factory.Instance.GetPartyManager().GetByName(partyName);

                toReturn = new PartyModel()
                {
                    Id = myParty.Id,
                    Board = myParty.Board,
                    Name = myParty.PartLink,
                    BlackToken = myParty.BlackLink,
                    WhiteToken = myParty.WhiteLink,
                    WhiteTurn = myParty.WhiteTurn
                };
            }
            catch (Exception ex)
            {
               // return BadRequest(ex.Message);
            }

            return Json(toReturn); // Ok(toReturn);
        }        

        public IHttpActionResult Put(PartyModel model)
        {
            try
            {
                Party toUpdate = Factory.Instance.GetPartyManager().GetByName(model.Name);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(model);
        }

        public IHttpActionResult Post()
        {
            PartyModel toReturn;

            try
            {
                Party newParty = Factory.Instance.GetPartyManager().New();
                toReturn = new PartyModel()
                {
                    Id = newParty.Id,
                    Board = newParty.Board,
                    Name = newParty.PartLink,
                    WhiteTurn = newParty.WhiteTurn
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(toReturn);
        }
    }
}

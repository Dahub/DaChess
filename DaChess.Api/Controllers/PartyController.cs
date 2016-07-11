using DaChess.Api.Models;
using System.Web.Http;
using DaChess.Business;
using System;
using System.Web.Http.Cors;
using System.Net.Http;
using System.Linq;

namespace DaChess.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PartyController : ApiController
    {
        public IHttpActionResult Get(HttpRequestMessage request)
        {
            //   request.Get
            string partyName = request.GetQueryNameValuePairs().Where(k => k.Key.Equals("Name")).First().Value;
            PartyModel toReturn;

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
                return BadRequest(ex.Message);
            }

            return Ok(toReturn);
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(toReturn);
        }

        public IHttpActionResult Put(PartyModel model)
        {
            try
            {
                // TODO update 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(model);
        }
    }
}

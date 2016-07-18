using DaChess.Business;
using DaChess.WebApi.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace DaChess.WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PartyController : ApiController
    {
        public JsonResult<PartyModel> Get(HttpRequestMessage request)
        {
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
                    WhiteTurn = myParty.WhiteTurn,
                    IsError = false,
                    ErrorMessage = String.Empty
                };
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la récupération de la partie";
            }

            return Json(toReturn);
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
            catch (DaChessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("Erreur non gérée dans la création de la partie");
            }

            return Ok(toReturn);
        }
    }
}

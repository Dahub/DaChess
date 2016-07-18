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
        public JsonResult<PlayerModel> Get(string token, string partyName)
        {
            PlayerModel toReturn = new PlayerModel();

            try
            {
                toReturn.PartyName = partyName;
                Colors c = Factory.Instance.GetPlayerManager().GetPlayerColor(token, partyName);
                toReturn.IsBlack = c == Colors.BLACK;
                toReturn.IsWhite = c == Colors.WHITE;
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la récupération de la couleur du joueur";
            }

            return Json(toReturn);
        }

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
            catch (DaChessException ex)
            {
                party.IsError = true;
                party.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                party.IsError = true;
                party.ErrorMessage = "Erreur non gérée dans l'ajout du joueur à la partie";
            }

            return Json(party);
        }
    }
}
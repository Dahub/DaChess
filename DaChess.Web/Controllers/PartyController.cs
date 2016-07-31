using DaChess.Business;
using DaChess.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaChess.Web.Controllers
{
    public class PartyController : Controller
    {
        [HttpPost]
        public JsonResult NewParty()
        {
            PartyModel toReturn = new PartyModel();

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
                toReturn.ErrorMessage = ex.Message;
                toReturn.IsError = true;
            }
            catch (Exception)
            {
                toReturn.ErrorMessage = "Erreur non gérée dans la création de la partie";
                toReturn.IsError = true;
            }

            return Json(toReturn);
        }

        public JsonResult Get(string name)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party myParty = Factory.Instance.GetPartyManager().GetByName(name);
                toReturn = myParty.ToPartyModel();
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

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Start(string partyName)
        {
            return View((object)partyName);
        }

        [HttpPost]
        public JsonResult AddPlayerToParty(PartyModel party)
        {
            try
            {
             
                if (party.WhiteAskToPlay && String.IsNullOrEmpty(party.WhiteToken))
                {
                    var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.WHITE);
                    party.WhiteToken = myParty.WhiteToken;
                }
                else if (party.BlackAskToPlay && String.IsNullOrEmpty(party.BlackToken))
                {
                    var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.BLACK);
                    party.BlackToken = myParty.BlackToken;
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

        public JsonResult Resign(string name, string token)
        {
            ResignModel toReturn = new ResignModel();

            try
            {
                toReturn.InfoMessage = Factory.Instance.GetPartyManager().Resign(name, token);
                toReturn.IsError = false;
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée lors de l'abandon du joueur";
            }            

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Drawn(string name, string token)
        {
            DrawnModel toReturn = new DrawnModel();

            try
            {
                toReturn.InfoMessage = Factory.Instance.GetPartyManager().Drawn(name, token);
                toReturn.IsError = false;
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée lors de la demande de nulle du joueur";
            }

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MakeMove(string name, string move, string token)
        {
            MoveModel toReturn = new MoveModel();

            try
            {
                IBoardManager manager = Factory.Instance.GetBoardManager();
                toReturn.InfoMessage = manager.MakeMove(move, name, token);
                toReturn.Board = manager.ToJsonString();
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la l'application du coup";
            }

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MakePromote(string name, string piece, string token)
        {
            PromoteModel toReturn = new PromoteModel();

            try
            {
                IBoardManager manager = Factory.Instance.GetBoardManager();
                manager.PromotePiece(name, piece, token);
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la promotion";
            }
        

            return Json(toReturn, JsonRequestBehavior.AllowGet);
        }
    }
}
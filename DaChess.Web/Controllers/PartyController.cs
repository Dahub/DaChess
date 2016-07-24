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
        public JsonResult Post()
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

        [HttpGet]
        public JsonResult Get(string name)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party myParty = Factory.Instance.GetPartyManager().GetByName(name);

                toReturn = new PartyModel()
                {
                    Id = myParty.Id,
                    Board = myParty.Board,
                    Name = myParty.PartLink,
                    BlackToken = myParty.BlackLink,
                    WhiteToken = myParty.WhiteLink,
                    WhiteTurn = myParty.WhiteTurn,
                    History = myParty.History,
                    BlackIsCheck = myParty.BlackIsCheck.HasValue?myParty.BlackIsCheck.Value:false,
                    WhiteIsCheck = myParty.WhiteIsCheck.HasValue?myParty.WhiteIsCheck.Value:false,
                    BlackCanPromote = myParty.BlackCanPromote.HasValue?myParty.BlackCanPromote.Value:false,
                    WhiteCanPromote = myParty.WhiteCanPromote.HasValue?myParty.WhiteCanPromote.Value:false,    
                    BlackIsCheckMat = myParty.BlackIsCheckMat.HasValue?myParty.BlackIsCheckMat.Value:false,
                    WhiteIsCheckMat = myParty.WhiteIsCheckMat.HasValue?myParty.WhiteIsCheckMat.Value:false,
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
using DaChess.Business;
using DaChess.Web.Models;
using System;

namespace DaChess.Web
{
    internal static class ExtensionMethods
    {
        internal static PartyModel ToPartyModel(this Party p)
        {
            return new PartyModel()
            {
                Id = p.Id,
                Board = p.Board,
                Name = p.PartLink,
                BlackToken = p.BlackToken,
                WhiteToken = p.WhiteToken,
                WhiteTurn = p.WhiteTurn,
                History = p.History,
                BlackIsCheck = p.FK_Black_Player_Stat == (int)PlayerStateEnum.CHECK,
                WhiteIsCheck = p.FK_White_Player_Stat == (int)PlayerStateEnum.CHECK,
                BlackCanPromote = p.BlackCanPromote.HasValue ? p.BlackCanPromote.Value : false,
                WhiteCanPromote = p.WhiteCanPromote.HasValue ? p.WhiteCanPromote.Value : false,
                BlackIsCheckMat = p.FK_Black_Player_Stat == (int)PlayerStateEnum.CHECKMAT,
                WhiteIsCheckMat = p.FK_White_Player_Stat == (int)PlayerStateEnum.CHECKMAT,
                WhiteIsPat = p.FK_White_Player_Stat == (int)PlayerStateEnum.PAT,
                BlackIsPat = p.FK_Black_Player_Stat == (int)PlayerStateEnum.PAT,
                LastMoveCase = p.LastMoveCase,
                IsError = false,
                ErrorMessage = String.Empty,
                PartyOver = p.PartyOver
            };
        }
    }
}
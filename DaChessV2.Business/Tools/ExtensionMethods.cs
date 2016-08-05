using DaChessV2.Dto;
using System;

namespace DaChessV2.Business
{
    public static class ExtensionMethods
    {
        public static PartyModel ToPartyModel(this Party p)
        {
            long blackTimeLeft = 0;
            long whiteTimeLeft = 0;

            if(p.FK_PartyCadence != (int)EnumPartyCadence.NO_LIMIT)
            {
                if(p.BlackTimeLeftInLilliseconds.HasValue)
                {
                    if(p.LastMoveDate.HasValue && p.FK_Black_PlayerState == (int)EnumPlayerState.CAN_MOVE)
                    {
                        blackTimeLeft = p.BlackTimeLeftInLilliseconds.Value - (long)((DateTime.Now - p.LastMoveDate.Value).TotalMilliseconds);
                    }
                    else
                    {
                        blackTimeLeft = p.BlackTimeLeftInLilliseconds.Value;
                    }
                }
                if (p.WhiteTimeLeftInMilliseconds.HasValue)
                {
                    if (p.LastMoveDate.HasValue && p.FK_White_PlayerState == (int)EnumPlayerState.CAN_MOVE)
                    {
                        whiteTimeLeft = p.WhiteTimeLeftInMilliseconds.Value - (long)((DateTime.Now - p.LastMoveDate.Value).TotalMilliseconds);
                    }
                    else
                    {
                        whiteTimeLeft = p.WhiteTimeLeftInMilliseconds.Value;
                    }
                }
            }

            PartyModel toReturn = new PartyModel()
            {
                BlackPlayerState = (EnumPlayerState)p.FK_Black_PlayerState,
                Board = p.Board,
                History = p.JsonHistory,
                LastCase = p.LastMoveCase,
                Name = p.PartyName,
                PartyState = (EnumPartyState)p.FK_PartyState,
                WhitePlayerState = (EnumPlayerState)p.FK_White_PlayerState,
                BlackPlayerTimeLeft = blackTimeLeft,
                WhitePlayerTimeLeft = whiteTimeLeft,
                PartyCadence = (EnumPartyCadence)p.FK_PartyCadence
            };

            return toReturn;
        }
    }
}

using DaChessV2.Dto;

namespace DaChessV2.Business
{
    public static class ExtensionMethods
    {
        public static PartyModel ToPartyModel(this Party p)
        {
            PartyModel toReturn = new PartyModel()
            {
                BlackPlayerState = (EnumPlayerState)p.FK_Black_PlayerState,
                Board = p.Board,
                History = p.JsonHistory,
                LastCase = p.LastMoveCase,
                Name = p.PartyName,
                PartyState = (EnumPartyState)p.FK_PartyState,
                WhitePlayerState = (EnumPlayerState)p.FK_White_PlayerState
            };

            return toReturn;
        }
    }
}

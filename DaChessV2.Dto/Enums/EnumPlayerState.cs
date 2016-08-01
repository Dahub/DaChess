namespace DaChessV2.Dto
{
    public enum EnumPlayerState
    {
        UNDEFINED = 1,
        CAN_MOVE = 2,
        CAN_PROMOTE = 3,
        ASK_DRAWN = 4,
        PAT = 5,
        RESIGN = 6,
        CHECK = 7,
        CHECK_MAT = 8
    }
}

namespace DaChessV2.Dto
{
    public enum EnumPlayerState
    {
        UNDEFINED = 1,
        CAN_MOVE = 2,
        WAIT_HIS_TURN = 3,
        CAN_PROMOTE = 4,
        ASK_DRAWN = 5,
        PAT = 6,
        RESIGN = 7,
        CHECK = 8,
        CHECK_MAT = 9,
        ASK_TO_REPLAY = 10,
        TIME_OVER = 11
    }
}

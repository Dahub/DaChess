namespace DaChess.Business
{
    public interface IBoardManager
    {
        string ToJsonString();
        void Init(string jsonBoard);
        string MakeMove(string move, string partyName, string playerToken);
        void PromotePiece(string partyName, string choisedPiece, string playerToken);

    }
}

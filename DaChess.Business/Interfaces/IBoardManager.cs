namespace DaChess.Business
{
    public interface IBoardManager
    {
        string ToJsonString();
        void Init(string jsonBoard);
        void MakeMove(string move, string partyName, string playerToken);
    }
}

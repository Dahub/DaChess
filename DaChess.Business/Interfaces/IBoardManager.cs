namespace DaChess.Business
{
    public interface IBoardManager
    {
        string ToJsonString();
        void Init(string jsonBoard);
    }
}

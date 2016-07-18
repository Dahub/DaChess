namespace DaChess.Business
{
    public interface IPlayerManager
    {
        Colors GetPlayerColor(string token, string partyName);
    }
}

namespace DaChess.Business
{
    public interface IPartyManager
    {
        Party New();
        Party AddPlayerToParty(int partyId, Colors playerColor);
    }
}

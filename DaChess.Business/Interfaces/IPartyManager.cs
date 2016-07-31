﻿namespace DaChess.Business
{
    public interface IPartyManager
    {
        Party New();
        Party GetByName(string name);
        Party AddPlayerToParty(int partyId, Colors playerColor);
        Party Update(Party toUpdate);
        string Resign(string name, string token);
        string Drawn(string name, string token);
    }
}

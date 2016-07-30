using System;

namespace DaChess.Business
{
    internal class PlayerManager : IPlayerManager
    {
        public Colors GetPlayerColor(string token, string partyName)
        {
            int color = 0;

            try
            {
                Party p = PartyHelper.GetByName(partyName);
                string[] infos = CryptoHelper.Decrypt(token, p.Seed).Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                color = (Int32.Parse(infos[1]));
            }
            catch (Exception)
            {
                throw;
            }


            return (Colors)color;
        }
    }
}

using System;
using System.Linq;

namespace DaChess.Business
{
    internal static class PartyHelper
    {
        internal static Party GetByName(string name)
        {
            Party toReturn;

            using (var context = new ChessEntities())
            {
                toReturn = context.Parties.Where(p => p.PartLink.ToLower().Equals(name.ToLower())).FirstOrDefault();
            }

            if (toReturn == null)
                throw new DaChessException(String.Format("Partie {0} introuvable", name));

            return toReturn;
        }

        internal static Colors GetPlayerColor(Party party, string token)
        {
            string[] infos = CryptoHelper.Decrypt(token, party.Seed).Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int color;
            if(infos.Length != 2 || !Int32.TryParse(infos[1], out color))
            {
                throw new DaChessException("Token erroné, impossible d'extraire la couleur du joueur");
            }
            return (Colors)color;
        }

        internal static bool IsPlayerInParty(Party party, string token)
        {
            if (party.WhiteLink == token || party.BlackLink == token)
                return true;
            return false;
        }

        internal static bool IsPlayerInParty(string name, string token)
        {
            Party p = GetByName(name);
            return IsPlayerInParty(p, token);
        }
    }
}

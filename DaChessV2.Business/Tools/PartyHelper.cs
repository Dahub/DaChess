using DaChessV2.Dto;
using System;
using System.Linq;

namespace DaChessV2.Business
{
    internal static class PartyHelper
    {
        internal static Party GetByName(string name)
        {
            Party toReturn;

            using (var context = new ChessEntities())
            {
                toReturn = context.Party.Where(p => p.PartyName.ToLower().Equals(name.ToLower())).FirstOrDefault();
            }

            if (toReturn == null)
                throw new DaChessException(String.Format("Partie {0} introuvable", name));

            return toReturn;
        }


        internal static Color GetPlayerColor(Party party, string token)
        {
            string[] infos = CryptoHelper.Decrypt(token, party.Seed).Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int color;
            if (infos.Length != 2 || !Int32.TryParse(infos[1], out color))
            {
                throw new DaChessException("Token erroné, impossible d'extraire la couleur du joueur");
            }
            return (Color)color;
        }

        internal static bool IsPlayerInParty(Party party, string token)
        {
            if (party.WhiteToken == token || party.BlackToken == token)
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

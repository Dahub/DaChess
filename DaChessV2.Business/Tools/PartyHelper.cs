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
                throw new DaChessException(String.Format("Partie {0} introuvable", name), true);

            return toReturn;
        }

        internal static Color GetPlayerColor(Party party, string token)
        {
            string[] infos = CryptoHelper.Decrypt(token, party.Seed).Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int color;
            if (infos.Length != 2 || !Int32.TryParse(infos[1], out color))
            {
                throw new DaChessException(
                    String.Format("Partie {0} : Token erroné, impossible d'extraire la couleur du joueur", party.PartyName), true);
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

        internal static History UpdateHistorique(string move, Party party, Color playerColor, EnumPartyState partyState)
        {
            History histo;
            if (String.IsNullOrEmpty(party.JsonHistory))
            {
                histo = new History();
            }
            else
            {
                histo = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(party.JsonHistory);
            }

            int moveNumber = 0;
            if (histo.Moves.Count() > 0)
            {
                moveNumber = histo.Moves.OrderByDescending(h => h.Key).First().Key;
            }
            if (playerColor == Color.WHITE)
            {
                histo.Moves.Add(moveNumber + 1, move);
            }
            else
            {
                if (histo.Moves.Count == 0) // cas de l'abandon des blancs avant le premier coup
                    histo.Moves.Add(1, String.Empty);
                else
                    histo.Moves[moveNumber] += " " + move;
            }

            // on vérifie si la partie est terminée
            if (partyState == EnumPartyState.DRAWN)
            {
                if(histo.Moves.ContainsKey(moveNumber + 1))
                    histo.Moves[moveNumber + 1] += " 1/2-1/2";
                else
                    histo.Moves[moveNumber] += " 1/2-1/2";
            }           
            else if (partyState == EnumPartyState.OVER_BLACK_WIN)
            {
                histo.Moves[moveNumber + 1] += " 0-1";
            }
            else if (partyState == EnumPartyState.OVER_WHITE_WIN)
            {
                if (moveNumber == 0) // cas de l'abandon des blancs avant le premier coup
                    histo.Moves[1] += " 1-0";
                else
                    histo.Moves[moveNumber] += " 0-1";
            }

            return histo;
        }
    }
}

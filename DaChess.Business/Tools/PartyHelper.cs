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
    }
}

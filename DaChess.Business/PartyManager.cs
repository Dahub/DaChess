using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaChess.Business
{
    internal class PartyManager : IPartyManager
    {
        public Party New()
        {
            Party toReturn = null;

            using(ChessEntities context = new ChessEntities())
            {
                toReturn = new Party()
                {
                    Seed = Guid.NewGuid().ToString(),
                    WhiteTurn = true,
                    BoardType = Boards.Classic(context)
                };

                toReturn.White = context.Players.Where(p => p.Id.Equals(1)).First();
                toReturn.Black = context.Players.Where(p => p.Id.Equals(2)).First();
                toReturn.Board = toReturn.BoardType.Content;
                context.Parties.Add(toReturn);
             
                context.SaveChanges();
            }
            // générer le token unique de partie

    

            // génrer le token associé à cette partie pour chaque joueur

            return toReturn;
        }
    }
}

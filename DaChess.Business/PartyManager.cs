using System;
using System.Linq;

namespace DaChess.Business
{
    internal class PartyManager : IPartyManager
    {
        private const int NUMBER_OF_TRY = 200;

        /// <summary>
        /// Build a new party with an unique name 
        /// </summary>
        /// <returns></returns>
        public Party New()
        {
            Party toReturn = null;

            using (ChessEntities context = new ChessEntities())
            {
                int count = 0;

                do
                {
                    toReturn = InitParty(context);
                    count++;
                    if (count > NUMBER_OF_TRY)
                        throw new Exception("Can not create a new Party");
                } while (context.Parties.Where(p => p.PartLink.Equals(toReturn.PartLink)).FirstOrDefault() != null);

                context.Parties.Add(toReturn);
                context.SaveChanges();
            }

            return toReturn;
        }

        public Party AddPlayerToParty(int partyId, Colors playerColor)
        {
            Party toReturn;

            using (ChessEntities context = new ChessEntities())
            {
                toReturn = context.Parties.Where(p => p.Id.Equals(partyId)).FirstOrDefault();
                if (toReturn == null)
                    throw new Exception(String.Format("Can not fin party {0}", partyId));

                // on génère le lien
                string infos = String.Format("{0}#;#{1}", partyId, (int)playerColor);
                string link = CryptoHelper.Encrypt(infos, toReturn.Seed);

                switch (playerColor)
                {
                    case Colors.BLACK:
                        if (String.IsNullOrEmpty(toReturn.BlackLink))
                        {
                            toReturn.BlackLink = link;
                        }
                        break;
                    case Colors.WHITE:
                        if (String.IsNullOrEmpty(toReturn.WhiteLink))
                        {
                            toReturn.WhiteLink = link;
                        }
                        break;
                }

                context.SaveChanges();
            }

            return toReturn;
        }

        private static Party InitParty(ChessEntities context)
        {
            Party toReturn = new Party()
            {
                Seed = Guid.NewGuid().ToString(),
                WhiteTurn = true,
                BoardType = Boards.Classic(context),
                PartLink = DaTools.NameMaker.Creator.Build(DaTools.NameMaker.NameType.FIRST_NAMES, DaTools.NameMaker.NameType.FIRST_NAMES),
            };
            toReturn.Board = toReturn.BoardType.Content;
            return toReturn;
        }
    }
}


using System;
using System.Linq;

namespace DaChess.Business
{
    internal class PartyManager : IPartyManager
    {
        private const int NUMBER_OF_TRY = 200;

        public Party GetByName(string name)
        {
            return PartyHelper.GetByName(name);
        }

        public Party Update(Party toUpdate)
        {
            try
            {
                using (var context = new ChessEntities())
                {
                    context.Entry(toUpdate).State =
                        System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
            }
            catch (DaChessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaChessException("Erreur dans la mise à jour de la partie", ex);
            }

            return toUpdate;
        }

        /// <summary>
        /// Build a new party with an unique name 
        /// </summary>
        /// <returns></returns>
        public Party New()
        {
            Party toReturn = null;

            try
            {
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
            }
            catch (DaChessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaChessException("Erreur dans la création de la partie", ex);
            }

            return toReturn;
        }

        public Party AddPlayerToParty(int partyId, Colors playerColor)
        {
            Party toReturn;

            try
            {
                using (ChessEntities context = new ChessEntities())
                {
                    toReturn = context.Parties.Where(p => p.Id.Equals(partyId)).FirstOrDefault();
                    if (toReturn == null)
                        throw new DaChessException(String.Format("Impossible de trouver la partie d'id {0}", partyId));

                    if (!String.IsNullOrEmpty(toReturn.BlackToken) && playerColor == Colors.BLACK)
                        throw new DaChessException("La partie a déjà un jouer noir inscrit");

                    if (!String.IsNullOrEmpty(toReturn.WhiteToken) && playerColor == Colors.WHITE)
                        throw new DaChessException("La partie a déjà un jouer blanc inscrit");

                    // on génère le lien
                    string infos = String.Format("{0}#;#{1}", partyId, (int)playerColor);
                    string link = CryptoHelper.Encrypt(infos, toReturn.Seed);

                    switch (playerColor)
                    {
                        case Colors.BLACK:
                            if (String.IsNullOrEmpty(toReturn.BlackToken))
                            {
                                toReturn.BlackToken = link;
                            }
                            break;
                        case Colors.WHITE:
                            if (String.IsNullOrEmpty(toReturn.WhiteToken))
                            {
                                toReturn.WhiteToken = link;
                            }
                            break;
                    }

                    context.SaveChanges();
                }
            }
            catch (DaChessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaChessException("Erreur dans l'ajout du joueur à la partie", ex);
            }

            return toReturn;
        }

        private static Party InitParty(ChessEntities context)
        {
            Party toReturn;

            toReturn = new Party()
            {
                Seed = Guid.NewGuid().ToString(),
                CreationDate = DateTime.Now,
                WhiteTurn = true,
                BoardType = BoardsHelper.GetClassic(context),
                PartLink = DaTools.NameMaker.Creator.Build(DaTools.NameMaker.NameType.FIRST_NAMES, DaTools.NameMaker.NameType.FIRST_NAMES),
                FK_Black_Player_Stat = (int)PlayerStateEnum.UNDEFINE,
                FK_White_Player_Stat = (int)PlayerStateEnum.UNDEFINE,
                LastMoveCase = String.Empty
            };
            toReturn.Board = toReturn.BoardType.Content;

            return toReturn;
        }
    }
}


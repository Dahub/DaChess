using DaChessV2.Dto;
using System;
using System.Linq;

namespace DaChessV2.Business
{
    /// <summary>
    /// Cette classe regroupe les méthodes qui permettent de faire vivre la partie, depuis
    /// sa création jusqu'aux actions des joueurs
    /// </summary>
    public class PartyManager
    {
        private const int NUMBER_OF_TRY = 200; // nombre de tentative pour trouver un nom de partie inutilisé      

        /// <summary>
        /// Création d'une nouvelle partie
        /// On va tenter de trouver un nom de partie inutilisé, tant qu'on n'en a pas trouvé, on retente
        /// Une fois ce dernier trouvé, on sauvegarde la partie et on transforme le résultat
        /// en PartyModel qui sera retourné à l'appellant
        /// </summary>
        /// <returns>Objet PartyModel avec les informations de la partie crée</returns>
        public PartyModel CreateNewParty()
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party myParty;
                using (ChessEntities context = new ChessEntities())
                {
                    int count = 0;

                    do
                    {
                        myParty = InitParty(context);
                        count++;
                        if (count > NUMBER_OF_TRY)
                            throw new Exception("Impossible de trouve un nom de partie inutilisé");
                    } while (context.Party.Where(p => p.PartyName.Equals(myParty.PartyName)).FirstOrDefault() != null);

                    context.Party.Add(myParty);
                    context.SaveChanges();
                }

                toReturn = myParty.ToPartyModel();
                toReturn.ResultText = "Partie crée avec succès";
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur non gérée dans la création de la partie";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        /// <summary>
        /// Retrouve une partie à partir de son nom
        /// Si aucune partie n'existe sous ce nom, une exception est lancée.
        /// Sinon, on retourne un objet PartyModel
        /// </summary>
        /// <param name="partyName">Nom de la partie</param>
        /// <returns>Objet PartyModel avec les informations de la partie demandée</returns>
        public PartyModel GetParty(string partyName)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                if (String.IsNullOrEmpty(partyName))
                    throw new DaChessException("Impossible de trouver une partie sans nom");

                Party p = PartyHelper.GetByName(partyName);
                toReturn = p.ToPartyModel();
                toReturn.ResultText = String.Format("Partie {0} récupérée", partyName);
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur non gérée dans la création de la partie";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        /// <summary>
        /// Ajoute un joueur à la partie comme joueur de la couleur désignée
        /// Un token sera généré avec la forme suivante  [id partie]#;#[couleur du joueur]
        /// et est ensuite crypté à partir du "seed" de la partie.
        /// Ce token sera retourné au client et stocké dans la page et dans un cookie pour être utilisé
        /// ultérieurement à des fins d'authentification et aussi lors des déplacements, pour vérifier
        /// que le joueur a bien le droit de jouer la partie.
        /// </summary>
        /// <param name="playerColor">Couleur demandée</param>
        /// <param name="partyName">Nom de la partie concernée</param>
        /// <returns>Un objet PlayerModel contenant le token de l'utilisateur</returns>
        public PlayerModel AddPlayerToParty(Color playerColor, string partyName)
        {
            PlayerModel toReturn = new PlayerModel();

            try
            {
                Party p = PartyHelper.GetByName(partyName);

                // vérification qu'il n'existe pas déjà un joueur pour cette partie à la couleur demandée
                if (playerColor == Color.BLACK && p.FK_Black_PlayerState != (int)EnumPlayerState.UNDEFINED)
                    throw new DaChessException("Cette partie a déjà un joueur pour la couleur noire");

                if (playerColor == Color.WHITE && p.FK_White_PlayerState != (int)EnumPlayerState.UNDEFINED)
                    throw new DaChessException("Cette partie a déjà un joueur pour la couleur blanche");

                // on génère le token de la forme suivant [id partie]#;#[couleur du joueur]
                string infos = String.Format("{0}#;#{1}", p.Id, (int)playerColor);
                string token = CryptoHelper.Encrypt(infos, p.Seed);

                switch (playerColor)
                {
                    case Color.BLACK:                       
                        p.BlackToken = token;
                        p.FK_Black_PlayerState = (int)EnumPlayerState.WAIT_HIS_TURN;
                        break;
                    case Color.WHITE:
                        p.WhiteToken = token;
                        p.FK_White_PlayerState = (int)EnumPlayerState.WAIT_HIS_TURN;
                        break;
                }

                // si la partie possède 2 joueurs, elle est prête
                if(p.FK_Black_PlayerState != (int)EnumPlayerState.UNDEFINED && p.FK_White_PlayerState != (int)EnumPlayerState.UNDEFINED)
                {
                    p.FK_PartyState = (int)EnumPartyState.RUNNING;
                    p.FK_White_PlayerState = (int)EnumPlayerState.CAN_MOVE; // la partie peut commencer, au tour des blancs
                }

                this.Update(p);

                toReturn.PlayerColor = playerColor;
                toReturn.Token = token;
                toReturn.ResultText = String.Format("Le jouer a bien rejoins la partie {0} récupérée", partyName);
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur non gérée dans la création de la partie";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        public PartyModel MakeMove(string move, string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                // on récupère la couleur du joueur et sa partie

                // si il est bien de cette partie et si la pièce à bouger est bien de la bonne couleur, on continue
                // sinon exception

                toReturn.ResultText = "Déplacement terminé";

                throw new DaChessException("Méthode à implémenter");
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = String.Format("Erreur non gérée dans le déplacement {0}", move);
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        public PartyModel PromotePiece(string partyName, EnumPieceType choisedPiece, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                toReturn.ResultText = "Promotion terminée";

                throw new DaChessException("Méthode à implémenter");
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur non gérée dans la promotion de la pièce";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        private Party Update(Party toUpdate)
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

        private Party InitParty(ChessEntities context)
        {
            Party toReturn;

            try
            {
                toReturn = new Party()
                {
                    Seed = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    BoardType = BoardHelper.GetClassic(context),
                    PartyName = DaTools.NameMaker.Creator.Build(DaTools.NameMaker.NameType.FIRST_NAMES, DaTools.NameMaker.NameType.FIRST_NAMES),
                    FK_Black_PlayerState = (int)EnumPlayerState.UNDEFINED,
                    FK_White_PlayerState = (int)EnumPlayerState.UNDEFINED,
                    FK_PartyState = (int)EnumPartyState.WAIT_FOR_PLAYER,
                    LastMoveCase = String.Empty,
                    BlackToken = String.Empty,
                    WhiteToken = String.Empty,
                    EnPassantCase = String.Empty,
                    JsonHistory = String.Empty
                };
                toReturn.Board = toReturn.BoardType.Content;
            }
            catch (Exception ex)
            {
                throw new DaChessException("Une erreur s'est produite lors de l'initialisation de la nouvelle partie", ex);
            }

            return toReturn;
        }      
    }
}

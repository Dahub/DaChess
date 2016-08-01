using DaChessV2.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace DaChessV2.Business
{
    /// <summary>
    /// Cette classe regroupe les méthodes qui permettent de faire vivre la partie, depuis
    /// sa création jusqu'aux actions des joueurs
    /// </summary>
    public class PartyManager
    {
        private const int NUMBER_OF_TRY = 200; // nombre de tentative pour trouver un nom de partie inutilisé    
        private const int BOARD_CASE = 8; // nombre de cases du plateau de jeu

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

        /// <summary>
        /// Le joueur effectue un déplacement
        /// Si tout se passe bien, un objet PartyModel contenant la disposition des pièces
        /// après déplacement est retourné.
        /// Si le joueur de token passé en argument ne joue pas cette partie, une exception est lancée
        /// </summary>
        /// <param name="move">le déplacement de la forme [case départ] [case arrivée] exemple : "e4 e5"</param>
        /// <param name="partyName">le nom de la partie</param>
        /// <param name="playerToken">le token du joueur</param>
        /// <returns>un objet PartyModel à jour</returns>
        public PartyModel MakeMove(string move, string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                // on récupère la couleur du joueur et sa partie
                Party party = PartyHelper.GetByName(partyName);
                toReturn = party.ToPartyModel();
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);

                // on transforme le board json en tableau de cases [line][colonne]
                CaseInfo[][] boardCases = BoardHelper.ExtractCasesInfos(party.Board, BOARD_CASE);

                // on récupère les 2 cases du déplacement, et on lance une exception si ce n'est pas cohérent
                move = move.TrimEnd(' ');
                string[] moveCases = move.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (!BoardHelper.IsMoveOk(moveCases))
                    throw new DaChessException(String.Format("Le coup {0} est mal formaté", move));

                // on récupère la première pièce              
                string enPassant = String.Empty;
                string lastMoveCase = String.Empty;
                bool promotePawn = false;
                History histo = null;

                int startLine = Int32.Parse(moveCases[0].Substring(1, 1)) - 1;
                int startCol = BoardHelper.ColToInt(moveCases[0].Substring(0, 1)) - 1;

                if (boardCases[startLine][startCol].Piece.HasValue)
                {
                    int endLine = Int32.Parse(moveCases[1].Substring(1, 1)) - 1;
                    int endCol = BoardHelper.ColToInt(moveCases[1].Substring(0, 1)) - 1;
                    EnumMoveType mt;

                    if (!BoardHelper.IsLegalMove(boardCases[startLine][startCol], boardCases[endLine][endCol], boardCases, startLine, endLine, startCol, endCol, party, out mt))
                    {
                        throw new DaChessException("Coup illégal");
                    }
                }





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

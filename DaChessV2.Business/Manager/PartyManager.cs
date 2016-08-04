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
        private const int BOARD_CASE = 8; // nombre de cases du plateau de jeu

        /// <summary>
        /// Création d'une nouvelle partie avec les paramètres passés en option :
        /// Le type de cadence, les temps et le temps fischer
        /// On va tenter de trouver un nom de partie inutilisé, tant qu'on n'en a pas trouvé, on retente
        /// Une fois ce dernier trouvé, on sauvegarde la partie et on transforme le résultat
        /// en PartyModel qui sera retourné à l'appellant
        /// </summary>
        /// <returns>Objet PartyModel avec les informations de la partie crée</returns>
        public PartyModel CreateNewParty(PartyOptionModel optionModel)
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

                    myParty.FK_PartyCadence = (int)optionModel.ChoisedCadenceType;
                    myParty.PartyTimeInSeconds = optionModel.TimeInSeconds;
                    myParty.PartyFischerInSeconds = optionModel.FischerInSeconds;
                    myParty.WhiteTimeLeftInMilliseconds = optionModel.TimeInSeconds * 1000;
                    myParty.BlackTimeLeftInLilliseconds = optionModel.TimeInSeconds * 1000;

                    context.Party.Add(myParty);
                    context.SaveChanges();

                    context.PartyHistory.Add(new PartyHistory()
                    {
                        Board = BoardHelper.ToBoardDescription(myParty.Board, BOARD_CASE),
                        FK_Party = myParty.Id,
                        DateCreation = DateTime.Now
                    });

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
        /// indique que le temps du joueur token est écoulé.
        /// Cette méthode est appellée depuis le client, en se basant sur un compteur javascript
        /// </summary>
        /// <param name="partyName">nom de la partie concernée</param>
        /// <param name="token">token du joueur concerné</param>
        /// <returns>un objet PartyModel mis à jour</returns>
        public PartyModel TimeOver(string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                string infoMsg = String.Empty;
                bool updateParty = false;

                long timeUse = CalcTimeUseForMove(party);
                EnumPartyState partyState = EnumPartyState.RUNNING;

                switch (playerColor)
                {
                    case Color.WHITE:
                        if (party.FK_Black_PlayerState != (int)EnumPlayerState.TIME_OVER)
                        {
                            party.FK_White_PlayerState = (int)EnumPlayerState.TIME_OVER;
                            partyState = EnumPartyState.OVER_BLACK_WIN;
                            infoMsg = "Temps du joueur blanc écoulé";
                            updateParty = true;
                        }
                        break;
                    case Color.BLACK:
                        if (party.FK_White_PlayerState != (int)EnumPlayerState.TIME_OVER)
                        {
                            party.FK_Black_PlayerState = (int)EnumPlayerState.TIME_OVER;
                            partyState = EnumPartyState.OVER_WHITE_WIN;
                            infoMsg = "Temps du joueur noir écoulé";
                            updateParty = true;
                        }
                        break;
                }

                if (updateParty)
                {
                    History histo = PartyHelper.UpdateHistorique(String.Empty, party, playerColor, partyState);
                    party.JsonHistory = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                    party.FK_PartyState = (int)partyState;
                    Update(party);
                }

                toReturn = party.ToPartyModel();
                toReturn.ResultText = infoMsg;
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
                toReturn.ErrorMsg = "Erreur non gérée dans l'enregistrement du dépassement du temps du joueur";
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
                if (p.FK_Black_PlayerState != (int)EnumPlayerState.UNDEFINED && p.FK_White_PlayerState != (int)EnumPlayerState.UNDEFINED)
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
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);

                // on transforme le board json en tableau de cases [line][colonne]
                CaseInfo[][] boardCases = BoardHelper.ToCaseInfoFromJsonString(party.Board, BOARD_CASE);

                // on récupère les 2 cases du déplacement, et on lance une exception si ce n'est pas cohérent
                move = move.TrimEnd(' ');
                string[] moveCases = move.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (!BoardHelper.IsMoveSyntaxeOk(moveCases))
                    throw new DaChessException(String.Format("Le coup {0} est mal formaté", move));

                // on récupère la première pièce              
                string enPassant = String.Empty;
                string lastMoveCase = String.Empty;
                History histo = null;

                int startLine = Int32.Parse(moveCases[0].Substring(1, 1)) - 1;
                int startCol = BoardHelper.ColToInt(moveCases[0].Substring(0, 1)) - 1;

                string resultText = String.Empty;

                EnumPlayerState ennemiState = EnumPlayerState.CAN_MOVE;
                EnumPlayerState playerState = EnumPlayerState.WAIT_HIS_TURN;
                EnumPartyState partyState = EnumPartyState.RUNNING;
                EnumMoveType mt = EnumMoveType.ILLEGAL;
                if (boardCases[startLine][startCol].Piece.HasValue)
                {
                    int endLine = Int32.Parse(moveCases[1].Substring(1, 1)) - 1;
                    int endCol = BoardHelper.ColToInt(moveCases[1].Substring(0, 1)) - 1;

                    mt = BoardHelper.GetMoveType(boardCases, new Coord(startLine, startCol), new Coord(endLine, endCol), party.EnPassantCase);
                    if (mt == EnumMoveType.ILLEGAL)
                        throw new DaChessException("Coup illégal");

                    // on stocke la case d'arrivée
                    lastMoveCase = String.Concat(BoardHelper.IntToCol(endCol + 1), endLine + 1);
                    move = MoveNotationHelper.BuildMoveNotation(move, mt, boardCases[startLine][startCol], new Coord(startLine, startCol), new Coord(endLine, endCol), boardCases);
                    switch (mt) // gestion de l'après déplacement
                    {
                        case EnumMoveType.EN_PASSANT:
                            string epCol = party.EnPassantCase.Substring(0, 1);
                            string epLine = party.EnPassantCase.Substring(1, 1);
                            int epColInt = BoardHelper.ColToInt(epCol) - 1;
                            int epLineInt = Int32.Parse(epLine) - 1;
                            boardCases[epLineInt][epColInt].HasMove = null;
                            boardCases[epLineInt][epColInt].Piece = null;
                            boardCases[epLineInt][epColInt].PieceColor = null;
                            resultText = "Prise en passant !";
                            break;
                        case EnumMoveType.CASTLING_SHORT:
                            boardCases[startLine][endCol - 1].HasMove = true;
                            boardCases[startLine][endCol - 1].Piece = EnumPieceType.ROOK;
                            boardCases[startLine][endCol - 1].PieceColor = boardCases[startLine][boardCases[startLine].Length - 1].PieceColor;
                            boardCases[startLine][boardCases[startLine].Length - 1].HasMove = null;
                            boardCases[startLine][boardCases[startLine].Length - 1].Piece = null;
                            boardCases[startLine][boardCases[startLine].Length - 1].PieceColor = null;
                            resultText = "Petit roque";
                            break;
                        case EnumMoveType.CASTLING_LONG:
                            boardCases[startLine][endCol + 1].HasMove = true;
                            boardCases[startLine][endCol + 1].Piece = EnumPieceType.ROOK;
                            boardCases[startLine][endCol + 1].PieceColor = boardCases[startLine][0].PieceColor;
                            boardCases[startLine][0].HasMove = null;
                            boardCases[startLine][0].Piece = null;
                            boardCases[startLine][0].PieceColor = null;
                            resultText = "Grand roque";
                            break;
                        case EnumMoveType.PROMOTE:
                            playerState = EnumPlayerState.CAN_PROMOTE;
                            ennemiState = EnumPlayerState.WAIT_HIS_TURN;
                            resultText = "Promotion du pion";
                            if (boardCases[endLine][endCol].Piece.HasValue)
                                move = move.Replace(" ", "x");
                            break;
                    }

                    // pour la gestion de la prise en passant
                    if (boardCases[startLine][startCol].Piece == EnumPieceType.PAWN && Math.Abs(startLine - endLine) == 2) // un pion qui a bougé de 2 cases
                    {
                        enPassant = moveCases[1]; // case d'arrivée
                    }

                    boardCases[endLine][endCol].HasMove = true;
                    boardCases[endLine][endCol].Piece = boardCases[startLine][startCol].Piece;
                    boardCases[endLine][endCol].PieceColor = boardCases[startLine][startCol].PieceColor;
                    boardCases[startLine][startCol].HasMove = null;
                    boardCases[startLine][startCol].Piece = null;
                    boardCases[startLine][startCol].PieceColor = null;
                }
                else
                {
                    throw new DaChessException("Coup illégal, aucune pièce sur la case de départ");
                }

                string newResultText;
                ennemiState = BoardHelper.DefineEnnemiState(out newResultText, out move, move, playerColor, boardCases, ennemiState);
                if (!String.IsNullOrEmpty(newResultText)) // on met à jour le résultat si il y a eu changement
                    resultText = newResultText;
                partyState = RefreshPartyState(playerColor, ennemiState, partyState);

                if (BoardHelper.IsCheck(playerColor, boardCases))
                    throw (new DaChessException("Coup impossible, échec !"));

                // on sauvegarde
                using (var context = new ChessEntities())
                {
                    PartyHistory partHist = new PartyHistory()
                    {
                        Board = BoardHelper.ToBoardDescription(boardCases),
                        FK_Party = party.Id,
                        DateCreation = DateTime.Now
                    };
                    context.PartyHistory.Add(partHist);
                    context.SaveChanges();
                }

                // vérification qu'on n'ai pas 3 fois la même position
                if (BoardHelper.IsTreeTimeSamePosition(BoardHelper.ToBoardDescription(boardCases), party.Id))
                {
                    // partie nulle
                    resultText = "3 fois la même position, partie nulle";
                    partyState = EnumPartyState.DRAWN;
                }

                // Gestion des temps, vérification que le temps du joueur n'est pas écoulé
                if (mt != EnumMoveType.PROMOTE)
                {
                    ManagePlayerTime(party, playerColor, ref resultText, ref playerState, ref partyState);
                }

                // mise à jour de l'historique        
                histo = PartyHelper.UpdateHistorique(move, party, playerColor, partyState);

                party.Board = BoardHelper.ToJsonStringFromCaseInfo(boardCases);
                party.FK_Black_PlayerState = (int)(playerColor == Color.WHITE ? ennemiState : playerState);
                party.FK_White_PlayerState = (int)(playerColor == Color.BLACK ? ennemiState : playerState);
                party.JsonHistory = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                party.EnPassantCase = enPassant;
                party.LastMoveCase = lastMoveCase;
                party.FK_PartyState = (int)partyState;

                Update(party);
                toReturn = party.ToPartyModel();
                toReturn.ResultText = resultText;
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

        /// <summary>
        /// Modifie la dernière pièce à avoir bougé en pièce choisie
        /// Cett méthode vérifie aussi que la promotion ne provoque pas un échec ou un mat
        /// </summary>
        /// <param name="partyName">Nom de la partie concernée</param>
        /// <param name="choisedPiece">Pièce choisie pour la cible de la promotion</param>
        /// <param name="playerToken">token du joueur</param>
        /// <returns>un objet PartyModel mis à jour</returns>
        public PartyModel PromotePiece(string partyName, string choisedPiece, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                CaseInfo[][] boardCases = BoardHelper.ToCaseInfoFromJsonString(party.Board, BOARD_CASE);

                int limitLine = 0;
                if (playerColor == Color.WHITE) // uniquement les lignes de fond de plateau
                    limitLine = boardCases.Length - 1;

                for (int col = 0; col < boardCases[limitLine].Length; col++)
                {
                    if (boardCases[limitLine][col].Piece.HasValue
                        && boardCases[limitLine][col].Piece.Value == EnumPieceType.PAWN
                        && boardCases[limitLine][col].PieceColor == playerColor)
                    {
                        boardCases[limitLine][col].Piece = BoardHelper.GetPieceType(choisedPiece);
                        break;
                    }
                }

                // mise à jour de l'historique
                History histo = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(party.JsonHistory);
                int lastMove = histo.Moves.Select(m => m.Key).Max();
                string toAdd = String.Empty;
                switch (BoardHelper.GetPieceType(choisedPiece))
                {
                    case EnumPieceType.BISHOP:
                        toAdd = "=B";
                        break;
                    case EnumPieceType.KNIGHT:
                        toAdd = "=N";
                        break;
                    case EnumPieceType.ROOK:
                        toAdd = "=R";
                        break;
                    case EnumPieceType.QUEEN:
                        toAdd = "=Q";
                        break;
                }
                histo.Moves[lastMove] += toAdd;

                EnumPlayerState playerState = EnumPlayerState.WAIT_HIS_TURN;
                EnumPlayerState ennemiState = EnumPlayerState.CAN_MOVE;
                ennemiState = BoardHelper.DefineEnnemiState(playerColor, boardCases, ennemiState);
                EnumPartyState partyState = EnumPartyState.RUNNING;
                partyState = RefreshPartyState(playerColor, ennemiState, partyState);

                string resultText = "Promotion terminée";
                ManagePlayerTime(party, playerColor, ref resultText, ref playerState, ref partyState);

                party.Board = BoardHelper.ToJsonStringFromCaseInfo(boardCases);
                party.FK_Black_PlayerState = (int)(playerColor == Color.WHITE ? ennemiState : playerState);
                party.FK_White_PlayerState = (int)(playerColor == Color.BLACK ? ennemiState : playerState);
                party.JsonHistory = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                party.FK_PartyState = (int)partyState;

                Update(party);

                toReturn = party.ToPartyModel();
                toReturn.ResultText = resultText;
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

        /// <summary>
        /// Le joueur possédant le token playerToken abandonne la partie.
        /// La partie est automatiquement marquée comme gagnée par son adversaire
        /// </summary>
        /// <param name="partyName">Nom de la partie concernée</param>
        /// <param name="playerToken">Token du joueur qui abandonne</param>
        /// <returns>Un objet PartyModel mis à jour</returns>
        public PartyModel Resign(string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);

                EnumPartyState partyState = EnumPartyState.RUNNING;
                if (playerColor == Color.WHITE)
                    partyState = EnumPartyState.OVER_BLACK_WIN;
                else
                    partyState = EnumPartyState.OVER_WHITE_WIN;
                History histo = PartyHelper.UpdateHistorique(String.Empty, party, playerColor, partyState);

                party.JsonHistory = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                party.FK_PartyState = (int)partyState;

                string infoMsg = String.Empty;
                if (playerColor == Color.WHITE)
                {
                    infoMsg = "Le joueur blanc abandonne";
                    party.FK_White_PlayerState = (int)EnumPlayerState.RESIGN;
                }
                else
                {
                    infoMsg = "Le joueur noir abandonne";
                    party.FK_Black_PlayerState = (int)EnumPlayerState.RESIGN;
                }

                Update(party);

                toReturn = party.ToPartyModel();
                toReturn.ResultText = infoMsg;
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
                toReturn.ErrorMsg = "Erreur non gérée dans l'abandon du joueur";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        /// <summary>
        /// Un joueur a accepté la demande de nulle
        /// Si les deux joueurs ont accepté (information stockée dans les Playerstates de la partie,
        /// la partie est annulée
        /// </summary>
        /// <param name="partyName">Nom de la partie concernée</param>
        /// <param name="playerToken">Token du joueur qui abandonne</param>
        /// <returns>Un objet PartyModel mis à jour</returns>
        public PartyModel Drawn(string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                string infoMsg = String.Empty;

                if (playerColor == Color.WHITE)
                {
                    party.FK_White_PlayerState = (int)EnumPlayerState.ASK_DRAWN;
                }
                else
                {
                    party.FK_Black_PlayerState = (int)EnumPlayerState.ASK_DRAWN;
                }

                // on vérifie si les deux joueurs ont annulé, si oui, partie terminée
                if (party.FK_Black_PlayerState == (int)EnumPlayerState.ASK_DRAWN && party.FK_White_PlayerState == (int)EnumPlayerState.ASK_DRAWN)
                {
                    infoMsg = "Les deux joueurs acceptent la nulle";
                    History histo = PartyHelper.UpdateHistorique(String.Empty, party, playerColor == Color.BLACK ? Color.WHITE : Color.BLACK, EnumPartyState.DRAWN);
                    party.JsonHistory = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                    party.FK_PartyState = (int)EnumPartyState.DRAWN;
                }

                Update(party);

                toReturn = party.ToPartyModel();
                toReturn.ResultText = infoMsg;
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
                toReturn.ErrorMsg = "Erreur non gérée dans la demande de nulle du joueur";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        /// <summary>
        /// Appellée lorsqu'un joueur propose de rejouer une autre partie ou lorsqu'un joueur accepte
        /// la proposition de joueur une autre partie.
        /// Le PlayerState du joueur de la couleur correspondante est mis à jour
        /// Si le joueur avait déjà fait une demande avant, une exception est lancée
        /// Si les deux joueurs sont dans l'état "demande de rejouer", alors une nouvelle partie est générée
        /// et est retournée dans l'objet PartyModel
        /// </summary>
        /// <param name="partyName">Le nom de la partie actuelle</param>
        /// <param name="playerToken">Le joueur faisant la demande ou acceptant la demande</param>
        /// <returns>Un objet PartyModel contenant la partie ou la nouvelle partie</returns>
        public PartyModel Replay(string partyName, string playerToken)
        {
            PartyModel toReturn = new PartyModel();

            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Color playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                string infoMsg = String.Empty;

                if (playerColor == Color.WHITE && party.FK_White_PlayerState == (int)EnumPlayerState.ASK_TO_REPLAY
                    || playerColor == Color.BLACK && party.FK_Black_PlayerState == (int)EnumPlayerState.ASK_TO_REPLAY)
                {
                    throw new DaChessException("Vous avez déjà proposé de rejouer");
                }

                if (playerColor == Color.WHITE)
                {
                    party.FK_White_PlayerState = (int)EnumPlayerState.ASK_TO_REPLAY;
                    infoMsg = "Le joueur blanc propose une autre partie";
                }
                else
                {
                    party.FK_Black_PlayerState = (int)EnumPlayerState.ASK_TO_REPLAY;
                    infoMsg = "Le joueur noir propose une autre partie";
                }

                // si les deux joueurs sont OK, on génère une nouvelle partie
                if (party.FK_White_PlayerState == (int)EnumPlayerState.ASK_TO_REPLAY && party.FK_Black_PlayerState == (int)EnumPlayerState.ASK_TO_REPLAY)
                {
                    toReturn = CreateNewParty(new PartyOptionModel()
                    {
                        ChoisedCadenceType = (EnumPartyCadence)party.FK_PartyCadence,
                        TimeInSeconds = party.PartyTimeInSeconds.HasValue ? party.PartyTimeInSeconds.Value : 0,
                        FischerInSeconds = party.PartyFischerInSeconds.HasValue ? party.PartyFischerInSeconds.Value : 0
                    });
                    infoMsg = String.Format("Nouvelle partie {0}", toReturn.Name);
                }
                else // sinon on reste sur l'ancienne
                {
                    toReturn = party.ToPartyModel();
                }

                Update(party);

                toReturn.ResultText = infoMsg;
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
                toReturn.ErrorMsg = "Erreur non gérée dans la demande de rejouer la partie";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }

        #region private

        private void ManagePlayerTime(Party party, Color playerColor, ref string resultText, ref EnumPlayerState playerState, ref EnumPartyState partyState)
        {
            if (party.FK_PartyCadence != (int)EnumPartyCadence.NO_LIMIT)
            {
                long timeUsedForMove = CalcTimeUseForMove(party);
                // mise à jour du temps du joueur
                if (IsTimeOverAfterUpdatePlayerTime(party, playerColor, timeUsedForMove))
                {
                    // on a dépassé le temps
                    resultText = "Temps écoulé";
                    partyState = playerColor == Color.WHITE ? EnumPartyState.OVER_BLACK_WIN : EnumPartyState.OVER_WHITE_WIN;
                    playerState = EnumPlayerState.TIME_OVER;
                }
            }
            party.LastMoveDate = DateTime.Now;
        }

        /// <summary>
        /// Retourne le temps écoulé depuis le dernier coup en millisecondes
        /// </summary>
        /// <param name="party">la partie concernée</param>
        /// <returns>Le temps écoulé</returns>
        private long CalcTimeUseForMove(Party party)
        {
            long calcTime = 0;
            if (party.LastMoveDate.HasValue)
            {
                calcTime = (long)(DateTime.Now - party.LastMoveDate.Value).TotalMilliseconds;
            }
            return calcTime;
        }

        private bool IsTimeOverAfterUpdatePlayerTime(Party party, Color playerColor, long timeUsedForMove)
        {
            bool timeOver = false;

            // on retranche le temps fischer de la durée du coup
            if (party.FK_PartyCadence == (int)EnumPartyCadence.FISCHER && party.PartyFischerInSeconds.HasValue)
                timeUsedForMove -= party.PartyFischerInSeconds.Value * 1000;

            switch (playerColor)
            {
                case Color.BLACK:
                    party.BlackTimeLeftInLilliseconds = party.BlackTimeLeftInLilliseconds - timeUsedForMove;
                    if (party.BlackTimeLeftInLilliseconds <= 0)
                        timeOver = true;
                    break;
                case Color.WHITE:
                    party.WhiteTimeLeftInMilliseconds = party.WhiteTimeLeftInMilliseconds - timeUsedForMove;
                    if (party.WhiteTimeLeftInMilliseconds <= 0)
                        timeOver = true;
                    break;
            }

            return timeOver;
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

        private static EnumPartyState RefreshPartyState(Color playerColor, EnumPlayerState ennemiState, EnumPartyState partyState)
        {
            if (ennemiState == EnumPlayerState.CHECK_MAT && playerColor == Color.BLACK)
                partyState = EnumPartyState.OVER_WHITE_WIN;
            if (ennemiState == EnumPlayerState.CHECK_MAT && playerColor == Color.WHITE)
                partyState = EnumPartyState.OVER_BLACK_WIN;
            if (ennemiState == EnumPlayerState.PAT)
                partyState = EnumPartyState.DRAWN;
            return partyState;
        }

        #endregion
    }
}

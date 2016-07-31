using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace DaChess.Business
{
    internal class BoardManager : IBoardManager
    {
        private const int BOARD_CASE = 8;

        internal CaseInfo[][] Cases { get; set; } // tableau de cases : ligne/ colonne

        internal BoardManager()
        {
            Cases = new CaseInfo[BOARD_CASE][];
            for (int i = 0; i < BOARD_CASE; i++)
            {
                Cases[i] = new CaseInfo[8];
            }
        }

        public void Init(string jsonBoard)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(jsonBoard);
            object[] cases = (object[])(routes_list["board"]);

            for (int i = 0; i < cases.Length; i++)
            {
                IDictionary<string, object> myCase = (IDictionary<string, object>)cases[i];

                int col = BoardsHelper.ColToInt(myCase["col"].ToString());
                int line = Int32.Parse(myCase["line"].ToString());

                CaseInfo toAdd = new CaseInfo()
                {
                    HasMove = Boolean.Parse(myCase["hasMove"].ToString()),
                    Piece = GetPieceType(myCase["piece"].ToString().ToLower()),
                    PieceColor = myCase["piece"].ToString().StartsWith("b") ? Colors.BLACK : Colors.WHITE
                };

                Cases[line - 1][col - 1] = toAdd;
            }
        }

        /// <summary>
        /// Un coup consiste en une case de départ et une d'arrivée séparées par un espace
        /// </summary>
        /// <param name="move"></param>
        public string MakeMove(string move, string partyName, string playerToken)
        {
            string resultText = String.Empty;

            try
            {
                move = move.TrimEnd(' ');

                Party party = PartyHelper.GetByName(partyName);
                Colors playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                this.Init(party.Board);

                if (!PartyHelper.IsPlayerInParty(partyName, playerToken))
                    throw new DaChessException("Vous n'êtes pas dans cette partie !");

                string[] cases = move.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (!IsMoveOk(cases))
                    throw new DaChessException(String.Format("Le coup {0} est mal formaté", move));

                // on récupère la première pièce              
                string enPassant = String.Empty;
                string lastMoveCase = String.Empty;
                bool promotePawn = false;
                History histo = null;

                int startLine = Int32.Parse(cases[0].Substring(1, 1)) - 1;
                int startCol = BoardsHelper.ColToInt(cases[0].Substring(0, 1)) - 1;

                if (Cases[startLine][startCol].Piece.HasValue)
                {
                    int endLine = Int32.Parse(cases[1].Substring(1, 1)) - 1;
                    int endCol = BoardsHelper.ColToInt(cases[1].Substring(0, 1)) - 1;
                    MovesType mt;

                    if (!BoardsHelper.IsLegalMove(Cases[startLine][startCol], Cases[endLine][endCol], Cases, startLine, endLine, startCol, endCol, party, out mt))
                    {
                        throw new DaChessException("Coup illégal");
                    }

                    // on stocke la case d'arrivée
                    lastMoveCase = String.Concat(BoardsHelper.IntToCol(endCol + 1), endLine + 1);

                    switch (mt) // gestion de l'après déplacement
                    {
                        case MovesType.CLASSIC:
                            move = move.Replace(" ", String.Empty);
                            break;
                        case MovesType.CAPTURE:
                            move = move.Replace(" ", "x");
                            break;
                        case MovesType.EN_PASSANT:
                            move = move.Replace(" ", "x");
                            move = String.Concat(move, " e.p.");
                            // on enlève la pièce prise en passant
                            string epCol = party.EnPassantCase.Substring(0, 1);
                            string epLine = party.EnPassantCase.Substring(1, 1);
                            int epColInt = BoardsHelper.ColToInt(epCol) - 1;
                            int epLineInt = Int32.Parse(epLine) - 1;
                            this.Cases[epLineInt][epColInt].HasMove = null;
                            this.Cases[epLineInt][epColInt].Piece = null;
                            this.Cases[epLineInt][epColInt].PieceColor = null;
                            resultText = "Prise en passant !";
                            break;
                        case MovesType.CASTLING_SHORT:
                            move = "O-O";
                            this.Cases[startLine][endCol - 1].HasMove = true;
                            this.Cases[startLine][endCol - 1].Piece = PiecesType.ROOK;
                            this.Cases[startLine][endCol - 1].PieceColor = this.Cases[startLine][this.Cases[startLine].Length - 1].PieceColor;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].HasMove = null;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].Piece = null;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].PieceColor = null;
                            resultText = "Petit roque";
                            break;
                        case MovesType.CASTLING_LONG:
                            move = "O-O-O";
                            this.Cases[startLine][endCol + 1].HasMove = true;
                            this.Cases[startLine][endCol + 1].Piece = PiecesType.ROOK;
                            this.Cases[startLine][endCol + 1].PieceColor = this.Cases[startLine][0].PieceColor;
                            this.Cases[startLine][0].HasMove = null;
                            this.Cases[startLine][0].Piece = null;
                            this.Cases[startLine][0].PieceColor = null;
                            resultText = "Grand roque";
                            break;
                        case MovesType.PROMOTE:
                            promotePawn = true;
                            resultText = "Promotion du pion";
                            if (Cases[endLine][endCol].Piece.HasValue)
                                move = move.Replace(" ", "x");
                            break;
                        default:
                            move = move.Replace(" ", "-");
                            break;
                    }

                    // pour la gestion de la prise en passant
                    if (Cases[startLine][startCol].Piece == PiecesType.PAWN && Math.Abs(startLine - endLine) == 2) // un pion qui a bougé de 2 cases
                    {
                        enPassant = cases[1]; // case d'arrivée
                    }

                    Cases[endLine][endCol].HasMove = true;
                    Cases[endLine][endCol].Piece = Cases[startLine][startCol].Piece;
                    Cases[endLine][endCol].PieceColor = Cases[startLine][startCol].PieceColor;
                    Cases[startLine][startCol].HasMove = null;
                    Cases[startLine][startCol].Piece = null;
                    Cases[startLine][startCol].PieceColor = null;
                }
                else
                {
                    throw new DaChessException("Coup illégal, aucune pièce sur la case de départ");
                }

                PlayerStateEnum ennemiState = DefineEnnemiState(out resultText, out move, move, playerColor);

                if (BoardsHelper.IsCheck(playerColor, this.Cases))
                    throw (new DaChessException("Coup impossible, échec !"));

                // on sauvegarde
                using (var context = new ChessEntities())
                {                   
                    PartyHistory partHist = new PartyHistory()
                    {
                        Board = this.ToBoardDescription(),
                        FK_Party = party.Id,
                        DateCreation = DateTime.Now
                    };
                    context.PartyHistories.Add(partHist);
                    context.SaveChanges();
                }

                // vérification qu'on n'ai pas 3 fois la même position
                if (BoardsHelper.IsTreeTimeSamePosition(this.ToBoardDescription(), party.Id))
                {
                    // partie nulle
                    resultText = "3 fois la même position, partie nulle";
                    ennemiState = PlayerStateEnum.DRAWN;
                }

                // mise à jour de l'historique        
                histo = UpdateHistorique(move, party, playerColor, ennemiState);

                using (var context = new ChessEntities())
                {
                    context.Parties.Attach(party);
                    party.Board = this.ToJsonString();
                    party.WhiteTurn = promotePawn == true ? party.WhiteTurn : !party.WhiteTurn; // on ne change de joueur que si il n'y a pas de promotion de pion à faire
                    party.History = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                    party.EnPassantCase = enPassant;
                    party.LastMoveCase = lastMoveCase;
                    if (playerColor == Colors.WHITE)
                    {
                        party.FK_Black_Player_Stat = (int)ennemiState;
                        party.FK_White_Player_Stat = (int)PlayerStateEnum.UNDEFINE;
                    }
                    else
                    {
                        party.FK_White_Player_Stat = (int)ennemiState;
                        party.FK_Black_Player_Stat = (int)PlayerStateEnum.UNDEFINE;
                    }
                    party.BlackCanPromote = promotePawn && playerColor == Colors.BLACK;
                    party.WhiteCanPromote = promotePawn && playerColor == Colors.WHITE;
                    party.PartyOver = ennemiState == PlayerStateEnum.CHECKMAT || ennemiState == PlayerStateEnum.PAT || ennemiState == PlayerStateEnum.DRAWN;                  

                    context.SaveChanges();
                }
            }
            catch (DaChessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaChessException(
                    String.Format("Erreur non gérée lors du coup {0}", move), ex);
            }

            return resultText;
        }

        public void PromotePiece(string partyName, string choisedPiece, string playerToken)
        {
            try
            {
                Party party = PartyHelper.GetByName(partyName);
                Colors playerColor = PartyHelper.GetPlayerColor(party, playerToken);
                this.Init(party.Board);

                // on récupère le pion correspondant
                int limitLine = 0;
                if (playerColor == Colors.WHITE) // uniquement les lignes de fond de plateau
                    limitLine = this.Cases.Length - 1;

                for (int col = 0; col < this.Cases[limitLine].Length; col++)
                {
                    if (Cases[limitLine][col].Piece.HasValue
                        && Cases[limitLine][col].Piece.Value == PiecesType.PAWN
                        && Cases[limitLine][col].PieceColor == playerColor)
                    {
                        Cases[limitLine][col].Piece = this.GetPieceType(choisedPiece);
                        break;
                    }
                }

                // mise à jour de l'historique
                History histo = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(party.History);
                int lastMove = histo.Moves.Select(m => m.Key).Max();
                string toAdd = String.Empty;
                switch (this.GetPieceType(choisedPiece))
                {
                    case PiecesType.BISHOP:
                        toAdd = "=B";
                        break;
                    case PiecesType.KNIGHT:
                        toAdd = "=N";
                        break;
                    case PiecesType.ROOK:
                        toAdd = "=R";
                        break;
                    case PiecesType.QUEEN:
                        toAdd = "=Q";
                        break;
                }
                histo.Moves[lastMove] += toAdd;

                PlayerStateEnum ennemiState = DefineEnnemiState(playerColor);

                // on sauvegarde la partie
                using (var context = new ChessEntities())
                {
                    context.Parties.Attach(party);
                    party.Board = this.ToJsonString();
                    party.WhiteTurn = !party.WhiteTurn; // on ne change de joueur après promotion
                    party.BlackCanPromote = false;
                    party.WhiteCanPromote = false;
                    party.History = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                    if (playerColor == Colors.WHITE)
                    {
                        party.FK_Black_Player_Stat = (int)ennemiState;
                        party.FK_White_Player_Stat = (int)PlayerStateEnum.UNDEFINE;
                    }
                    else
                    {
                        party.FK_White_Player_Stat = (int)ennemiState;
                        party.FK_Black_Player_Stat = (int)PlayerStateEnum.UNDEFINE;
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
                throw new DaChessException("Erreur non gérée lors de la promotion", ex);
            }

        }

        public string ToJsonString()
        {
            string toReturn = @"{{
		        ""board"":[{0}]}}";
            StringBuilder innerString = new StringBuilder();

            bool removeLastChar = false;
            for (int line = 0; line < Cases.Length; line++)
            {
                for (int col = 0; col < Cases[line].Length; col++)
                {
                    if (Cases[line][col].Piece.HasValue)
                    {
                        innerString.Append(CaseInfoToJson(Cases[line][col], col, line));
                        innerString.Append(',');
                        removeLastChar = true;
                    }
                }
            }
            if (removeLastChar)
                innerString.Remove(innerString.Length - 1, 1);

            return string.Format(toReturn, innerString.ToString());
        }

        public string ToBoardDescription()
        {
            StringBuilder toReturn = new StringBuilder();

            IList<string> myPieces = new List<string>();
            string pattern = "{0}{1}{2}{3}#"; // couleur, nom de pièce en anglais (K,Q,R,B,N,P)

            for (int line = 0; line < Cases.Length; line++)
            {
                for (int col = 0; col < Cases[line].Length; col++)
                {
                    if (Cases[line][col].Piece.HasValue)
                    {
                        myPieces.Add(String.Format(pattern, (int)Cases[line][col].PieceColor.Value,
                           (char)Cases[line][col].Piece.Value, BoardsHelper.IntToCol(col + 1), line + 1));
                    }
                }
            }

            // on met les pièces dans l'ordre
            foreach(var p in myPieces.OrderBy(p => p))
            {
                toReturn.Append(p);
            }

            return toReturn.ToString();
        }

        public static History UpdateHistorique(string move, Party party, Colors playerColor, PlayerStateEnum ennemiState)
        {
            History histo;
            if (String.IsNullOrEmpty(party.History))
            {
                histo = new History();
            }
            else
            {
                histo = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(party.History);
            }

            int moveNumber = 0;
            if (histo.Moves.Count() > 0)
            {
                moveNumber = histo.Moves.OrderByDescending(h => h.Key).First().Key;
            }
            if (playerColor == Colors.WHITE)
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
            if ((ennemiState == PlayerStateEnum.PAT || ennemiState == PlayerStateEnum.DRAWN) && playerColor == Colors.WHITE)
            {
                histo.Moves[moveNumber + 1] += " 1/2-1/2";
            }
            else if ((ennemiState == PlayerStateEnum.PAT || ennemiState == PlayerStateEnum.DRAWN) && playerColor == Colors.BLACK)
            {
                histo.Moves[moveNumber] += " 1/2-1/2";
            }
            else if ((ennemiState == PlayerStateEnum.CHECKMAT || ennemiState == PlayerStateEnum.RESIGN) && playerColor == Colors.WHITE)
            {
                histo.Moves[moveNumber + 1] += " 1-0";
            }
            else if ((ennemiState == PlayerStateEnum.CHECKMAT || ennemiState == PlayerStateEnum.RESIGN) && playerColor == Colors.BLACK)
            {
                if (moveNumber == 0) // cas de l'abandon des blancs avant le premier coup
                    histo.Moves[1] += " 0-1";
                else
                    histo.Moves[moveNumber] += " 0-1";
            }

            return histo;
        }

        private PlayerStateEnum DefineEnnemiState(Colors playerColor)
        {
            PlayerStateEnum toReturn = PlayerStateEnum.UNDEFINE;

            // je peux mettre échec l'adversaire mais je ne peux pas l'être à la fin de mon coup            
            if (BoardsHelper.IsCheck(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
            {
                // vérifier si on mat
                if (BoardsHelper.IsCheckMat(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
                    toReturn = PlayerStateEnum.CHECKMAT;
                else
                    toReturn = PlayerStateEnum.CHECK;
            }
            else if (BoardsHelper.IsPat(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
                toReturn = PlayerStateEnum.PAT;

            if (BoardsHelper.IsCheck(playerColor, this.Cases))
                throw (new DaChessException("Coup impossible, échec !"));

            return toReturn;
        }

        private PlayerStateEnum DefineEnnemiState(out string resultText, out string moveResult, string move, Colors playerColor)
        {
            PlayerStateEnum toReturn = DefineEnnemiState(playerColor);

            resultText = String.Empty;
            moveResult = move;
            switch (toReturn)
            {
                case PlayerStateEnum.CHECK:
                    resultText = "Echec !";
                    moveResult = String.Concat(move, "+");
                    break;
                case PlayerStateEnum.CHECKMAT:
                    resultText = "Echec et Mat !";
                    moveResult = String.Concat(move, "++");
                    break;
                case PlayerStateEnum.PAT:
                    resultText = "Pat !";
                    break;
            }

            return toReturn;
        }

        private string CaseInfoToJson(CaseInfo c, int col, int line)
        {
            return string.Format(@"{{
                ""col"" :""{0}"",
				""line"" : ""{1}"",
				""piece"" : ""{2}"",
                ""hasMove"" : ""{3}""
            }}", BoardsHelper.IntToCol(col + 1), line + 1, this.PieceTypeToString(c.Piece.Value, c.PieceColor.Value), c.HasMove.ToString());
        }

        private bool IsMoveOk(string[] cases)
        {
            if (cases.Length != 2)
                return false;
            if (cases[1].Length != 2)
                return false;
            if (cases[0].Length != 2)
                return false;

            return true;
        }

        private PiecesType GetPieceType(string piece)
        {
            if (piece.Contains("paw"))
                return PiecesType.PAWN;
            else if (piece.Contains("roo"))
                return PiecesType.ROOK;
            else if (piece.Contains("kni"))
                return PiecesType.KNIGHT;
            else if (piece.Contains("bis"))
                return PiecesType.BISHOP;
            else if (piece.Contains("que"))
                return PiecesType.QUEEN;
            else if (piece.Contains("kin"))
                return PiecesType.KING;

            throw new DaChessException("Pièce inconnue");
        }

        private string PieceTypeToString(PiecesType pt, Colors c)
        {
            string toReturn = String.Empty;
            string pattern = "{0}_{1}";
            string color = c == Colors.BLACK ? "b" : "w";
            switch (pt)
            {
                case PiecesType.BISHOP:
                    toReturn = String.Format(pattern, color, "bishop");
                    break;
                case PiecesType.KING:
                    toReturn = String.Format(pattern, color, "king");
                    break;
                case PiecesType.KNIGHT:
                    toReturn = String.Format(pattern, color, "knight");
                    break;
                case PiecesType.PAWN:
                    toReturn = String.Format(pattern, color, "pawn");
                    break;
                case PiecesType.QUEEN:
                    toReturn = String.Format(pattern, color, "queen");
                    break;
                case PiecesType.ROOK:
                    toReturn = String.Format(pattern, color, "rook");
                    break;
            }
            return toReturn;
        }
    }

    internal class History
    {
        internal History()
        {
            Moves = new Dictionary<int, string>();
        }

        public Dictionary<int, string> Moves { get; set; }
    }

    internal struct CaseInfo
    {
        public bool? HasMove { get; set; }
        public PiecesType? Piece { get; set; }
        public Colors? PieceColor { get; set; }
    }

    internal struct Coord
    {
        public Coord(int l, int c)
        {
            Line = l;
            Col = c;
        }

        public int Line { get; set; }
        public int Col { get; set; }
    }
}

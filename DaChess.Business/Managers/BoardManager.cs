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
            for(int i = 0;i< BOARD_CASE;i++)
            {
                Cases[i] = new CaseInfo[8];
            }
        }

        public void Init(string jsonBoard)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(jsonBoard);
            object[] cases = (object[])(routes_list["board"]);

            for(int i = 0; i< cases.Length;i++)
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
            string toReturn = String.Empty;

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
                string col = cases[0].Substring(0, 1);
                string line = cases[0].Substring(1, 1);
                string enPassant = String.Empty;
                bool promotePawn = false;
                bool isEnnemiCheck = false;
                History histo = null;

                int startLine = Int32.Parse(cases[0].Substring(1, 1)) - 1;
                int startCol = BoardsHelper.ColToInt(cases[0].Substring(0, 1)) -1;

                if(Cases[startLine][startCol].Piece.HasValue)
                {
                    int endLine = Int32.Parse(cases[1].Substring(1, 1)) -1;
                    int endCol = BoardsHelper.ColToInt(cases[1].Substring(0, 1)) -1;
                    MovesType mt;

                    if (!BoardsHelper.IsLegalMove(Cases[startLine][startCol], Cases[endLine][endCol], Cases, startLine, endLine, startCol, endCol, party, out mt))
                    {
                        throw new DaChessException("Coup illégal");
                    }

                    switch(mt)
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
                            toReturn = "Prise en passant !";
                            break;
                        case MovesType.CASTLING_SHORT:
                            move = "O-O";                            
                            this.Cases[startLine][endCol - 1].HasMove = true;
                            this.Cases[startLine][endCol - 1].Piece = PiecesType.ROOK;
                            this.Cases[startLine][endCol - 1].PieceColor = this.Cases[startLine][this.Cases[startLine].Length - 1].PieceColor;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].HasMove = null;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].Piece = null;
                            this.Cases[startLine][this.Cases[startLine].Length - 1].PieceColor = null;
                            toReturn = "Petit roque";
                            break;
                        case MovesType.CASTLING_LONG:
                            move = "O-O-O";
                            this.Cases[startLine][endCol + 1].HasMove = true;
                            this.Cases[startLine][endCol + 1].Piece = PiecesType.ROOK;
                            this.Cases[startLine][endCol + 1].PieceColor = this.Cases[startLine][0].PieceColor;
                            this.Cases[startLine][0].HasMove = null;
                            this.Cases[startLine][0].Piece = null;
                            this.Cases[startLine][0].PieceColor = null;
                            toReturn = "Grand roque";
                            break;
                        case MovesType.PROMOTE:
                            promotePawn = true;
                            toReturn = "Promotion du pion";
                            if(Cases[endLine][endCol].Piece.HasValue)
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

                // je peux mettre échec l'adversaire mais je ne peux pas l'être à la fin de mon coup            
                if (BoardsHelper.IsCheck(playerColor == Colors.BLACK?Colors.WHITE:Colors.BLACK, this.Cases))
                {
                    // vérifier si on mat
                    if(BoardsHelper.IsCheckMat(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
                    {

                    }
                    toReturn = "Echec !";
                    isEnnemiCheck = true;
                    move = String.Concat(move, "!");
                }
                if (BoardsHelper.IsCheck(playerColor, this.Cases))
                    throw (new DaChessException("Coup impossible, échec !"));


                // mise à jour de l'historique        
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
                if (PartyHelper.GetPlayerColor(party, playerToken) == Colors.WHITE)
                {
                    histo.Moves.Add(moveNumber + 1, move);
                }
                else
                {
                    histo.Moves[moveNumber] += " " + move;
                }
                
                using (var context = new ChessEntities())
                {
                    context.Parties.Attach(party);
                    party.Board = this.ToJsonString();
                    party.WhiteTurn = promotePawn == true?party.WhiteTurn:!party.WhiteTurn; // on ne change de joueur que si il n'y a pas de promotion de pion à faire
                    party.History = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                    party.EnPassantCase = enPassant;
                    party.BlackIsCheck = isEnnemiCheck && playerColor == Colors.WHITE;
                    party.WhiteIsCheck = isEnnemiCheck && playerColor == Colors.BLACK;
                    party.BlackCanPromote = promotePawn && playerColor == Colors.BLACK;
                    party.WhiteCanPromote = promotePawn && playerColor == Colors.WHITE;
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

            return toReturn;
        }

        public void PromotePiece(string partyName, string choisedPiece, string playerToken)
        {
            Party party = PartyHelper.GetByName(partyName);
            Colors playerColor = PartyHelper.GetPlayerColor(party, playerToken);
            this.Init(party.Board);

            // on récupère le pion correspondant
            int limitLine = 0;
            if (playerColor == Colors.WHITE) // uniquement les lignes de fond de plateau
                limitLine = this.Cases.Length - 1;

            for(int col = 0; col<this.Cases[limitLine].Length; col++)
            {
                if(Cases[limitLine][col].Piece.HasValue 
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
            switch(this.GetPieceType(choisedPiece))
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

            bool isEnnemiCheck = false;
            if (BoardsHelper.IsCheck(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
            {
                // vérifier si on mat
                if (BoardsHelper.IsCheckMat(playerColor == Colors.BLACK ? Colors.WHITE : Colors.BLACK, this.Cases))
                {

                }

                isEnnemiCheck = true;
                histo.Moves[lastMove] += "!";
            }

            // on sauvegarde la partie
            using (var context = new ChessEntities())
            {
                context.Parties.Attach(party);
                party.Board = this.ToJsonString();
                party.WhiteTurn = !party.WhiteTurn; // on ne change de joueur après promotion
                party.BlackCanPromote = false;
                party.WhiteCanPromote = false;
                party.History = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
                party.BlackIsCheck = isEnnemiCheck && playerColor == Colors.WHITE;
                party.WhiteIsCheck = isEnnemiCheck && playerColor == Colors.BLACK;
                context.SaveChanges();
            }
        }

        public string ToJsonString()
        {
            string toReturn = @"{{
		        ""board"":[{0}]}}";
            StringBuilder innerString = new StringBuilder();

            bool removeLastChar = false;
            for (int line = 0; line< Cases.Length; line++)
            {
                for(int col = 0; col<Cases[line].Length;col++)
                {
                    if(Cases[line][col].Piece.HasValue)
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
            switch(pt)
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
        public PiecesType? Piece {get;set;}
        public Colors? PieceColor { get; set; }
    }

    internal struct Coord
    {
        public int Line { get; set; }
        public int Col { get; set; }
    }
}

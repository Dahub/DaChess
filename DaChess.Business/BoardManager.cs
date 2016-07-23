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
        public void MakeMove(string move, string partyName, string playerToken)
        {
            try
            {
                move = move.TrimEnd(' ');

                Party party = PartyHelper.GetByName(partyName);
                this.Init(party.Board);

                if (!PartyHelper.IsPlayerInParty(partyName, playerToken))
                    throw new DaChessException("Vous n'êtes pas dans cette partie !");

                string[] cases = move.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (!IsMoveOk(cases))
                    throw new DaChessException(String.Format("Le coup {0} est mal formaté", move));

                // on récupère la première pièce
                string col = cases[0].Substring(0, 1);
                string line = cases[0].Substring(1, 1);
                //string history = party.History;
                History histo = null;

                int startLine = Int32.Parse(cases[0].Substring(1, 1)) - 1;
                int startCol = BoardsHelper.ColToInt(cases[0].Substring(0, 1)) -1;

                if(Cases[startLine][startCol].Piece.HasValue)
                {
                    int endLine = Int32.Parse(cases[1].Substring(1, 1)) -1;
                    int endCol = BoardsHelper.ColToInt(cases[1].Substring(0, 1)) -1;

                    if (!BoardsHelper.IsLegalMove(Cases[startLine][startCol], Cases[endLine][endCol], Cases, startLine, endLine, startCol, endCol))
                    {
                        throw new DaChessException("Coup illégal");
                    }

                    if (Cases[endLine][endCol].Piece.HasValue) // si il y a une pièce, c'est une prise
                    {
                        move = move.Replace(" ", "x");
                    }
                    else
                    {
                        move = move.Replace(" ", "-");
                    }

                    Cases[endLine][endCol].HasMove = true;
                    Cases[endLine][endCol].Piece = Cases[startLine][startCol].Piece;
                    Cases[endLine][endCol].PieceColor = Cases[startLine][startCol].PieceColor;
                    Cases[startLine][startCol].HasMove = null;
                    Cases[startLine][startCol].Piece = null;
                    Cases[startLine][startCol].PieceColor = null;              

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
                }
                else
                {
                    throw new DaChessException("Coup illégal, aucune pièce sur la case de départ");
                }

                using (var context = new ChessEntities())
                {
                    context.Parties.Attach(party);
                    party.Board = this.ToJsonString();
                    party.WhiteTurn = !party.WhiteTurn;
                    party.History = Newtonsoft.Json.JsonConvert.SerializeObject(histo);
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
}

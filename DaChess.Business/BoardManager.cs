﻿using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace DaChess.Business
{
    internal class BoardManager : IBoardManager
    {
        internal IList<BoardCase> BoardCases { get; set; }

        internal BoardManager() { BoardCases = new List<BoardCase>(); }

        public void Init(string jsonBoard)
        {
            BoardCases = new List<BoardCase>();

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(jsonBoard);
            object[] cases = (object[])(routes_list["board"]);
            for (int i = 0; i < cases.Length; i++)
            {
                BoardCases.Add(new BoardCase((IDictionary<string, object>)(cases[i])));
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

                BoardCase startCase = BoardsHelper.FindCase(col, line, this.BoardCases);
                if (startCase != null)
                {
                    string piece = startCase.Piece;
                    col = cases[1].Substring(0, 1);
                    line = cases[1].Substring(1, 1);
                    BoardCase endCase = BoardsHelper.FindCase(col, line, this.BoardCases);

                    if (endCase == null)
                    {
                        endCase = new BoardCase() { Col = col, Line = line };
                        this.BoardCases.Add(endCase);
                    }

                    if (!BoardsHelper.IsLegalMove(startCase, endCase, BoardCases))
                    {
                        throw new DaChessException("Coup illégal");
                    }

                    if (!String.IsNullOrEmpty(endCase.Piece)) // si il y a une pièce, c'est une prise
                    {
                        move = move.Replace(" ", "x");
                    }
                    else
                    {
                        move = move.Replace(" ", "-");
                    }
                    endCase.Piece = piece;
                    endCase.HasMove = true;
                    this.BoardCases.Remove(startCase);

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
            for (int i = 0; i < BoardCases.Count; i++)
            {

                innerString.Append(BoardCases[i].ToJsonString());
                if (i != BoardCases.Count - 1)
                {
                    innerString.Append(',');
                }
            }
            return string.Format(toReturn, innerString.ToString());
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
    }

    internal class History
    {
        internal History()
        {
            Moves = new Dictionary<int, string>();
        }

        public Dictionary<int, string> Moves { get; set; }
    }

    internal class BoardCase
    {
        internal BoardCase() { }

        internal BoardCase(IDictionary<string, object> values)
        {
            if (values.ContainsKey("col"))
            {
                this.Col = values["col"].ToString();
            }
            if (values.ContainsKey("line"))
            {
                this.Line = values["line"].ToString();
            }
            if (values.ContainsKey("piece"))
            {
                this.Piece = values["piece"].ToString();
            }
            if (values.ContainsKey("hasMove"))
            {
                this.HasMove = Boolean.Parse(values["hasMove"].ToString());
            }
        }

        public string Col { get; set; }
        public string Line { get; set; }
        public string Piece { get; set; }
        public bool HasMove { get; set; }
        public PiecesType PieceType
        {
            get
            {
                if (Piece.ToLower().Contains("paw"))
                    return PiecesType.PAWN;
                else if (Piece.ToLower().Contains("roo"))
                    return PiecesType.ROOK;
                else if (Piece.ToLower().Contains("kni"))
                    return PiecesType.KNIGHT;
                else if (Piece.ToLower().Contains("bis"))
                    return PiecesType.BISHOP;
                else if (Piece.ToLower().Contains("que"))
                    return PiecesType.QUEEN;
                else if (Piece.ToLower().Contains("kin"))
                    return PiecesType.KING;

                throw new DaChessException("Aucune pièce sur cette case");
            }
        }
        public Colors PieceColor
        {
            get
            {
                if (Piece.ToLower().Contains("w_"))
                    return Colors.WHITE;
                else if (Piece.ToLower().Contains("b_"))
                    return Colors.BLACK;

                throw new DaChessException("Aucune pièce sur cette case");
            }
        }

        public string ToJsonString()
        {
            return string.Format(@"{{
                ""col"" :""{0}"",
				""line"" : ""{1}"",
				""piece"" : ""{2}"",
                ""hasMove"" : ""{3}""
            }}", this.Col, this.Line, this.Piece, this.HasMove.ToString());
        }
    }
}
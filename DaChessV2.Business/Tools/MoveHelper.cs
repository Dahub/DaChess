using DaChessV2.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaChessV2.Business
{
    /// <summary>
    /// Cette classe est dédiée à la gestion de la notation des coups
    /// </summary>
    internal static class MoveNotationHelper
    {
        /// <summary>
        /// A partir d'un déplacement de la forme [col départ][line départ] [col arrivée][line arrivée]
        /// construit un déplacement normé
        /// exemple : e2 e4 (déplacement du pion en e2 vers e4) => e4
        /// </summary>
        /// <param name="move">le déplacement dans sa forme [col départ][line départ] [col arrivée][line arrivée]</param>
        /// <param name="moveType">le type de déplacement (classique, prise, roque...)</param>
        /// <param name="movingPiece">la pièce qui se déplace</param>
        /// <param name="board">le plateau de jeu</param>
        /// <returns>le déplacement en notation normée</returns>
        internal static string BuildMoveNotation(string move, EnumMoveType moveType, CaseInfo movingPiece, Coord startCase, Coord endCase, CaseInfo[][] board)
        {
            if (movingPiece.Piece.HasValue)
            {
                switch (moveType)
                {
                    case EnumMoveType.CLASSIC:                        
                        move = DefineMoveFromPiece(move, movingPiece, startCase, endCase, board);
                        move = move.Replace(" ", String.Empty);
                        if (movingPiece.Piece.Value == EnumPieceType.PAWN) // si c'est un pion, on enlève même la colonne
                            move = move.Substring(1, 2);
                        break;
                    case EnumMoveType.CAPTURE:
                        move = DefineMoveFromPiece(move, movingPiece, startCase, endCase, board);
                        move = move.Replace(" ", "x");                        
                        break;
                    case EnumMoveType.EN_PASSANT:
                        move = DefineMoveFromPiece(move, movingPiece, startCase, endCase, board);
                        move = move.Replace(" ", "x");
                        move = String.Concat(move, " e.p.");
                        break;
                    case EnumMoveType.CASTLING_SHORT:
                        move = "O-O";
                        break;
                    case EnumMoveType.CASTLING_LONG:
                        move = "O-O-O";
                        break;
                }
            }
            return move;
        }

        private static string DefineMoveFromPiece(string move, CaseInfo movingPiece, Coord startCase, Coord endCase, CaseInfo[][] board)
        {
            switch (movingPiece.Piece.Value)
            {
                case EnumPieceType.PAWN:
                    // on enlève la case de départ
                    move = String.Concat(move.First(), move.Substring(2, 3));
                    break;
                case EnumPieceType.KING:
                    // pas de soucis, il n'y a qu'un roi, aucune ambiguité possible                           
                    move = move.Substring(2, 3);
                    move = String.Concat((char)movingPiece.Piece.Value, move);
                    break;
                case EnumPieceType.KNIGHT:
                case EnumPieceType.BISHOP:
                case EnumPieceType.ROOK:
                case EnumPieceType.QUEEN:
                    // on va récupérer toutes les pièces de la couleur du joueur du type concerné
                    IList<Coord> allSamePieces = GetAllPiecesCoordsFromTypeAndColor(movingPiece.Piece.Value, movingPiece.PieceColor.Value, board);
                    if (allSamePieces.Count == 1) // 1 seule pièce, pas d'ambiguité
                    {
                        move = move.Substring(2, 3);
                        move = String.Concat((char)movingPiece.Piece.Value, move);
                    }
                    else
                    {
                        IList<Coord> otherCanDoMove = SearchWhoCanMove(startCase, endCase, board, allSamePieces, movingPiece.Piece.Value);
                        if (otherCanDoMove.Count == 0) // pas d'autre déplacement possible
                        {
                            move = move.Substring(2, 3);
                            move = String.Concat((char)movingPiece.Piece.Value, move);
                        }
                        else
                        {
                            bool colDif = true;
                            bool lineDif = true;
                            foreach (var c in otherCanDoMove)
                            {
                                if (c.Line == startCase.Line)
                                    lineDif = false;
                                if (c.Col == startCase.Col)
                                    colDif = false;
                            }
                            if (colDif) // on conserve la colonne
                            {
                                move = String.Concat(move.First(), move.Substring(2, 3));
                                move = String.Concat((char)movingPiece.Piece.Value, move);
                            }
                            else if (lineDif) // on conserve la ligne
                            {
                                move = String.Concat(move.Substring(1, 1), move.Substring(2, 3));
                                move = String.Concat((char)movingPiece.Piece.Value, move);
                            }
                            else // on conserve les deux
                            {
                                move = String.Concat((char)movingPiece.Piece.Value, move);
                            }
                        }
                    }
                    break;
            }

            return move;
        }

        private static IList<Coord> SearchWhoCanMove(Coord startCase, Coord endCase, CaseInfo[][] board, IList<Coord> pieces, EnumPieceType pieceType)
        {
            IList<Coord> otherCanDoMove = new List<Coord>();
            foreach (var n in pieces)
            {
                if (n.Line != startCase.Line || n.Col != startCase.Col) // on vérifie que ce n'est pas notre pièce qu'on test
                {
                    if(BoardHelper.GetMoveType(board, n, endCase, String.Empty) != EnumMoveType.ILLEGAL)
                        otherCanDoMove.Add(n);
                }
            }

            return otherCanDoMove;
        }

        private static IList<Coord> GetAllPiecesCoordsFromTypeAndColor(EnumPieceType pieceType, Color pieceColor, CaseInfo[][] board)
        {
            IList<Coord> toReturn = new List<Coord>();

            for (int line = 0; line < board.Length; line++)
            {
                for (int col = 0; col < board.Length; col++)
                {
                    if (board[line][col].Piece.HasValue && board[line][col].Piece.Value == pieceType && board[line][col].PieceColor.Value == pieceColor)
                    {
                        toReturn.Add(new Coord(line, col));
                    }
                }
            }

            return toReturn;
        }
    }
}

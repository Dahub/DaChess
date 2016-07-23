using System;
using System.Collections.Generic;
using System.Linq;

namespace DaChess.Business
{
    internal static class BoardsHelper
    {
        internal static Board GetClassic(ChessEntities context)
        {
            return context.Boards.Where(b => b.Id.Equals(1)).FirstOrDefault();
        }

        internal static bool IsLegalMove(BoardCase startCase, BoardCase endCase, IList<BoardCase> board)
        {
            if (!String.IsNullOrEmpty(endCase.Piece) && startCase.PieceColor == endCase.PieceColor)
            {
                return false;
            }

            int startLine = Int32.Parse(startCase.Line);
            int endLine = Int32.Parse(endCase.Line);
            int startCol = ColToInt(startCase.Col);
            int endCol = ColToInt(endCase.Col);
            switch (startCase.PieceType)
            {
                case PiecesType.PAWN:
                    if (String.IsNullOrEmpty(endCase.Piece) && startCol == endCol)
                    {
                        if (startCase.PieceColor == Colors.WHITE && endLine - startLine == 1)
                            return true;
                        if (startCase.PieceColor == Colors.BLACK && startLine - endLine == 1)
                            return true;
                        if (startCase.PieceColor == Colors.WHITE && endLine - startLine == 2
                            && startCase.HasMove == false
                            && EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                            return true;
                        if (startCase.PieceColor == Colors.BLACK && startLine - endLine == 2 && startCase.HasMove == false
                            && EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                            return true;
                        return false;
                    }
                    else if (!String.IsNullOrEmpty(endCase.Piece) && Math.Abs(startCol - endCol) == 1)
                    {
                        if (startCase.PieceColor == Colors.WHITE
                            && endLine - startLine == 1
                            && Math.Abs(startCol - endCol) == 1)
                            return true;
                        if (startCase.PieceColor == Colors.BLACK
                          && startLine - endLine == 1
                          && Math.Abs(startCol - endCol) == 1)
                            return true;
                    }
                    return false;
                case PiecesType.ROOK:
                    if (startLine != endLine && startCol != endCol)
                        return false;
                    if (!EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                        return false;
                    return true;
                case PiecesType.KNIGHT:
                    if (Math.Abs(startLine - endLine) == 2 && Math.Abs(startCol - endCol) == 1)
                        return true;
                    if (Math.Abs(startLine - endLine) == 1 && Math.Abs(startCol - endCol) == 2)
                        return true;
                    return false;
                case PiecesType.BISHOP:
                    if (startLine == endLine || startCol == endCol)
                        return false;
                    if (Math.Abs(endLine - startLine) != Math.Abs(endCol - startCol))
                        return false;
                    if (!EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                        return false;
                    return true;
                case PiecesType.QUEEN:
                    if (startLine == endLine || startCol == endCol && EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                        return true;
                    if (Math.Abs(endLine - startLine) == Math.Abs(endCol - startCol) && EmptyBeetwenToCases(startCase, endCase, board, startCol, endCol, startLine, endLine))
                        return true;
                    return false;
                case PiecesType.KING:
                    if (Math.Abs(endLine - startLine) <= 1 && Math.Abs(endCol - startCol) <= 1)
                        return true;
                    return false;
            }
            return true;
        }

        private static bool EmptyBeetwenToCases(BoardCase startCase, BoardCase endCase, IList<BoardCase> board, int startCol, int endCol, int startLine, int endLine)
        {
            int security = 0;

            int linePoint = startLine;
            int colPoint = startCol;
            if (startLine < endLine)
            {
                linePoint++;
            }
            else if (endLine < startLine)
            {
                linePoint--;
            }
            if (startCol < endCol)
            {
                colPoint++;
            }
            else if (endCol < startCol)
            {
                colPoint--;
            }
            while (linePoint != endLine || colPoint != endCol)
            {
                security++;
                BoardCase c = FindCase(IntToCol(colPoint), linePoint.ToString(), board);
                if (c != null && !String.IsNullOrEmpty(c.Piece))
                    return false;

                if (startLine < endLine)
                {
                    linePoint++;
                }
                else if (endLine < startLine)
                {
                    linePoint--;
                }
                if (startCol < endCol)
                {
                    colPoint++;
                }
                else if (endCol < startCol)
                {
                    colPoint--;
                }

                if (security > 26)
                {
                    throw new DaChessException("Erreur lors de l'analyse d'un trajet diagonal");
                }
            }
            return true;
        }

        internal static int ColToInt(string col)
        {
            // a => 1
            // b => 2
            // ...
            return (int)(col.ToLower().ToCharArray().First()) - 96;
        }

        internal static string IntToCol(int col)
        {
            return ((char)(col + 96)).ToString();
        }

        internal static BoardCase FindCase(string col, string line, IList<BoardCase> cases)
        {
            return cases.Where(b => b.Col.ToUpper().Equals(col.ToUpper()) && b.Line.ToUpper().Equals(line.ToUpper())).FirstOrDefault();
        }
    }
}

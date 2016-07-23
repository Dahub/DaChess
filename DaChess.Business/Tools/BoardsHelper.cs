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

        internal static bool IsLegalMove(CaseInfo startCase, CaseInfo endCase, CaseInfo[][] board, int startLine, int endLine, int startCol, int endCol, Party party, out MovesType moveType)
        {
            if(endCase.Piece.HasValue)
                moveType = MovesType.Capture;
            else
                moveType = MovesType.Classic;

            if (endCase.Piece.HasValue && startCase.PieceColor == endCase.PieceColor)
                return false;

            switch(startCase.Piece)
            {
                case PiecesType.PAWN:
                    if (!endCase.Piece.HasValue && startCol == endCol)
                    {
                        if (startCase.PieceColor == Colors.WHITE && endLine - startLine == 1)
                            return true;
                        if (startCase.PieceColor == Colors.BLACK && startLine - endLine == 1)
                            return true;
                        if (startCase.PieceColor == Colors.WHITE && endLine - startLine == 2
                            && startCase.HasMove == false
                            && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                            return true;
                        if (startCase.PieceColor == Colors.BLACK && startLine - endLine == 2 && startCase.HasMove == false
                            && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                            return true;
                        return false;
                    }
                    else if (endCase.Piece.HasValue && Math.Abs(startCol - endCol) == 1)
                    {
                        if (startCase.PieceColor == Colors.WHITE && endLine - startLine == 1 && Math.Abs(startCol - endCol) == 1)
                            return true;
                        if (startCase.PieceColor == Colors.BLACK && startLine - endLine == 1 && Math.Abs(startCol - endCol) == 1)
                            return true;
                    }
                    // peut être une prise en passant ?
                    else if (!endCase.Piece.HasValue && Math.Abs(startCol - endCol) == 1 && !String.IsNullOrEmpty(party.EnPassantCase))
                    {
                        string epCol = party.EnPassantCase.Substring(0, 1);
                        string epLine = party.EnPassantCase.Substring(1, 1);
                        int epColInt = ColToInt(epCol) - 1;
                        int epLineInt = Int32.Parse(epLine) - 1;
                        CaseInfo epCase = board[epLineInt][epColInt];
                        if (epCase.Piece.HasValue && epCase.Piece == PiecesType.PAWN && epColInt == endCol && Math.Abs(epLineInt - endLine) == 1)
                        {
                            moveType = MovesType.EnPassant; 
                            return true;
                        }
                    }
                    return false;
                case PiecesType.ROOK:
                    if (startLine != endLine && startCol != endCol)
                        return false;
                    if (!EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
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
                    if (!EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return false;
                    return true;
                case PiecesType.QUEEN:
                    if (startLine == endLine || startCol == endCol && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return true;
                    if (Math.Abs(endLine - startLine) == Math.Abs(endCol - startCol) && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return true;
                    return false;
                case PiecesType.KING:
                    if (Math.Abs(endLine - startLine) <= 1 && Math.Abs(endCol - startCol) <= 1)
                        return true;
                    return false;
            }

            return true;
        }

        internal static bool IsCheck(Colors kingColor, CaseInfo[][] board)
        {
            Coord king = new Coord();
            IList<Coord> ennemies = new List<Coord>();

            // on commence par récupérer récupérer ce qui nous intéresse
            for(int line = 0; line < board.Length;line++)
            {
                for(int col = 0; col < board[line].Length; col++)
                {
                    if(board[line][col].Piece.HasValue)
                    {
                        if(board[line][col].Piece.Value == PiecesType.KING && board[line][col].PieceColor.Value == kingColor)
                        {
                            king = new Coord() { Line = line, Col = col };
                        }
                        else if(board[line][col].PieceColor.Value != kingColor)
                        {
                            ennemies.Add(new Coord() { Line = line, Col = col });
                        }
                    }
                }
            }
            foreach(Coord c in ennemies)
            {
                switch(board[c.Line][c.Col].Piece.Value)
                {
                    case PiecesType.PAWN:
                        if (board[c.Line][c.Col].PieceColor == Colors.WHITE)
                        {
                            if (king.Line == c.Line + 1 && Math.Abs(king.Col - c.Col) == 1)
                                return true;
                        }
                        else
                        {
                            if (king.Line == c.Line - 1 && Math.Abs(king.Col - c.Col) == 1)
                                return true;
                        }                        
                        break;
                    case PiecesType.BISHOP:
                        if (Math.Abs(king.Line - c.Line) == Math.Abs(king.Col - c.Col) && EmptyBeetwenToCases(board, c.Col, king.Col, c.Line, king.Line))
                            return true;
                        break;
                    case PiecesType.KNIGHT:
                        if (Math.Abs(c.Line - king.Line) == 2 && Math.Abs(c.Col - king.Col) == 1)
                            return true;
                        if (Math.Abs(c.Line - king.Line) == 1 && Math.Abs(c.Col - king.Col) == 2)
                            return true;
                        break;
                    case PiecesType.QUEEN:
                        if ((Math.Abs(king.Line - c.Line) == Math.Abs(king.Col - c.Col) || king.Line == c.Line ||king.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, king.Col, c.Line, king.Line))
                            return true;
                        break;
                    case PiecesType.ROOK:
                        if ((king.Line == c.Line || king.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, king.Col, c.Line, king.Line))
                            return true;
                        break;
                }
            }

            return false;
        }

        private static bool EmptyBeetwenToCases(CaseInfo[][] board, int startCol, int endCol, int startLine, int endLine)
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
                // BoardCase c = FindCase(IntToCol(colPoint), linePoint.ToString(), board);
                CaseInfo c = board[linePoint][colPoint];
                if (c.Piece.HasValue)
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
                    throw new DaChessException("Erreur lors de l'analyse d'un trajet");
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
    }
}

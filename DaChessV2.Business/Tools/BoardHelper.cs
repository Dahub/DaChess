using DaChessV2.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace DaChessV2.Business
{
    /// <summary>
    /// Boite à outils pour la gestion du plateau de jeu
    /// </summary>
    public static class BoardHelper
    {
        internal static BoardType GetClassic(ChessEntities context)
        {
            return context.BoardType.Where(b => b.Id.Equals(1)).FirstOrDefault();
        }

        internal static bool IsLegalMove(CaseInfo startCase, CaseInfo endCase, CaseInfo[][] board, int startLine, int endLine, int startCol, int endCol, Party party, out EnumMoveType moveType)
        {
            if (endCase.Piece.HasValue)
                moveType = EnumMoveType.CAPTURE;
            else
                moveType = EnumMoveType.CLASSIC;

            if (endCase.Piece.HasValue && startCase.PieceColor == endCase.PieceColor)
                return false;

            switch (startCase.Piece)
            {
                case EnumPieceType.PAWN:
                    // avons nous une promotion ?
                    if (startCase.PieceColor == Color.WHITE && endLine == board.Length - 1 || startCase.PieceColor == Color.BLACK && endLine == 0)
                    {
                        moveType = EnumMoveType.PROMOTE;
                    }
                    if (!endCase.Piece.HasValue && startCol == endCol)
                    {
                        if (startCase.PieceColor == Color.WHITE && endLine - startLine == 1)
                            return true;
                        if (startCase.PieceColor == Color.BLACK && startLine - endLine == 1)
                            return true;
                        if (startCase.PieceColor == Color.WHITE && endLine - startLine == 2
                            && startCase.HasMove == false
                            && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                            return true;
                        if (startCase.PieceColor == Color.BLACK && startLine - endLine == 2 && startCase.HasMove == false
                            && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                            return true;
                        return false;
                    }
                    else if (endCase.Piece.HasValue && Math.Abs(startCol - endCol) == 1)
                    {
                        if (startCase.PieceColor == Color.WHITE && endLine - startLine == 1 && Math.Abs(startCol - endCol) == 1)
                            return true;
                        if (startCase.PieceColor == Color.BLACK && startLine - endLine == 1 && Math.Abs(startCol - endCol) == 1)
                            return true;
                    }
                    // peut être une prise en passant ?
                    else if (!endCase.Piece.HasValue && Math.Abs(startCol - endCol) == 1 && !String.IsNullOrEmpty(party.EnPassantCase))
                    {
                        string epCol = party.EnPassantCase.Substring(0, 1);
                        string epLine = party.EnPassantCase.Substring(1, 1);
                        int epColInt = ColToInt(epCol) - 1;
                        int epLineInt = Int32.Parse(epLine) - 1;
                        if (board[epLineInt][epColInt].Piece.HasValue && board[epLineInt][epColInt].Piece == EnumPieceType.PAWN && epColInt == endCol && Math.Abs(epLineInt - endLine) == 1)
                        {
                            moveType = EnumMoveType.EN_PASSANT;
                            return true;
                        }
                    }
                    return false;
                case EnumPieceType.ROOK:
                    if (startLine != endLine && startCol != endCol)
                        return false;
                    if (!EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return false;
                    return true;
                case EnumPieceType.KNIGHT:
                    if (Math.Abs(startLine - endLine) == 2 && Math.Abs(startCol - endCol) == 1)
                        return true;
                    if (Math.Abs(startLine - endLine) == 1 && Math.Abs(startCol - endCol) == 2)
                        return true;
                    return false;
                case EnumPieceType.BISHOP:
                    if (startLine == endLine || startCol == endCol)
                        return false;
                    if (Math.Abs(endLine - startLine) != Math.Abs(endCol - startCol))
                        return false;
                    if (!EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return false;
                    return true;
                case EnumPieceType.QUEEN:
                    if (startLine == endLine || startCol == endCol && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return true;
                    if (Math.Abs(endLine - startLine) == Math.Abs(endCol - startCol) && EmptyBeetwenToCases(board, startCol, endCol, startLine, endLine))
                        return true;
                    return false;
                case EnumPieceType.KING:
                    if (Math.Abs(endLine - startLine) <= 1 && Math.Abs(endCol - startCol) <= 1)
                        return true;
                    // cas du roque
                    if (endLine == startLine && board[startLine][startCol].HasMove == false && Math.Abs(startCol - endCol) == 2)
                    {
                        if (startCol < endCol) // petit roque
                        {
                            CaseInfo rookCase = board[startLine][board[startLine].Length - 1];
                            if (rookCase.Piece.HasValue && rookCase.Piece.Value == EnumPieceType.ROOK && rookCase.HasMove.Value == false) // on a bien une tour sur la case attendue
                            {
                                if (EmptyBeetwenToCases(board, startCol, board[startLine].Length - 1, startLine, endLine)) // aucune case sur le chemin
                                {
                                    // il faut vérifier que la case intermédiaire n'est pas en prise
                                    if (!IsCaseInCapture(new Coord() { Line = startLine, Col = startCol + 1 }, board, rookCase.PieceColor.Value))
                                    {
                                        moveType = EnumMoveType.CASTLING_SHORT;
                                        return true;
                                    }
                                }
                            }
                        }
                        else // grand roque
                        {
                            CaseInfo rookCase = board[startLine][0];
                            if (rookCase.Piece.HasValue && rookCase.Piece.Value == EnumPieceType.ROOK && rookCase.HasMove.Value == false) // on a bien une tour sur la case attendue
                            {
                                if (EmptyBeetwenToCases(board, startCol, 0, startLine, endLine)) // aucune case sur le chemin
                                {
                                    if (!IsCaseInCapture(new Coord() { Line = startLine, Col = startCol - 1 }, board, rookCase.PieceColor.Value))
                                    {
                                        moveType = EnumMoveType.CASTLING_LONG;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    return false;
            }

            return true;
        }

        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board, IList<Coord> ennemies)
        {
            foreach (Coord c in ennemies)
            {
                switch (board[c.Line][c.Col].Piece.Value)
                {
                    case EnumPieceType.PAWN:
                        if (board[c.Line][c.Col].PieceColor == Color.WHITE)
                        {
                            if (toTest.Line == c.Line + 1 && Math.Abs(toTest.Col - c.Col) == 1)
                                return true;
                        }
                        else
                        {
                            if (toTest.Line == c.Line - 1 && Math.Abs(toTest.Col - c.Col) == 1)
                                return true;
                        }
                        break;
                    case EnumPieceType.BISHOP:
                        if (Math.Abs(toTest.Line - c.Line) == Math.Abs(toTest.Col - c.Col) && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                    case EnumPieceType.KNIGHT:
                        if (Math.Abs(c.Line - toTest.Line) == 2 && Math.Abs(c.Col - toTest.Col) == 1)
                            return true;
                        if (Math.Abs(c.Line - toTest.Line) == 1 && Math.Abs(c.Col - toTest.Col) == 2)
                            return true;
                        break;
                    case EnumPieceType.QUEEN:
                        if ((Math.Abs(toTest.Line - c.Line) == Math.Abs(toTest.Col - c.Col) || toTest.Line == c.Line || toTest.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                    case EnumPieceType.ROOK:
                        if ((toTest.Line == c.Line || toTest.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                }
            }

            return false;
        }

        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board, Color toTestColor)
        {
            IList<Coord> ennemies = BuildPieceList(board, toTestColor == Color.WHITE ? Color.BLACK : Color.WHITE);
            return IsCaseInCapture(toTest, board, ennemies);
        }

        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board)
        {
            if (!board[toTest.Line][toTest.Col].Piece.HasValue)
                return false;
            Color toTestColor = board[toTest.Line][toTest.Col].PieceColor.Value;
            return IsCaseInCapture(toTest, board, toTestColor);
        }

        internal static bool IsCheck(Color kingColor, CaseInfo[][] board)
        {
            Coord king = ExtractKing(kingColor, board);
            return IsCaseInCapture(king, board);
        }

        internal static bool IsCheckMat(Color kingColor, CaseInfo[][] board)
        {
            Coord kingCoord = ExtractKing(kingColor, board);
            IList<Coord> ennemies = BuildPieceList(board, kingColor == Color.WHITE ? Color.BLACK : Color.WHITE);

            // le roi ne doit pas pouvoir bouger on commence par tester le déplacement du roi en diagonal supérieur gauche, puis sens horaire
            if (KingCanMove(ennemies, board, kingCoord))
                return false;

            // si on ne peut pas bouger le roi, il faut tester tous les coups possibles et voir si un d'eux annule l'échec
            Coord cibleCase = new Coord();
            int boardSize = board.Length;
            foreach (var c in BuildPieceList(board, kingColor))
            {
                switch (board[c.Line][c.Col].Piece)
                {
                    case EnumPieceType.PAWN:
                        int direction = board[c.Line][c.Col].PieceColor == Color.WHITE ? 1 : -1;
                        // on tente d'avancer d'une case
                        if (IsEmpty(c.Line + 1 * direction, c.Col, board))
                        {
                            cibleCase = new Coord(c.Line + 1 * direction, c.Col);
                            if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                return false;
                        }

                        // on regarde si on peut avancer de 2
                        if (board[c.Line][c.Col].HasMove == false && IsEmpty(c.Line + 1 * direction, c.Col, board) && IsEmpty(c.Line + 2 * direction, c.Col, board))
                        {
                            cibleCase = new Coord(c.Line + 2 * direction, c.Col);
                            if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                return false;
                        }

                        // on regarde la prise sur la gauche
                        if (c.Col > 0)
                        {
                            cibleCase = new Coord(c.Line + 1 * direction, c.Col - 1);
                            if (board[cibleCase.Line][cibleCase.Col].Piece.HasValue && board[cibleCase.Line][cibleCase.Col].PieceColor != kingColor && !IsKingInCheckAfterMove(c, cibleCase, board, kingColor)) // on a une pièce à prendre
                                return false;
                        }

                        // puis sur la droite 
                        if (c.Col < boardSize - 1)
                        {
                            cibleCase = new Coord(c.Line + 1 * direction, c.Col + 1);
                            if (board[cibleCase.Line][cibleCase.Col].Piece.HasValue && board[cibleCase.Line][cibleCase.Col].PieceColor != kingColor && !IsKingInCheckAfterMove(c, cibleCase, board, kingColor)) // on a une pièce à prendre
                                return false;
                        }

                        break;
                    case EnumPieceType.KNIGHT:
                        // 8 coups potentiellements possibles                       
                        // haut + gauche
                        if (c.Col > 0 && c.Line < board.Length - 2)
                        {
                            cibleCase = new Coord(c.Line + 2, c.Col - 1);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // haut + droite
                        if (c.Col < board.Length && c.Line < board.Length - 2)
                        {
                            cibleCase = new Coord(c.Line + 2, c.Col + 1);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + haut
                        if (c.Col < board.Length - 2 && c.Line < board.Length - 1)
                        {
                            cibleCase = new Coord(c.Line + 1, c.Col + 2);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + bas
                        if (c.Col < board.Length - 2 && c.Line > 0)
                        {
                            cibleCase = new Coord(c.Line - 1, c.Col + 2);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // bas + droite
                        if (c.Col < board.Length - 1 && c.Line > 1)
                        {
                            cibleCase = new Coord(c.Line - 2, c.Col + 1);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // bas + gauche
                        if (c.Col > 0 && c.Line > 1)
                        {
                            cibleCase = new Coord(c.Line - 2, c.Col - 1);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + bas
                        if (c.Col > 1 && c.Line > 0)
                        {
                            cibleCase = new Coord(c.Line - 1, c.Col - 2);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + haut
                        if (c.Col > 1 && c.Line < board.Length - 1)
                        {
                            cibleCase = new Coord(c.Line + 1, c.Col - 2);
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (!IsKingInCheckAfterMove(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        break;
                    case EnumPieceType.BISHOP:
                        // 4 directions
                        // en haut à droite
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, +1, c, board, kingColor))
                            return false;
                        // en bas à droite
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, -1, c, board, kingColor))
                            return false;
                        // en haut à gauche
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, +1, c, board, kingColor))
                            return false;
                        // en bas à gauche
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, -1, c, board, kingColor))
                            return false;
                        break;
                    case EnumPieceType.ROOK:
                        // 4 directions aussi
                        // en haut
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(0, +1, c, board, kingColor))
                            return false;
                        // en bas
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(0, -1, c, board, kingColor))
                            return false;
                        // à droite
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, 0, c, board, kingColor))
                            return false;
                        // à gauche
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, 0, c, board, kingColor))
                            return false;
                        break;
                    case EnumPieceType.QUEEN:
                        // 8 directions
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, +1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, -1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, +1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, -1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(0, +1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(0, -1, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(+1, 0, c, board, kingColor))
                            return false;
                        if (TryAllMoveDiagonalOrLateralToCancelCheck(-1, 0, c, board, kingColor))
                            return false;
                        break;
                }
            }

            return true;
        }

        internal static bool IsPat(Color kingColor, CaseInfo[][] board)
        {
            // on est pat si on ne peut jouer le roi sans être échec
            Coord kingCoord = ExtractKing(kingColor, board);
            IList<Coord> ennemies = BuildPieceList(board, kingColor == Color.WHITE ? Color.BLACK : Color.WHITE);

            // le roi ne doit pas pouvoir bouger on commence par tester le déplacement du roi en diagonal supérieur gauche, puis sens horaire
            // on test le roi en premier car il est rare qu'il ne puisse pas du tout se déplacer
            if (KingCanMove(ennemies, board, kingCoord))
                return false;

            // on regarde si on peut bouger une pièce
            IList<Coord> pieces = BuildPieceList(board, kingColor);

            // on vérifie les pions : déplacement + prise
            int direction = 1;
            if (kingColor == Color.BLACK)
                direction = -1;
            Coord endCase;
            foreach (var c in pieces)
            {
                if (board[c.Line][c.Col].Piece == EnumPieceType.PAWN)
                {
                    // déplacement
                    endCase = new Coord(c.Line + direction, c.Col);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && !board[endCase.Line][endCase.Col].Piece.HasValue
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // si la case devant le pion est vide, et pas d'échec provoqué par le move, pas PAT
                        return false;

                    // prise à gauche
                    endCase = new Coord(c.Line + direction, c.Col - 1);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col > 0 && endCase.Col < board.Length // on ne sort pas du plateau
                        && board[endCase.Line][endCase.Col].PieceColor.HasValue && board[endCase.Line][endCase.Col].PieceColor != kingColor // pièce en prise à l'arrivée
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // on ne déclenche pas mat
                        return false;

                    // priseà droite
                    endCase = new Coord(c.Line + direction, c.Col + 1);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col > 0 && endCase.Col < board.Length // on ne sort pas du plateau
                        && board[endCase.Line][endCase.Col].PieceColor.HasValue && board[endCase.Line][endCase.Col].PieceColor != kingColor // pièce en prise à l'arrivée
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // on ne déclenche pas mat
                        return false;
                }
                if (board[c.Line][c.Col].Piece == EnumPieceType.KNIGHT)
                {
                    endCase = new Coord(c.Line + 2, c.Col + 1); // en haut à droite
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line + 1, c.Col + 2); // à droite en haut
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line - 1, c.Col + 2); // à droite en bas
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line - 2, c.Col + 1); // en bas à droite
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line - 2, c.Col - 1); // en bas à gauche
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line - 1, c.Col - 2); // à gauche en bas
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line + 1, c.Col - 2); // à gauche en haut
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;

                    endCase = new Coord(c.Line + 2, c.Col - 1); // en haut à gauche
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                }
                if (board[c.Line][c.Col].Piece == EnumPieceType.BISHOP || board[c.Line][c.Col].Piece == EnumPieceType.QUEEN) // on test les diagonales => fou et reine
                {
                    endCase = new Coord(c.Line + 1, c.Col + 1); // en haut à droite
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                    endCase = new Coord(c.Line + 1, c.Col - 1); // etc...
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor))
                        return false;
                    endCase = new Coord(c.Line - 1, c.Col + 1);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                    endCase = new Coord(c.Line - 1, c.Col - 1);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                }
                if (board[c.Line][c.Col].Piece == EnumPieceType.ROOK || board[c.Line][c.Col].Piece == EnumPieceType.QUEEN) // on test les verticales => tour et reine
                {
                    endCase = new Coord(c.Line, c.Col + 1); // en haut à droite
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 && // si on ne sort pas du plateau
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor) // que la case d'arrivée est libre ou d'une autre couleur
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                    endCase = new Coord(c.Line, c.Col - 1); // etc...
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor))
                        return false;
                    endCase = new Coord(c.Line - 1, c.Col);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                    endCase = new Coord(c.Line - 1, c.Col);
                    if (endCase.Line < board.Length && endCase.Line >= 0 && endCase.Col < board.Length && endCase.Col >= 0 &&
                        (!board[endCase.Line][endCase.Col].Piece.HasValue || board[endCase.Line][endCase.Col].PieceColor != kingColor)
                        && !IsKingInCheckAfterMove(c, endCase, board, kingColor)) // et qu'on ne met pas le roi en échec
                        return false;
                }
            }

            return true;
        }

        internal static bool IsTreeTimeSamePosition(string board, int idParty)
        {
            bool toReturn = false;

            using (var context = new ChessEntities())
            {
                if (context.PartyHistory.Where(h => h.FK_Party.Equals(idParty) && h.Board == board).Count() == 3)
                {
                    toReturn = true;
                }
            }

            return toReturn;
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

        internal static bool IsMoveOk(string[] cases)
        {
            if (cases.Length != 2)
                return false;
            if (cases[1].Length != 2)
                return false;
            if (cases[0].Length != 2)
                return false;

            return true;
        }

        internal static EnumPieceType GetPieceType(string piece)
        {
            if (piece.Contains("paw"))
                return EnumPieceType.PAWN;
            else if (piece.Contains("roo"))
                return EnumPieceType.ROOK;
            else if (piece.Contains("kni"))
                return EnumPieceType.KNIGHT;
            else if (piece.Contains("bis"))
                return EnumPieceType.BISHOP;
            else if (piece.Contains("que"))
                return EnumPieceType.QUEEN;
            else if (piece.Contains("kin"))
                return EnumPieceType.KING;

            throw new DaChessException("Pièce inconnue");
        }

        internal static EnumPlayerState DefineEnnemiState(out string resultText, out string moveResult, string move, Color playerColor, CaseInfo[][] boardCases, EnumPlayerState ennemiState)
        {
            EnumPlayerState toReturn = DefineEnnemiState(playerColor, boardCases, ennemiState);

            resultText = String.Empty;
            moveResult = move;
            switch (toReturn)
            {
                case EnumPlayerState.CHECK:
                    resultText = "Echec !";
                    moveResult = String.Concat(move, "+");
                    break;
                case EnumPlayerState.CHECK_MAT:
                    resultText = "Echec et Mat !";
                    moveResult = String.Concat(move, "++");
                    break;
                case EnumPlayerState.PAT:
                    resultText = "Pat !";
                    break;
            }

            return toReturn;
        }

        internal static EnumPlayerState DefineEnnemiState(Color playerColor, CaseInfo[][] boardCases, EnumPlayerState toReturn)
        {
            // je peux mettre échec l'adversaire mais je ne peux pas l'être à la fin de mon coup            
            if (IsCheck(playerColor == Color.BLACK ? Color.WHITE : Color.BLACK, boardCases))
            {
                // vérifier si on mat
                if (IsCheckMat(playerColor == Color.BLACK ? Color.WHITE : Color.BLACK, boardCases))
                    toReturn = EnumPlayerState.CHECK_MAT;
                else
                    toReturn = EnumPlayerState.CHECK;
            }
            else if (IsPat(playerColor == Color.BLACK ? Color.WHITE : Color.BLACK, boardCases))
                toReturn = EnumPlayerState.PAT;

            if (IsCheck(playerColor, boardCases))
                throw (new DaChessException("Coup impossible, échec !"));

            return toReturn;
        }

        internal static string ToJsonStringFromCaseInfo(CaseInfo[][] boardCases)
        {
            string toReturn = @"{{
		        ""board"":[{0}]}}";
            StringBuilder innerString = new StringBuilder();

            bool removeLastChar = false;
            for (int line = 0; line < boardCases.Length; line++)
            {
                for (int col = 0; col < boardCases[line].Length; col++)
                {
                    if (boardCases[line][col].Piece.HasValue)
                    {
                        innerString.Append(CaseInfoToJson(boardCases[line][col], col, line));
                        innerString.Append(',');
                        removeLastChar = true;
                    }
                }
            }
            if (removeLastChar)
                innerString.Remove(innerString.Length - 1, 1);

            return string.Format(toReturn, innerString.ToString());
        }

        internal static CaseInfo[][] ToCaseInfoFromJsonString(string jsonBoard, int boardLenght)
        {
            CaseInfo[][] toReturn = new CaseInfo[boardLenght][];
            for (int i = 0; i < boardLenght; i++)
            {
                toReturn[i] = new CaseInfo[8];
            }

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(jsonBoard);
            object[] cases = (object[])(routes_list["board"]);

            for (int i = 0; i < cases.Length; i++)
            {
                IDictionary<string, object> myCase = (IDictionary<string, object>)cases[i];

                int col = BoardHelper.ColToInt(myCase["col"].ToString());
                int line = Int32.Parse(myCase["line"].ToString());

                CaseInfo toAdd = new CaseInfo()
                {
                    HasMove = Boolean.Parse(myCase["hasMove"].ToString()),
                    Piece = GetPieceType(myCase["piece"].ToString().ToLower()),
                    PieceColor = myCase["piece"].ToString().StartsWith("b") ? Color.BLACK : Color.WHITE
                };

                toReturn[line - 1][col - 1] = toAdd;
            }

            return toReturn;
        }

        internal static string ToBoardDescription(string JsonBoardCases, int boardCasesNumber)
        {
            CaseInfo[][] boardCases = BoardHelper.ToCaseInfoFromJsonString(JsonBoardCases, boardCasesNumber);
            return ToBoardDescription(boardCases);
        }

        internal static string ToBoardDescription(CaseInfo[][] boardCases)
        {
            StringBuilder toReturn = new StringBuilder();

            IList<string> myPieces = new List<string>();
            string pattern = "{0}{1}{2}{3}#"; // couleur, nom de pièce en anglais (K,Q,R,B,N,P)

            for (int line = 0; line < boardCases.Length; line++)
            {
                for (int col = 0; col < boardCases[line].Length; col++)
                {
                    if (boardCases[line][col].Piece.HasValue)
                    {
                        myPieces.Add(String.Format(pattern, (int)boardCases[line][col].PieceColor.Value,
                           (char)boardCases[line][col].Piece.Value, IntToCol(col + 1), line + 1));
                    }
                }
            }

            // on met les pièces dans l'ordre
            foreach (var p in myPieces.OrderBy(p => p))
            {
                toReturn.Append(p);
            }

            return toReturn.ToString();
        }

        #region private

        private static string CaseInfoToJson(CaseInfo c, int col, int line)
        {
            return string.Format(@"{{
                ""col"" :""{0}"",
				""line"" : ""{1}"",
				""piece"" : ""{2}"",
                ""hasMove"" : ""{3}""
            }}", IntToCol(col + 1), line + 1, PieceTypeToString(c.Piece.Value, c.PieceColor.Value), c.HasMove.ToString());
        }

        private static string PieceTypeToString(EnumPieceType pt, Color c)
        {
            string toReturn = String.Empty;
            string pattern = "{0}_{1}";
            string color = c == Color.BLACK ? "b" : "w";
            switch (pt)
            {
                case EnumPieceType.BISHOP:
                    toReturn = String.Format(pattern, color, "bishop");
                    break;
                case EnumPieceType.KING:
                    toReturn = String.Format(pattern, color, "king");
                    break;
                case EnumPieceType.KNIGHT:
                    toReturn = String.Format(pattern, color, "knight");
                    break;
                case EnumPieceType.PAWN:
                    toReturn = String.Format(pattern, color, "pawn");
                    break;
                case EnumPieceType.QUEEN:
                    toReturn = String.Format(pattern, color, "queen");
                    break;
                case EnumPieceType.ROOK:
                    toReturn = String.Format(pattern, color, "rook");
                    break;
            }
            return toReturn;
        }

        private static bool TryAllMoveDiagonalOrLateralToCancelCheck(int colDirection, int lineDirection, Coord c, CaseInfo[][] board, Color kingColor)
        {
            int col = c.Col;
            int line = c.Line;
            bool continueTest = true;
            while (continueTest)
            {
                col += colDirection; // on bouge d'une case
                line += lineDirection;
                if (col > 0 && line > 0 && col < board.Length - 1 && line < board.Length - 1) // tant qu'on n'a pas touché un bord 
                {
                    if (!board[line][col].Piece.HasValue || board[line][col].PieceColor != kingColor) // on avance tant que la cible est vide ou a une pièce d'une autre couleur
                    {
                        if (!IsKingInCheckAfterMove(c, new Coord() { Col = col, Line = line }, board, kingColor))
                            return true;

                        if (board[line][col].PieceColor != kingColor) // c'était une prise, on arrête là
                            continueTest = false;
                    }
                    else
                    {
                        continueTest = false;
                    }
                }
                else
                {
                    continueTest = false;
                }
            }

            return false;
        }

        private static bool KingCanMove(IList<Coord> ennemies, CaseInfo[][] board, Coord kingCoord)
        {
            int boardSize = board.Length - 1;
            int line = kingCoord.Line;
            int col = kingCoord.Col;
            if (line > 0 && col < boardSize) // coin inférieur droit
            {
                if (IsEmpty(line - 1, col + 1, board) && !IsCaseInCapture(new Coord { Line = line - 1, Col = col + 1 }, board, ennemies))
                    return true;
            }
            if (col < boardSize)
            {// case latérale droite
                if (IsEmpty(line, col + 1, board) && !IsCaseInCapture(new Coord { Line = line, Col = col + 1 }, board, ennemies))
                    return true;
            }
            if (col < boardSize && line < boardSize) // coin supérieur droit
            {
                if (IsEmpty(line + 1, col + 1, board) && !IsCaseInCapture(new Coord { Line = line + 1, Col = col + 1 }, board, ennemies))
                    return true;
            }
            if (line < boardSize) // case supérieure
            {
                if (IsEmpty(line + 1, col, board) && !IsCaseInCapture(new Coord { Line = line + 1, Col = col }, board, ennemies))
                    return true;
            }
            if (line < boardSize && col > 0) // coin supérieur gauche
            {
                if (IsEmpty(line + 1, col - 1, board) && !IsCaseInCapture(new Coord { Line = line + 1, Col = col - 1 }, board, ennemies))
                    return true;
            }
            if (col > 0)// case latérale gauche
            {
                if (IsEmpty(line, col - 1, board) && !IsCaseInCapture(new Coord { Line = line, Col = col - 1 }, board, ennemies))
                    return true;
            }
            if (col > 0 && line > 0)// coin inférieur gauche
            {
                if (IsEmpty(line - 1, col - 1, board) && !IsCaseInCapture(new Coord { Line = line - 1, Col = col - 1 }, board, ennemies))
                    return true;
            }
            if (line > 0) // case inférieure
            {
                if (IsEmpty(line - 1, col, board) && !IsCaseInCapture(new Coord { Line = line - 1, Col = col }, board, ennemies))
                    return true;
            }

            return false;
        }

        private static bool IsKingInCheckAfterMove(Coord startCase, Coord endCase, CaseInfo[][] board, Color kingColor)
        {
            bool toReturn = true;

            CaseInfo backUpStart = new CaseInfo()
            {
                HasMove = board[startCase.Line][startCase.Col].HasMove,
                Piece = board[startCase.Line][startCase.Col].Piece,
                PieceColor = board[startCase.Line][startCase.Col].PieceColor
            };
            CaseInfo backupEnd = new CaseInfo()
            {
                HasMove = board[endCase.Line][endCase.Col].HasMove,
                Piece = board[endCase.Line][endCase.Col].Piece,
                PieceColor = board[endCase.Line][endCase.Col].PieceColor
            };
            SimulateMove(startCase, endCase, board);
            if (!IsCheck(kingColor, board))
            {
                toReturn = false;
            }
            board[startCase.Line][startCase.Col] = backUpStart;
            board[endCase.Line][endCase.Col] = backupEnd;
            return toReturn;
        }

        private static void SimulateMove(Coord start, Coord end, CaseInfo[][] board)
        {
            board[end.Line][end.Col].HasMove = board[start.Line][start.Col].HasMove;
            board[end.Line][end.Col].Piece = board[start.Line][start.Col].Piece;
            board[end.Line][end.Col].PieceColor = board[start.Line][start.Col].PieceColor;
            board[start.Line][start.Col].HasMove = null;
            board[start.Line][start.Col].Piece = null;
            board[start.Line][start.Col].PieceColor = null;
        }

        private static IList<Coord> BuildPieceList(CaseInfo[][] board, Color toTestColor)
        {
            IList<Coord> ennemies = new List<Coord>();
            for (int line = 0; line < board.Length; line++)
            {
                for (int col = 0; col < board[line].Length; col++)
                {
                    if (board[line][col].Piece.HasValue)
                    {
                        if (board[line][col].PieceColor.Value == toTestColor)
                        {
                            ennemies.Add(new Coord() { Line = line, Col = col });
                        }
                    }
                }
            }
            return ennemies;
        }

        private static bool IsEmpty(int line, int col, CaseInfo[][] board)
        {
            return !board[line][col].Piece.HasValue;
        }

        private static Coord ExtractKing(Color kingColor, CaseInfo[][] board)
        {
            Coord king = new Coord();
            // on récupère la case du roi
            for (int line = 0; line < board.Length; line++)
            {
                for (int col = 0; col < board[line].Length; col++)
                {
                    if (board[line][col].Piece.HasValue)
                    {
                        if (board[line][col].Piece.Value == EnumPieceType.KING && board[line][col].PieceColor.Value == kingColor)
                        {
                            king = new Coord() { Line = line, Col = col };
                            line = board.Length; // break la première boucle
                            break; // break la boucle imbriquée
                        }
                    }
                }
            }

            return king;
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

                if (board[linePoint][colPoint].Piece.HasValue)
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

        #endregion
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
        public EnumPieceType? Piece { get; set; }
        public Color? PieceColor { get; set; }
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

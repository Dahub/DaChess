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
            if (endCase.Piece.HasValue)
                moveType = MovesType.CAPTURE;
            else
                moveType = MovesType.CLASSIC;

            if (endCase.Piece.HasValue && startCase.PieceColor == endCase.PieceColor)
                return false;

            switch (startCase.Piece)
            {
                case PiecesType.PAWN:
                    // avons nous une promotion ?
                    if (startCase.PieceColor == Colors.WHITE && endLine == board.Length - 1 || startCase.PieceColor == Colors.BLACK && endLine == 0)
                    {
                        moveType = MovesType.PROMOTE;
                    }
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
                            moveType = MovesType.EN_PASSANT;
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
                    // cas du roque
                    if (endLine == startLine && board[startLine][endLine].HasMove == false && Math.Abs(startCol - endCol) == 2)
                    {
                        if (startCol < endCol) // petit roque
                        {
                            CaseInfo rookCase = board[startLine][board[startLine].Length - 1];
                            if (rookCase.Piece.HasValue && rookCase.Piece.Value == PiecesType.ROOK && rookCase.HasMove.Value == false) // on a bien une tour sur la case attendue
                            {
                                if (EmptyBeetwenToCases(board, startCol, board[startLine].Length - 1, startLine, endLine)) // aucune case sur le chemin
                                {
                                    // il faut vérifier que la case intermédiaire n'est pas en prise
                                    if (!IsCaseInCapture(new Coord() { Line = startLine, Col = startCol + 1 }, board, rookCase.PieceColor.Value))
                                    {
                                        moveType = MovesType.CASTLING_SHORT;
                                        return true;
                                    }
                                }
                            }
                        }
                        else // grand roque
                        {
                            CaseInfo rookCase = board[startLine][0];
                            if (rookCase.Piece.HasValue && rookCase.Piece.Value == PiecesType.ROOK && rookCase.HasMove.Value == false) // on a bien une tour sur la case attendue
                            {
                                if (EmptyBeetwenToCases(board, startCol, 0, startLine, endLine)) // aucune case sur le chemin
                                {
                                    if (!IsCaseInCapture(new Coord() { Line = startLine, Col = startCol - 1 }, board, rookCase.PieceColor.Value))
                                    {
                                        moveType = MovesType.CASTLING_LONG;
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

        /// <summary>
        /// On connait déjà les pièces ennemies, pas besoin de les rechercher
        /// utilisée pour savoir si il y a mat
        /// </summary>
        /// <param name="toTest"></param>
        /// <param name="board"></param>
        /// <param name="ennemies"></param>
        /// <returns></returns>
        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board, IList<Coord> ennemies)
        {
            foreach (Coord c in ennemies)
            {
                switch (board[c.Line][c.Col].Piece.Value)
                {
                    case PiecesType.PAWN:
                        if (board[c.Line][c.Col].PieceColor == Colors.WHITE)
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
                    case PiecesType.BISHOP:
                        if (Math.Abs(toTest.Line - c.Line) == Math.Abs(toTest.Col - c.Col) && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                    case PiecesType.KNIGHT:
                        if (Math.Abs(c.Line - toTest.Line) == 2 && Math.Abs(c.Col - toTest.Col) == 1)
                            return true;
                        if (Math.Abs(c.Line - toTest.Line) == 1 && Math.Abs(c.Col - toTest.Col) == 2)
                            return true;
                        break;
                    case PiecesType.QUEEN:
                        if ((Math.Abs(toTest.Line - c.Line) == Math.Abs(toTest.Col - c.Col) || toTest.Line == c.Line || toTest.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                    case PiecesType.ROOK:
                        if ((toTest.Line == c.Line || toTest.Col == c.Col)
                            && EmptyBeetwenToCases(board, c.Col, toTest.Col, c.Line, toTest.Line))
                            return true;
                        break;
                }
            }

            return false;
        }

        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board, Colors toTestColor)
        {
            IList<Coord> ennemies = BuildPieceList(board, toTestColor == Colors.WHITE ? Colors.BLACK : Colors.WHITE);
            return IsCaseInCapture(toTest, board, ennemies);
        }

        internal static bool IsCaseInCapture(Coord toTest, CaseInfo[][] board)
        {
            if (!board[toTest.Line][toTest.Col].Piece.HasValue)
                return false;
            Colors toTestColor = board[toTest.Line][toTest.Col].PieceColor.Value;
            return IsCaseInCapture(toTest, board, toTestColor);
        }

        internal static bool IsCheck(Colors kingColor, CaseInfo[][] board)
        {
            Coord king = ExtractKing(kingColor, board);
            return IsCaseInCapture(king, board);
        }

        internal static bool IsCheckMat(Colors kingColor, CaseInfo[][] board)
        {
            Coord kingCoord = ExtractKing(kingColor, board);
            IList<Coord> ennemies = BuildPieceList(board, kingColor == Colors.WHITE ? Colors.BLACK : Colors.WHITE);

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
                    case PiecesType.PAWN:
                        int direction = board[c.Line][c.Col].PieceColor == Colors.WHITE ? 1 : -1;
                        // on tente d'avancer d'une case
                        if (IsEmpty(c.Line + 1 * direction, c.Col, board))
                        {
                            cibleCase.Col = c.Col;
                            cibleCase.Line = c.Line + 1 * direction;
                            if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                return false;
                        }

                        // on regarde si on peut avancer de 2
                        if (board[c.Line][c.Col].HasMove == false && IsEmpty(c.Line + 1 * direction, c.Col, board) && IsEmpty(c.Line + 2 * direction, c.Col, board))
                        {
                            cibleCase.Col = c.Col;
                            cibleCase.Line = c.Line + 2 * direction;
                            if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                return false;
                        }

                        // on regarde la prise sur la gauche
                        if (c.Col > 0)
                        {
                            cibleCase.Col = c.Col - 1;
                            cibleCase.Line = c.Line + 1 * direction;
                            if (board[cibleCase.Line][cibleCase.Col].Piece.HasValue && board[cibleCase.Line][cibleCase.Col].PieceColor != kingColor) // on a une pièce à prendre
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }

                        // puis sur la droite 
                        if (c.Col < boardSize - 1)
                        {
                            cibleCase.Col = c.Col + 1;
                            cibleCase.Line = c.Line + 1 * direction;
                            if (board[cibleCase.Line][cibleCase.Col].Piece.HasValue && board[cibleCase.Line][cibleCase.Col].PieceColor != kingColor) // on a une pièce à prendre
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }

                        break;
                    case PiecesType.KNIGHT:
                        // 8 coups potentiellements possibles                       
                        // haut + gauche
                        if (c.Col > 0 && c.Line < board.Length - 2)
                        {
                            cibleCase.Col = c.Col - 1;
                            cibleCase.Line = c.Line + 2;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // haut + droite
                        if (c.Col < board.Length && c.Line < board.Length - 2)
                        {
                            cibleCase.Col = c.Col + 1;
                            cibleCase.Line = c.Line + 2;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + haut
                        if (c.Col < board.Length - 2 && c.Line < board.Length - 1)
                        {
                            cibleCase.Col = c.Col + 2;
                            cibleCase.Line = c.Line + 1;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + bas
                        if (c.Col < board.Length - 2 && c.Line > 0)
                        {
                            cibleCase.Col = c.Col + 2;
                            cibleCase.Line = c.Line - 1;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // bas + droite
                        if (c.Col < board.Length - 1 && c.Line > 1)
                        {
                            cibleCase.Col = c.Col + 1;
                            cibleCase.Line = c.Line - 2;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // bas + gauche
                        if (c.Col > 0 && c.Line > 1)
                        {
                            cibleCase.Col = c.Col - 1;
                            cibleCase.Line = c.Line - 2;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + bas
                        if (c.Col > 1 && c.Line > 0)
                        {
                            cibleCase.Col = c.Col - 2;
                            cibleCase.Line = c.Line - 1;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }
                        // droite + haut
                        if (c.Col > 1 && c.Line < board.Length - 1)
                        {
                            cibleCase.Col = c.Col - 2;
                            cibleCase.Line = c.Line + 1;
                            if (!board[cibleCase.Line][cibleCase.Col].Piece.HasValue || board[cibleCase.Line][cibleCase.Col].PieceColor.Value != kingColor)
                            {
                                if (IsMoveCancelCheck(c, cibleCase, board, kingColor))
                                    return false;
                            }
                        }

                        break;
                    case PiecesType.BISHOP:
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
                    case PiecesType.ROOK:
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
                    case PiecesType.QUEEN:
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

        internal static bool IsPat(Colors kingColor, CaseInfo[][] board)
        {
            // on est pat si on ne peut jouer le roi sans être échec
            Coord kingCoord = ExtractKing(kingColor, board);
            IList<Coord> ennemies = BuildPieceList(board, kingColor == Colors.WHITE ? Colors.BLACK : Colors.WHITE);

            // le roi ne doit pas pouvoir bouger on commence par tester le déplacement du roi en diagonal supérieur gauche, puis sens horaire
            if (KingCanMove(ennemies, board, kingCoord))
                return false;

            // on regarde si on peut bouger une pièce
            IList<Coord> pieces = BuildPieceList(board, kingColor);

            // on vérifie les pions : déplacement + prise
            int direction = 1;
            if (kingColor == Colors.BLACK)
                direction = -1;
            


            // ensuite les autres pièces, déplacement d'1 case suffit (avec prise)

            return true;
        }

        private static bool TryAllMoveDiagonalOrLateralToCancelCheck(int colDirection, int lineDirection, Coord c, CaseInfo[][] board, Colors kingColor)
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
                        if (IsMoveCancelCheck(c, new Coord() { Col = col, Line = line }, board, kingColor))
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

        private static bool IsMoveCancelCheck(Coord startCase, Coord endCase, CaseInfo[][] board, Colors kingColor)
        {
            bool toReturn = false;

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
                toReturn = true;
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

        private static IList<Coord> BuildPieceList(CaseInfo[][] board, Colors toTestColor)
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

        private static Coord ExtractKing(Colors kingColor, CaseInfo[][] board)
        {
            Coord king = new Coord();
            // on récupère la case du roi
            for (int line = 0; line < board.Length; line++)
            {
                for (int col = 0; col < board[line].Length; col++)
                {
                    if (board[line][col].Piece.HasValue)
                    {
                        if (board[line][col].Piece.Value == PiecesType.KING && board[line][col].PieceColor.Value == kingColor)
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

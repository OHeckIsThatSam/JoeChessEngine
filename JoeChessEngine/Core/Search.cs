using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core;

public class Search
{
    public Move BestMove = new();

    public int maxSearchTime;
    public int maxDepth;

    public static int SearchMoves(Board position, int depth)
    {
        if (0.Equals(depth))
        {
            return Evaluation.Evaluate(position);
        }

        int score = int.MinValue;

        foreach (Move move in GenerateMoves(position))
        {
            position.MakeMove(move);

            int currentScore = -SearchMoves(position, depth - 1);

            if (currentScore > score)
                score = currentScore;

            position.ReverseMove(move);
        }
        return score;
    }

    /// <summary>
    /// Generates all Legal moves from a given postion (state of the board)
    /// </summary>
    /// <param name="position">The board state</param>
    /// <returns>Legal Moves list</returns>
    public static List<Move> GenerateMoves(Board position)
    {
        List<Move> moves = [];

        // Generate all the possible moves in a position.

        // If in check? Flag on previous position
        // Then have to move king, block check, or take piece.
        // If in doulbe check then king move only

        // Generate all piece moves or captures?
        // Moves
        // Castling
        // If can castle flag
        // If king does not castle into or cross check generate moves

        // Pawn
        Bitboard pawns = position.PieceBitboards[Piece.Pawn | position.ColourToMove];
        foreach (int startSquare in pawns.GetActiveBits())
        {
            Bitboard pawnBitboard = new();
            pawnBitboard.AddBit(startSquare);

            Console.WriteLine("Calculating moves and attacks for pawn:");
            Console.WriteLine(pawnBitboard);

            Bitboard pawnMoves = new();
            Bitboard pawnAttacks = new();

            if (position.ColourToMove == Piece.White)
            {
                // AND attacks for the pawn with oppostion pieces
                pawnAttacks = AttackBitboards.PawnAttacks[0, startSquare].Copy();
                pawnAttacks.And(position.PieceBitboards[Piece.Black]);

                // Enpassant-have flag on previous position if en passant is possible?
                // If Enpassant is possible generate those move(s)

                Console.WriteLine($"Pawn Attacks \n {pawnAttacks}");

                // Check for promotion
                // Populate move Bitboard with forward move if empty square
                pawnMoves = pawnBitboard.ShiftRight(BitboardUtilities.PawnForward);
                pawnMoves.And(position.EmptyBitboard);

                // Check if double move is valid (on second rank and not blocked)
                if (pawnBitboard.Mask(BitboardUtilities.RankMask2) != 0 &&
                    !pawnMoves.IsEmpty())
                {
                    pawnMoves.Combine(
                        pawnMoves.ShiftRight(BitboardUtilities.PawnForward));

                    pawnMoves.And(position.EmptyBitboard);
                }

                Console.WriteLine($"Pawn Moves \n {pawnMoves}");
            }
            else
            {
                pawnAttacks = AttackBitboards.PawnAttacks[1, startSquare].Copy();
                pawnAttacks.And(position.PieceBitboards[Piece.White]);

                // Enpassant-have flag on previous position if en passant is possible?
                // If Enpassant is possible generate those move(s)

                Console.WriteLine($"Pawn Attacks \n {pawnAttacks}");

                pawnMoves = pawnBitboard.ShiftLeft(BitboardUtilities.PawnForward);
                pawnMoves.And(position.EmptyBitboard);

                if (pawnBitboard.Mask(BitboardUtilities.RankMask7) != 0 &&
                    !pawnMoves.IsEmpty())
                {
                    pawnMoves.Combine(
                        pawnMoves.ShiftLeft(BitboardUtilities.PawnForward));

                    pawnMoves.And(position.EmptyBitboard);
                }

                Console.WriteLine($"Pawn Moves \n {pawnMoves}");
            }

            moves.AddRange(ParseLegalMoves(position, startSquare, pawnAttacks));

            moves.AddRange(ParseLegalMoves(position, startSquare, pawnMoves));
        }


        Console.WriteLine("Getting moves for knights");

        // Get and loop through the positions of knights on the board
        Bitboard knights = position.PieceBitboards[Piece.Knight | position.ColourToMove];
        Console.WriteLine(knights.ToString());
        foreach (int startSquare in knights.GetActiveBits())
        {
            // Get attacks for a knight on that square
            Bitboard knightAttacks = AttackBitboards.KnightAttacks[startSquare].Copy();

            // AND the friendly pieces to the bitboard to give blocked attacks
            knightAttacks.And(position.PieceBitboards[position.ColourToMove]);

            // XOR the blocked attacks with normal attacks giving unblocked attacks
            // or captures
            knightAttacks.ExclusiveCombine(AttackBitboards.KnightAttacks[startSquare]);

            Console.WriteLine($"Knight attacks on: {startSquare}");
            Console.WriteLine(knightAttacks.ToString());

            moves.AddRange(ParseLegalMoves(position, startSquare, knightAttacks));
        }

        Console.WriteLine("Getting moves for bishops");
        Bitboard bishops = position.PieceBitboards[Piece.Bishop | position.ColourToMove];
        foreach (int startSquare in bishops.GetActiveBits())
        {
            // Calculate attacks for a bishop on that square given other pieces on the board 
            Bitboard bishopAttacks = AttackBitboards
                .GenerateBishopAttacks(startSquare, position.OccupiedBitboard);

            // AND friendly pieces to get blocked attacks
            Bitboard bishopBlockedAttacks = bishopAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            // XOR blocked attacks to give only unblocked attacks and captures
            bishopAttacks.ExclusiveCombine(bishopBlockedAttacks);

            Console.WriteLine($"Bishop attacks on: {startSquare}");
            Console.WriteLine(bishopAttacks);

            moves.AddRange(ParseLegalMoves(position, startSquare, bishopAttacks));
        }

        Console.WriteLine("Getting moves for rooks");
        Bitboard rooks = position.PieceBitboards[Piece.Rook | position.ColourToMove];
        foreach (int startSquare in rooks.GetActiveBits())
        {
            Bitboard rookAttacks = AttackBitboards
                .GenerateRookAttacks(startSquare, position.OccupiedBitboard);

            Bitboard rookBlockedAttacks = rookAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            rookAttacks.ExclusiveCombine(rookBlockedAttacks);

            Console.WriteLine($"Rook attacks on: {startSquare}");
            Console.WriteLine(rookAttacks);

            moves.AddRange(ParseLegalMoves(position, startSquare, rookAttacks));
        }

        Console.WriteLine("Getting moves for queen(s)");
        Bitboard queens = position.PieceBitboards[Piece.Queen | position.ColourToMove];
        foreach (int startSquare in queens.GetActiveBits())
        {
            Bitboard queenAttacks = AttackBitboards
                .GenerateQueenAttacks(startSquare, position.OccupiedBitboard);

            Bitboard queenBlockedAttacks = queenAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            queenAttacks.ExclusiveCombine(queenBlockedAttacks);

            Console.WriteLine($"Queen attacks on: {startSquare}");
            Console.WriteLine(queenAttacks);

            moves.AddRange(ParseLegalMoves(position, startSquare, queenAttacks));
        }

        Bitboard king = position.PieceBitboards[Piece.King | position.ColourToMove];
        int sqaure = king.GetLeastSignificantBit();
        Bitboard kingAttacks = AttackBitboards.KingAttacks[sqaure].Copy();
        // Remove king from board
        // Remove all attacked squares by oposition colour
        // Remove all squares occupied by friendly pieces
        // Remove all squares occupied by a protected opposition piece

        return moves;
    }

    private static List<Move> ParseLegalMoves(Board position, int startSquare, Bitboard bitboard)
    {
        List<Move> moves = [];

        if (bitboard.IsEmpty())
            return moves;

        foreach (int targetSquare in bitboard.GetActiveBits())
        {
            bool isCapture = position.OccupiedBitboard.GetBit(targetSquare) != 0;

            bool isLegal = true;

            // Check legality
            // Causes king to be in check?

            if (!isLegal)
                continue;

            moves.Add(new()
            {
                StartSqaure = startSquare,
                TargetSqaure = targetSquare,
                IsCapture = isCapture
            });
        }

        return moves;
    }
}

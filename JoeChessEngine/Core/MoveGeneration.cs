using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core;

public static class MoveGeneration
{
    /// <summary>
    /// Generates all legal moves from a given position (state of the board)
    /// </summary>
    /// <param name="position">The board state</param>
    /// <returns>Legal Moves list</returns>
    public static List<Move> GenerateMoves(Board position)
    {
        List<Move> moves = [];

        int kingSquare = position
            .PieceBitboards[position.ColourToMove | Piece.King]
            .GetLeastSignificantBit();

        moves.AddRange(KingMoves(position, kingSquare));

        Bitboard checkers = GetCheckers(position, kingSquare);

        // Double check, only king moves are possible to get out of check
        if (checkers.Count() > 1)
            return moves;

        /* Calculate a move mask based on if/what piece is checking the king.
         * The mask is used to limit the moves of the other pieces.
         * If the piece is a slider then moves that block and capture the 
         * attacker are allowed. Else only capturing the attack piece will stop
         * the check. By default all squares are allowed (all on bitboard).
         */
        Bitboard moveMask = new(ulong.MaxValue);
        if (checkers.Count() == 1)
        {
            moveMask = checkers;

            if (Piece.IsSlider(position.BoardSquares[checkers.GetLeastSignificantBit()]))
            {
                moveMask.Combine(AttackBitboards.GetAttackRay(
                    checkers.GetLeastSignificantBit(),
                    kingSquare));
            }
        }

        // TODO:
        // Castling
        // If can castle flag
        // If king does not castle into or cross check generate moves

        moves.AddRange(PawnMoves(position, moveMask));

        moves.AddRange(KnightMoves(position, moveMask));

        moves.AddRange(BishopMoves(position, moveMask));

        moves.AddRange(RookMoves(position, moveMask));

        moves.AddRange(QueenMoves(position, moveMask));

        return moves;
    }

    private static List<Move> KingMoves(Board position, int kingSquare)
    {
        /* Generate opposition attacks without friendly king to ensure moves that
         * leave the king in check (moving along a slider piece's attack) are not
         * created.
         */
        Bitboard blockers = position.OccupiedBitboard.Copy();
        blockers.RemoveBit(kingSquare);

        Bitboard opositionAttacks = AttackBitboards.GetAllAttacks(
            position.OpositionColour,
            blockers,
            position.PieceBitboards);

        Bitboard kingAttacks = AttackBitboards.KingAttacks[kingSquare].Copy();

        // Remove king attacks on squares attacked by the oposition
        Bitboard illegalKingAttacks = opositionAttacks.And(kingAttacks);

        // Remove king attacks blocked by friendly pieces
        Bitboard blockedKingAttacks = kingAttacks.Copy()
            .And(position.PieceBitboards[position.ColourToMove]);

        kingAttacks.ExclusiveCombine(
            illegalKingAttacks.Combine(blockedKingAttacks));

        return CreateMoves(position, kingSquare, kingAttacks);
    }

    private static List<Move> PawnMoves(Board position, Bitboard moveMask)
    {
        List<Move> moves = [];

        Bitboard pawns = position.PieceBitboards[Piece.Pawn | position.ColourToMove];
        foreach (int startSquare in pawns.GetActiveBits())
        {
            Bitboard pawnBitboard = new();
            pawnBitboard.AddBit(startSquare);

            Bitboard pawnMoves;
            Bitboard pawnAttacks = 
                AttackBitboards.PawnAttacks[position.ColourToMove, startSquare]
                    .Copy();

            Bitboard opositionPieces = position
                .PieceBitboards[position.OpositionColour].Copy();

            // Calculate and create En Passant moves seperately
            if (position.hasEnPassantTargetSquare)
            {
                // Calculate the square of the opponent pawn thats takeable
                int opponentPawnSquare = position.enPassantTargetSquare + 
                    (position.ColourToMove == Piece.White ? 8 : -8);

                // Add sqaure to oposition bitboard to make it attackable
                opositionPieces.AddBit(position.enPassantTargetSquare);

                /* Create seperate En Passant move mask that can contain the
                 * En Passant sqaure if the oppenents pawn is checking the king
                 */
                Bitboard enPassantMoveMask = moveMask.Copy();

                if (enPassantMoveMask.GetBit(opponentPawnSquare) != 0)
                    enPassantMoveMask.AddBit(position.enPassantTargetSquare);

                pawnAttacks.And(opositionPieces).And(enPassantMoveMask);

                foreach (int targetSquare in pawnAttacks.GetActiveBits())
                {
                    Move move;

                    move = new()
                    {
                        StartSqaure = startSquare,
                        TargetSqaure = targetSquare,
                        IsCapture = true,
                    };

                    if (position.enPassantTargetSquare == targetSquare)
                    {
                        move.IsEnPassant = true;
                        move.TargetPawnSqaure = opponentPawnSquare;
                    }

                    moves.Add(move);
                }
            } 
            else
            {
                pawnAttacks.And(opositionPieces).And(moveMask);

                moves.AddRange(CreateMoves(position, startSquare, pawnAttacks));
            }

            if (position.ColourToMove == Piece.White)
            {
                // TODO:
                // Promotion

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
            }
            else
            {
                pawnMoves = pawnBitboard.ShiftLeft(BitboardUtilities.PawnForward);
                pawnMoves.And(position.EmptyBitboard);

                if (pawnBitboard.Mask(BitboardUtilities.RankMask7) != 0 &&
                    !pawnMoves.IsEmpty())
                {
                    pawnMoves.Combine(
                        pawnMoves.ShiftLeft(BitboardUtilities.PawnForward));

                    pawnMoves.And(position.EmptyBitboard);
                }
            }

            pawnMoves.And(moveMask);
        
            moves.AddRange(CreateMoves(position, startSquare, pawnMoves));
        }

        return moves;
    }

    private static List<Move> KnightMoves(Board position, Bitboard moveMask)
    {
        List<Move> moves = [];

        Bitboard knights = 
            position.PieceBitboards[Piece.Knight | position.ColourToMove];

        foreach (int startSquare in knights.GetActiveBits())
        {
            // Get attacks for a knight on that square
            Bitboard knightAttacks = 
                AttackBitboards.KnightAttacks[startSquare].Copy();

            // AND the friendly pieces to the bitboard to give blocked attacks
            knightAttacks.And(position.PieceBitboards[position.ColourToMove]);

            // XOR the blocked attacks with normal attacks giving unblocked attacks
            // or captures
            knightAttacks.ExclusiveCombine(
                AttackBitboards.KnightAttacks[startSquare]);

            knightAttacks.And(moveMask);

            moves.AddRange(CreateMoves(position, startSquare, knightAttacks));
        }

        return moves;
    }

    private static List<Move> BishopMoves(Board position, Bitboard moveMask)
    {
        List<Move> moves = [];

        Bitboard bishops = 
            position.PieceBitboards[Piece.Bishop | position.ColourToMove];
        foreach (int startSquare in bishops.GetActiveBits())
        {
            // Calculate attacks for a bishop on that square given other pieces on the board 
            Bitboard bishopAttacks = 
                AttackBitboards.GenerateBishopAttacks(
                    startSquare, position.OccupiedBitboard);

            // AND friendly pieces to get blocked attacks
            Bitboard bishopBlockedAttacks = bishopAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            // XOR blocked attacks to give only unblocked attacks and captures
            bishopAttacks.ExclusiveCombine(bishopBlockedAttacks);

            bishopAttacks.And(moveMask);

            moves.AddRange(CreateMoves(position, startSquare, bishopAttacks));
        }

        return moves;
    }

    private static List<Move> RookMoves(Board position, Bitboard moveMask) 
    {
        List<Move> moves = [];

        Bitboard rooks = 
            position.PieceBitboards[Piece.Rook | position.ColourToMove];
        foreach (int startSquare in rooks.GetActiveBits())
        {
            Bitboard rookAttacks = AttackBitboards
                .GenerateRookAttacks(startSquare, position.OccupiedBitboard);

            Bitboard rookBlockedAttacks = rookAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            rookAttacks.ExclusiveCombine(rookBlockedAttacks);

            rookAttacks.And(moveMask);

            moves.AddRange(CreateMoves(position, startSquare, rookAttacks));
        }

        return moves;
    }

    private static List<Move> QueenMoves(Board position, Bitboard moveMask) 
    {
        List<Move> moves = [];

        Bitboard queens = 
            position.PieceBitboards[Piece.Queen | position.ColourToMove];
        foreach (int startSquare in queens.GetActiveBits())
        {
            Bitboard queenAttacks = AttackBitboards
                .GenerateQueenAttacks(startSquare, position.OccupiedBitboard);

            Bitboard queenBlockedAttacks = queenAttacks.Copy()
                .And(position.PieceBitboards[position.ColourToMove]);

            queenAttacks.ExclusiveCombine(queenBlockedAttacks);

            queenAttacks.And(moveMask);

            moves.AddRange(CreateMoves(position, startSquare, queenAttacks));
        }

        return moves;
    }

    /// <summary>
    /// Calculates if any opponent pieces are on squares that would attack the
    /// king (putting him in check).
    /// </summary>
    /// <param name="position">The current board state</param>
    /// <param name="kingSquare">The square's index the king occupies</param>
    /// <returns>A bitboard representation of the checkers. Returns as an empty 
    /// bitboard if non.</returns>
    private static Bitboard GetCheckers(Board position, int kingSquare)
    {
        Bitboard checkers = new();

        Bitboard opponentPawns =
            position.PieceBitboards[position.OpositionColour | Piece.Pawn].Copy();

        checkers.Combine(
            opponentPawns.And(
                AttackBitboards.PawnAttacks[position.ColourToMove, kingSquare]));


        Bitboard opponentKnights =
            position.PieceBitboards[position.OpositionColour | Piece.Knight].Copy();

        checkers.Combine(
            opponentKnights.And(
                AttackBitboards.KnightAttacks[kingSquare]));


        Bitboard opponentBishops =
            position.PieceBitboards[position.OpositionColour | Piece.Bishop]
                .Copy();

        checkers.Combine(
            opponentBishops.And(
                AttackBitboards.GenerateBishopAttacks(
                    kingSquare, position.OccupiedBitboard)));


        Bitboard opponentRooks =
            position.PieceBitboards[position.OpositionColour | Piece.Rook].Copy();

        checkers.Combine(
            opponentRooks.And(
                AttackBitboards.GenerateRookAttacks(
                    kingSquare, position.OccupiedBitboard)));


        Bitboard opponentQueens =
            position.PieceBitboards[position.OpositionColour | Piece.Queen].Copy();

        checkers.Combine(
            opponentQueens.And(
                AttackBitboards.GenerateQueenAttacks(
                    kingSquare, position.OccupiedBitboard)));

        return checkers;
    }

    /// <summary>
    /// Creates a list of individual moves from each of the moves within the 
    /// provided bitboard.
    /// </summary>
    /// <param name="position">The current board state</param>
    /// <param name="startSquare">The square the piece is moving from</param>
    /// <param name="bitboard">The bitboard containing the moves</param>
    /// <returns>List of indivual moves</returns>
    private static List<Move> CreateMoves(
        Board position, 
        int startSquare, 
        Bitboard bitboard)
    {
        List<Move> moves = [];

        if (bitboard.IsEmpty())
            return moves;

        foreach (int targetSquare in bitboard.GetActiveBits())
        {
            bool isCapture = 
                position.OccupiedBitboard.GetBit(targetSquare) != 0;

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

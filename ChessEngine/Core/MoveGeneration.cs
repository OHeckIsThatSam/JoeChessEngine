using ChessEngine.Core.Utilities;

namespace ChessEngine.Core;

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

        int kingSquare = BitboardUtil.GetLeastSignificantBit(
            position.PieceBitboards[position.ColourToMove | Piece.King]);

        /* Generate opposition attacks without friendly king to ensure moves that
         * leave the king in check (moving along a slider piece's attack) are not
         * created.
         */
        ulong oppositionAttacks = AttackBitboards.GetAllAttacks(
            position.OpositionColour,
            BitboardUtil.RemoveBit(position.OccupiedBitboard, kingSquare),
            position.PieceBitboards);
      
        moves.AddRange(KingMoves(position, kingSquare, oppositionAttacks));

        ulong checkers = GetCheckers(position, kingSquare);
        int checkerCount = BitboardUtil.Count(checkers);
        position.IsCheck = checkerCount > 0; 

        // Double check, only king moves are possible to get out of check
        if (checkerCount > 1)
            return moves;

        /* Calculate a move mask based on if/what piece is checking the king.
         * The mask is used to limit the moves of the other pieces.
         * If the piece is a slider then moves that block and capture the 
         * attacker are allowed. Else only capturing the attack piece will stop
         * the check. By default all squares are allowed (all on bitboard).
         */
        ulong moveMask = ulong.MaxValue;
        if (checkerCount == 1)
        {
            moveMask = checkers;

            int checkerSquare = BitboardUtil.GetLeastSignificantBit(checkers);
            if (Piece.IsSlider(position.BoardSquares[checkerSquare]))
            {
                moveMask |= AttackBitboards
                    .GetDiagonalAttackRay(checkerSquare, kingSquare);
                moveMask |= AttackBitboards
                    .GetOrthagonalAttackRay(checkerSquare, kingSquare);
            }
        }

        // Calculate absolute pins
        // Calculate a mask of squares that would pin pieces to the king.
        // Create an array of empty bitboards then populate with opponent's sliding
        // pieces.
        Dictionary<int, ulong> pinMasks = [];

        // Diagonal pins (bishops, queens)
        ulong opponentPieces = position.
            PieceBitboards[position.OpositionColour | Piece.Bishop];

        opponentPieces |= position.
            PieceBitboards[position.OpositionColour | Piece.Queen];

        int[] activeBits = BitboardUtil.GetActiveBits(opponentPieces);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int square = activeBits[i];

            CalculatePinMasks(
                position,
                pinMasks,
                AttackBitboards.GetDiagonalAttackRay(square, kingSquare),
                square);
        }

        // Orthagonal pins (rooks, queens)
        opponentPieces = position.
            PieceBitboards[position.OpositionColour | Piece.Rook];

        opponentPieces |= position.
            PieceBitboards[position.OpositionColour | Piece.Queen];
        
        activeBits = BitboardUtil.GetActiveBits(opponentPieces);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int square = activeBits[i];

            CalculatePinMasks(
                position,
                pinMasks,
                AttackBitboards.GetOrthagonalAttackRay(square, kingSquare),
                square);
        }

        moves.AddRange(CastlingMoves(position, oppositionAttacks));

        moves.AddRange(PawnMoves(position, moveMask, pinMasks));

        moves.AddRange(KnightMoves(position, moveMask, pinMasks));

        moves.AddRange(BishopMoves(position, moveMask, pinMasks));

        moves.AddRange(RookMoves(position, moveMask, pinMasks));

        moves.AddRange(QueenMoves(position, moveMask, pinMasks));

        return moves;
    }

    private static List<Move> KingMoves(
        Board position, 
        int kingSquare, 
        ulong oppositionAttacks)
    {
        ulong kingAttacks = AttackBitboards.KingAttacks[kingSquare];

        // Remove king attacks on squares attacked by the oposition
        ulong illegalKingAttacks = kingAttacks & oppositionAttacks;

        // Remove king attacks blocked by friendly pieces
        ulong blockedKingAttacks = kingAttacks & 
            position.PieceBitboards[position.ColourToMove];

        kingAttacks ^= illegalKingAttacks | blockedKingAttacks;

        return CreateMoves(position, kingSquare, kingAttacks);
    }

    private static List<Move> CastlingMoves(
        Board position, 
        ulong oppositionAttacks)
    {
        List<Move> moves = [];

        if (position.IsCheck)
            return moves;

        ulong castleAttacked;
        ulong castleBlocked;
        if (position.ColourToMove == Piece.White)
        {
            castleAttacked = oppositionAttacks &
                BitboardUtil.WhiteKingSideCastleMask;

            castleBlocked = position.OccupiedBitboard &
                BitboardUtil.WhiteKingSideCastleMask;

            if (position.CanWhiteKingSideCastle && castleAttacked.Equals(0) && 
                castleBlocked.Equals(0))
            {
                moves.Add(new Move()
                {
                    StartSquare = (int)BitboardUtil.Squares.e1,
                    TargetSquare = (int)BitboardUtil.Squares.g1,
                    IsCastling = true,
                    RookStartSquare = (int)BitboardUtil.Squares.h1,
                    RookTargetSquare = (int)BitboardUtil.Squares.f1,
                    IsCheck = IsAttackCheck(position,
                        AttackBitboards.GenerateRookAttacks(
                            (int)BitboardUtil.Squares.f1, position.OccupiedBitboard)),
                });
            }

            castleAttacked = oppositionAttacks &
                BitboardUtil.WhiteQueenSideCastleMask;

            castleBlocked = position.OccupiedBitboard &
                BitboardUtil.WhiteQueenSideCastleBlockMask;

            if (position.CanWhiteQueenSideCastle && castleAttacked.Equals(0) && 
                castleBlocked.Equals(0))
            {
                moves.Add(new Move()
                {
                    StartSquare = (int)BitboardUtil.Squares.e1,
                    TargetSquare = (int)BitboardUtil.Squares.c1,
                    IsCastling = true,
                    RookStartSquare = (int)BitboardUtil.Squares.a1,
                    RookTargetSquare = (int)BitboardUtil.Squares.d1,
                    IsCheck = IsAttackCheck(position, 
                        AttackBitboards.GenerateRookAttacks(
                            (int)BitboardUtil.Squares.d1, position.OccupiedBitboard)),
                });
            }
        }
        else
        {
            castleAttacked = oppositionAttacks &
                BitboardUtil.BlackKingSideCastleMask;

            castleBlocked = position.OccupiedBitboard &
                BitboardUtil.BlackKingSideCastleMask;

            if (position.CanBlackKingSideCastle && castleAttacked.Equals(0) &&
                castleBlocked.Equals(0))
            {
                moves.Add(new Move()
                {
                    StartSquare = (int)BitboardUtil.Squares.e8,
                    TargetSquare = (int)BitboardUtil.Squares.g8,
                    IsCastling = true,
                    RookStartSquare = (int)BitboardUtil.Squares.h8,
                    RookTargetSquare = (int)BitboardUtil.Squares.f8,
                    IsCheck = IsAttackCheck(position,
                        AttackBitboards.GenerateRookAttacks(
                            (int)BitboardUtil.Squares.f8, position.OccupiedBitboard)),
                });
            }

            castleAttacked = oppositionAttacks &
                BitboardUtil.BlackQueenSideCastleMask;

            castleBlocked = position.OccupiedBitboard &
                BitboardUtil.BlackQueenSideCastleBlockMask;

            if (position.CanBlackQueenSideCastle && castleAttacked.Equals(0) &&
                castleBlocked.Equals(0))
            {
                moves.Add(new Move()
                {
                    StartSquare = (int)BitboardUtil.Squares.e8,
                    TargetSquare = (int)BitboardUtil.Squares.c8,
                    IsCastling = true,
                    RookStartSquare = (int)BitboardUtil.Squares.a8,
                    RookTargetSquare = (int)BitboardUtil.Squares.d8,
                    IsCheck = IsAttackCheck(position,
                        AttackBitboards.GenerateRookAttacks(
                            (int)BitboardUtil.Squares.d8, position.OccupiedBitboard)),
                });
            }
        }

        return moves;
    }

    private static List<Move> PawnMoves(
        Board position, 
        ulong moveMask,
        Dictionary<int, ulong> pinMasks)
    {
        List<Move> moves = [];

        ulong pawns = position.PieceBitboards[Piece.Pawn | position.ColourToMove];
        int[] activeBits = BitboardUtil.GetActiveBits(pawns);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int startSquare = activeBits[i];

            ulong combinedMoveMask;
            // If pinned piece alter move mask with pin
            if (pinMasks.TryGetValue(startSquare, out ulong pinMask))
                combinedMoveMask = pinMask & moveMask;
            else
                combinedMoveMask = moveMask;

            ulong pawnBitboard = BitboardUtil.AddBit(0, startSquare);

            ulong pawnMoves;
            ulong promotionSquares;
            ulong pawnAttacks = AttackBitboards
                .PawnAttacks[position.ColourToMove, startSquare];

            ulong oppositionPieces = position.PieceBitboards[position.OpositionColour];

            if (position.ColourToMove == Piece.White)
                promotionSquares = BitboardUtil.Rank8Mask;
            else
                promotionSquares = BitboardUtil.Rank1Mask;

            // Calculate and create En Passant moves seperately
            if (position.hasEnPassantTargetSquare)
            {
                // Calculate the square of the opponent pawn thats takeable
                int opponentPawnSquare = position.enPassantTargetSquare +
                    (position.ColourToMove == Piece.White ? 8 : -8);

                // Add square to oposition bitboard to make it attackable
                oppositionPieces = BitboardUtil.AddBit(
                    oppositionPieces, position.enPassantTargetSquare);

                /* Create seperate En Passant move mask that can contain the
                 * En Passant square if the oppenents pawn is checking the king
                 */
                ulong enPassantMoveMask = combinedMoveMask;

                if (BitboardUtil.GetBit(enPassantMoveMask, opponentPawnSquare) != 0)
                    enPassantMoveMask = BitboardUtil.AddBit(
                        enPassantMoveMask, position.enPassantTargetSquare);

                pawnAttacks &= oppositionPieces & enPassantMoveMask;
                
                int[] activeAttackBits = BitboardUtil.GetActiveBits(pawnAttacks);
                for (int j = 0; j < activeAttackBits.Length; j++)
                {
                    int targetSquare = activeAttackBits[j];
                    Move move = new()
                    {
                        StartSquare = startSquare,
                        TargetSquare = targetSquare,
                        IsCapture = true,
                        CapturedPiece = position.BoardSquares[targetSquare]
                    };
                        
                    if (position.enPassantTargetSquare == targetSquare)
                    {
                        move.IsEnPassant = true;
                        move.TargetPawnSquare = opponentPawnSquare;
                        move.CapturedPiece = position.BoardSquares[opponentPawnSquare];
                    }

                    moves.Add(move);
                }
            }
            else
            {
                pawnAttacks &= oppositionPieces & combinedMoveMask;

                if ((pawnAttacks & promotionSquares) == 0)
                    moves.AddRange(
                        CreateMoves(position, startSquare, pawnAttacks));
                else
                    moves.AddRange(
                        CreatePromotionMoves(position, startSquare, pawnAttacks));
            }

            if (position.ColourToMove == Piece.White)
            {
                // Populate move Bitboard with forward move if empty square
                pawnMoves = pawnBitboard >> BitboardUtil.PawnForward;
                pawnMoves &= position.EmptyBitboard;

                // Check if double move is valid (on second rank and not blocked)
                if ((pawnBitboard & BitboardUtil.Rank2Mask) != 0 &&
                    pawnMoves != 0)
                {
                    pawnMoves |= pawnMoves >> BitboardUtil.PawnForward;

                    pawnMoves &= position.EmptyBitboard;
                }
            }
            else
            {
                pawnMoves = pawnBitboard << BitboardUtil.PawnForward;
                pawnMoves &= position.EmptyBitboard;

                if ((pawnBitboard & BitboardUtil.Rank7Mask) != 0 &&
                    pawnMoves != 0)
                {
                    pawnMoves |= pawnMoves << BitboardUtil.PawnForward;

                    pawnMoves &= position.EmptyBitboard;
                }
            }

            pawnMoves &= combinedMoveMask;

            // Add to move HasEnPassant if double move
            if ((pawnMoves & promotionSquares) == 0)
                moves.AddRange(CreateMoves(position, startSquare, pawnMoves));
            else
                moves.AddRange(CreatePromotionMoves(position, startSquare, pawnMoves));
        }

        return moves;
    }

    private static List<Move> KnightMoves(
        Board position, 
        ulong moveMask,
        Dictionary<int, ulong> pinMasks)
    {
        List<Move> moves = [];

        ulong knights = 
            position.PieceBitboards[Piece.Knight | position.ColourToMove];

        int[] activeBits = BitboardUtil.GetActiveBits(knights);
        for (int i = 0; i < activeBits.Length; i++)
        { 
            int startSquare = activeBits[i];

            // L shape move pattern means knight can never move along or capture
            // when pinned
            if (pinMasks.ContainsKey(startSquare))
                continue;

            // Get attacks for a knight on that square
            ulong attacks = AttackBitboards.KnightAttacks[startSquare];

            // AND the friendly pieces to the bitboard to give blocked attacks
            ulong blocked = attacks & position.PieceBitboards[position.ColourToMove];

            // XOR the blocked attacks with normal attacks giving unblocked attacks
            // or captures
            attacks ^= blocked;
            attacks &= moveMask;

            moves.AddRange(CreateMoves(position, startSquare, attacks));
        }

        return moves;
    }

    private static List<Move> BishopMoves(
        Board position, 
        ulong moveMask,
        Dictionary<int, ulong> pinMasks)
    {
        List<Move> moves = [];

        ulong bishops = 
            position.PieceBitboards[Piece.Bishop | position.ColourToMove];

        int[] activeBits = BitboardUtil.GetActiveBits(bishops);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int startSquare = activeBits[i];

            ulong combinedMoveMask;
            // If pinned piece alter move mask with pin
            if (pinMasks.TryGetValue(startSquare, out ulong pinMask))
                combinedMoveMask = pinMask & moveMask;
            else
                combinedMoveMask = moveMask;

            // Calculate attacks for a bishop on that square given other pieces on the board 
            ulong attacks = 
                AttackBitboards.GenerateBishopAttacks(
                    startSquare, position.OccupiedBitboard);

            // AND friendly pieces to get blocked attacks
            ulong blocked = attacks & position.PieceBitboards[position.ColourToMove];

            // XOR blocked attacks to give only unblocked attacks and captures
            attacks ^= blocked;
            attacks &= combinedMoveMask;

            moves.AddRange(CreateMoves(position, startSquare, attacks));
        }

        return moves;
    }

    private static List<Move> RookMoves(
        Board position, 
        ulong moveMask, 
        Dictionary<int, ulong> pinMasks) 
    {
        List<Move> moves = [];

        ulong rooks = 
            position.PieceBitboards[Piece.Rook | position.ColourToMove];

        int[] activeBits = BitboardUtil.GetActiveBits(rooks);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int startSquare = activeBits[i];

            ulong combinedMoveMask;
            // If pinned piece alter move mask with pin
            if (pinMasks.TryGetValue(startSquare, out ulong pinMask))
                combinedMoveMask = pinMask & moveMask;
            else
                combinedMoveMask = moveMask;

            ulong attacks = AttackBitboards
                .GenerateRookAttacks(startSquare, position.OccupiedBitboard);

            ulong blocked = attacks & position.PieceBitboards[position.ColourToMove];

            attacks ^= blocked;
            attacks &= combinedMoveMask;

            moves.AddRange(CreateMoves(position, startSquare, attacks));
        }

        return moves;
    }

    private static List<Move> QueenMoves(
        Board position, 
        ulong moveMask,
        Dictionary<int, ulong> pinMasks) 
    {
        List<Move> moves = [];

        ulong queens = 
            position.PieceBitboards[Piece.Queen | position.ColourToMove];

        int[] activeBits = BitboardUtil.GetActiveBits(queens);
        for (int i = 0; i < activeBits.Length; i++)
        {
            int startSquare = activeBits[i];

            ulong combinedMoveMask;
            // If pinned piece alter move mask with pin
            if (pinMasks.TryGetValue(startSquare, out ulong pinMask))
                combinedMoveMask = pinMask & moveMask;
            else
                combinedMoveMask = moveMask;

            ulong attacks = AttackBitboards
                .GenerateQueenAttacks(startSquare, position.OccupiedBitboard);
            
            ulong blocked = attacks & position.PieceBitboards[position.ColourToMove];

            attacks ^= blocked;
            attacks &= combinedMoveMask;

            moves.AddRange(CreateMoves(position, startSquare, attacks));
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
    private static ulong GetCheckers(Board position, int kingSquare)
    {
        ulong checkers = 0;

        checkers |= position.PieceBitboards[position.OpositionColour | Piece.Pawn] & 
            AttackBitboards.PawnAttacks[position.ColourToMove, kingSquare];

        checkers |= position.PieceBitboards[position.OpositionColour | Piece.Knight] &
            AttackBitboards.KnightAttacks[kingSquare];

        checkers |= position.PieceBitboards[position.OpositionColour | Piece.Bishop] &
            AttackBitboards.GenerateBishopAttacks(kingSquare, position.OccupiedBitboard);

        checkers |= position.PieceBitboards[position.OpositionColour | Piece.Rook] &
            AttackBitboards.GenerateRookAttacks(kingSquare, position.OccupiedBitboard);

        checkers |= position.PieceBitboards[position.OpositionColour | Piece.Queen] &
            AttackBitboards.GenerateQueenAttacks(kingSquare, position.OccupiedBitboard);

        return checkers;
    }

    private static void CalculatePinMasks(
        Board position,
        Dictionary<int, ulong> pinMasks,
        ulong attackRay,
        int square)
    {
        ulong pinnedPieces = attackRay & position.PieceBitboards[position.ColourToMove];

        // Ignore rays blocked by opponent pieces
        if ((attackRay & position.PieceBitboards[position.OpositionColour]) != 0)
            return;

        if (attackRay == 0 || BitboardUtil.Count(pinnedPieces) != 1)
            return;

        int pinnedSquare = BitboardUtil.GetLeastSignificantBit(pinnedPieces);

        /* Remove square that pinned piece occupies and add square of the
         * pin attacker to give all moves along pin
         */
        ulong pinnedMoveMask = BitboardUtil.RemoveBit(attackRay, pinnedSquare);
        pinnedMoveMask = BitboardUtil.AddBit(pinnedMoveMask, square);

        pinMasks.Add(pinnedSquare, pinnedMoveMask);
    }

    private static bool IsAttackCheck(Board postition, ulong attack)
    {
        ulong opponentKing = postition.PieceBitboards[
            Piece.King | postition.OpositionColour];

        return (attack & opponentKing) != 0;
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
        ulong bitboard)
    {
        List<Move> moves = [];

        int[] attackSqaures = BitboardUtil.GetActiveBits(bitboard);
        for (int i = 0; i < attackSqaures.Length; i++)
        {
            int targetSquare = attackSqaures[i];

            bool isCapture =
                BitboardUtil.GetBit(position.OccupiedBitboard, targetSquare) != 0;

            moves.Add(new()
            {
                StartSquare = startSquare,
                TargetSquare = targetSquare,
                IsCapture = isCapture,
                CapturedPiece = position.BoardSquares[targetSquare]
            });
        }

        return moves;
    }

    private static List<Move> CreatePromotionMoves(
        Board position,
        int startSquare,
        ulong bitboard)
    {
        List<Move> moves = [];

        int[] attackSqaures = BitboardUtil.GetActiveBits(bitboard);
        for (int i = 0; i < attackSqaures.Length; i++)
        {
            int targetSquare = attackSqaures[i];

            bool isCapture =
                BitboardUtil.GetBit(position.OccupiedBitboard, targetSquare) != 0;

            for (int j = 0; j < Piece.PromotionTypes.Length; j++)
            {
                moves.Add(new()
                {
                    StartSquare = startSquare,
                    TargetSquare = targetSquare,
                    IsCapture = isCapture,
                    CapturedPiece = position.BoardSquares[targetSquare],
                    IsPromotion = true,
                    PromotionType = Piece.PromotionTypes[j]
                });
            }
        }

        return moves;
    }
}

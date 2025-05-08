using ChessEngine.Core.Utilities;

namespace ChessEngine.Core;

/// <summary>
/// A static class that provides attack bitboards for pieces on any given 
/// sqaure of the board. Pawn, Knight and King attacks are precalclated as 
/// their attack's are not affected by other pieces positions. Bishop, Rook and  
/// Queen are calculated on the fly with respect to all other pieces on the  
/// board (blockers).
/// 
/// Note: the attacks do not include any legality check, they are simply 
/// bitboards to be used elsewhere by the engine. Legality checking will be done 
/// during move search by a legal moves generator.
/// 
/// TODO: Calculating attack bitboards on the fly is inefficent. An improved 
/// method is using Magic Bitboards to pre calculate sliding piece moves. THIS IS
/// CALLED MAGIC FOR A REASON, I cannot understand how these work or how to  
/// implement them at the moment.
/// </summary>
public static class AttackBitboards
{
    /* Pawn attacks is 2D for colour and position e.g. [side][sqaure].
     * The array is length nine so that the colour value defined in the Piece class
     * can be used as the index.
     */
    public static readonly ulong[,] PawnAttacks = new ulong[9, 64];

    public static readonly ulong[] KnightAttacks = new ulong[64];

    public static readonly ulong[] KingAttacks = new ulong[64];

    static AttackBitboards()
    {
        for (int square = 0; square < 64; square++)
        {
            PawnAttacks[0, square] = GeneratePawnAttacks(0, square);
            PawnAttacks[8, square] = GeneratePawnAttacks(8, square);

            KnightAttacks[square] = GenerateKnightAttacks(square);

            KingAttacks[square] = GenerateKingAttacks(square);
        }
    }

    private static ulong GeneratePawnAttacks(int side, int square)
    {
        ulong pawnBitboard = BitboardUtil.AddBit(0, square);
        ulong pawnAttacks = 0;

        // Change direction of attacks based on piece colour
        if (side == Piece.White)
        {
            // Restrict attacks on the edge of board (a and h file)
            if ((pawnBitboard & BitboardUtil.NotHFileMask) != 0)
                pawnAttacks |= pawnBitboard >> 7;

            if ((pawnBitboard & BitboardUtil.NotAFileMask) != 0)
                pawnAttacks |= pawnBitboard >> 9;
        }
        else
        {
            if ((pawnBitboard & BitboardUtil.NotAFileMask) != 0)
                pawnAttacks |= pawnBitboard << 7;

            if ((pawnBitboard & BitboardUtil.NotHFileMask) != 0)
                pawnAttacks |= pawnBitboard << 9;
        }

        return pawnAttacks;
    }

    private static ulong GenerateKnightAttacks(int square)
    {
        ulong knightBitboard = BitboardUtil.AddBit(0, square);
        ulong knightAttacks = 0;

        // Restict attacks on the edge and to stop attacks overflowing
        if ((knightBitboard & BitboardUtil.NotAFileMask) != 0)
        {
            knightAttacks |= knightBitboard >> 17;
            knightAttacks |= knightBitboard << 15;
        }

        if ((knightBitboard & BitboardUtil.NotABFileMask) != 0)
        {
            knightAttacks |= knightBitboard >> 10;
            knightAttacks |= knightBitboard << 6;
        }

        if ((knightBitboard & BitboardUtil.NotGHFileMask) != 0)
        {
            knightAttacks |= knightBitboard << 10;
            knightAttacks |= knightBitboard >> 6;
        }

        if ((knightBitboard & BitboardUtil.NotHFileMask) != 0)
        {
            knightAttacks |= knightBitboard >> 15;
            knightAttacks |= knightBitboard << 17;
        }

        return knightAttacks;
    }

    private static ulong GenerateKingAttacks(int square)
    {
        ulong kingBitboard = BitboardUtil.AddBit(0, square);
        ulong kingAttacks = 0;

        if ((kingBitboard & BitboardUtil.NotHFileMask) != 0)
        {
            kingAttacks |= kingBitboard << 1;
            kingAttacks |= kingBitboard << 9;
            kingAttacks |= kingBitboard >> 7;
        }

        if ((kingBitboard & BitboardUtil.NotAFileMask) != 0)
        {
            kingAttacks |= kingBitboard >> 1;
            kingAttacks |= kingBitboard >> 9;
            kingAttacks |= kingBitboard << 7;
        }

        kingAttacks |= kingBitboard << 8;
        kingAttacks |= kingBitboard >> 8;

        return kingAttacks;
    }

    public static ulong GenerateBishopAttacks(int square, ulong blockers)
    {
        ulong bishopAttacks = 0;

        int rank, file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        // Mask squares the bishop can move to from it's position checking for
        // pieces that would block an attack.
        for (rank = targetRank + 1, file = targetFile + 1;
             rank <= 7 && file <= 7;
             rank++, file++)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks = BitboardUtil.AddBit(bishopAttacks, squareIndex);

            // Break loop if blocker is on this square as bishop can't attack
            // anything after it.
            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile + 1;
             rank >= 0 && file <= 7;
             rank--, file++)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks = BitboardUtil.AddBit(bishopAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank + 1, file = targetFile - 1;
             rank <= 7 && file >= 0;
             rank++, file--)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks = BitboardUtil.AddBit(bishopAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile - 1;
             rank >= 0 && file >= 0;
             rank--, file--)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks = BitboardUtil.AddBit(bishopAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        return bishopAttacks;
    }

    public static ulong GenerateRookAttacks(int square, ulong blockers)
    {
        ulong rookAttacks = 0;

        int rank, file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        // Mask rook attacks with checking for pieces stoping the attack
        for (rank = targetRank, file = targetFile + 1;
             file <= 7;
             file++)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks = BitboardUtil.AddBit(rookAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank, file = targetFile - 1;
             file >= 0;
             file--)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks = BitboardUtil.AddBit(rookAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank + 1, file = targetFile;
             rank <= 7;
             rank++)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks = BitboardUtil.AddBit(rookAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile;
             rank >= 0;
             rank--)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks = BitboardUtil.AddBit(rookAttacks, squareIndex);

            if (BitboardUtil.GetBit(blockers, squareIndex) != 0)
                break;
        }

        return rookAttacks;
    }

    public static ulong GenerateQueenAttacks(int square, ulong blockers)
    {
        ulong queenAttacks = GenerateBishopAttacks(square, blockers);
        queenAttacks |= GenerateRookAttacks(square, blockers);

        return queenAttacks;
    }

    public static ulong GetAllAttacks(
        int colour,
        ulong blockers,
        ulong[] pieceBitboards)
    {
        ulong allAttacks = 0;

        ulong king = pieceBitboards[colour + Piece.King];
        if (king != 0)
        {
            int kingSquare = BitboardUtil.GetLeastSignificantBit(king);
            allAttacks |= KingAttacks[kingSquare];
        }

        ulong pawns = pieceBitboards[colour + Piece.Pawn];
        int[] activeBits = BitboardUtil.GetActiveBits(pawns);
        for (int i = 0; i < activeBits.Length; i++)
        {
            allAttacks |= PawnAttacks[colour, activeBits[i]];
        }

        ulong knights = pieceBitboards[colour + Piece.Knight];
        activeBits = BitboardUtil.GetActiveBits(knights);
        for (int i = 0; i < activeBits.Length; i++)
        {
            allAttacks |= KnightAttacks[activeBits[i]];
        }

        ulong bishops = pieceBitboards[colour + Piece.Bishop];
        activeBits = BitboardUtil.GetActiveBits(bishops);
        for (int i = 0; i < activeBits.Length; i++)
        {
            allAttacks |= GenerateBishopAttacks(activeBits[i], blockers);
        }

        ulong rooks = pieceBitboards[colour + Piece.Rook];
        activeBits = BitboardUtil.GetActiveBits(rooks);
        for (int i = 0; i < activeBits.Length; i++)
        {
            allAttacks |= GenerateRookAttacks(activeBits[i], blockers);
        }

        ulong queens = pieceBitboards[colour + Piece.Queen];
        activeBits = BitboardUtil.GetActiveBits(queens);
        for (int i = 0; i < activeBits.Length; i++)
        {
            allAttacks |= GenerateQueenAttacks(activeBits[i], blockers);
        }

        return allAttacks;
    }

    public static ulong GetDiagonalAttackRay(int startSquare, int targetSquare)
    {
        ulong attackRay = 0;

        int rank, file;

        int targetRank = startSquare / 8;
        int targetFile = startSquare % 8;

        for (rank = targetRank + 1, file = targetFile + 1;
             rank <= 7 && file <= 7;
             rank++, file++)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank - 1, file = targetFile + 1;
             rank >= 0 && file <= 7;
             rank--, file++)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank + 1, file = targetFile - 1;
             rank <= 7 && file >= 0;
             rank++, file--)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank - 1, file = targetFile - 1;
             rank >= 0 && file >= 0;
             rank--, file--)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        return 0;
    }

    // TODO: Use masks of ranks and files to determine if start and target square
    // are orthagonal to eachother.
    public static ulong GetOrthagonalAttackRay(int startSquare, int targetSquare)
    {
        ulong attackRay = 0;

        int rank, file;

        int targetRank = startSquare / 8;
        int targetFile = startSquare % 8;

        // Check each direction until target square is reached
        for (rank = targetRank, file = targetFile + 1;
             file <= 7;
             file++)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank, file = targetFile - 1;
             file >= 0;
             file--)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank + 1, file = targetFile;
             rank <= 7;
             rank++)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        attackRay = 0;
        for (rank = targetRank - 1, file = targetFile;
             rank >= 0;
             rank--)
        {
            int squareIndex = rank * 8 + file;

            if (squareIndex == targetSquare)
                return attackRay;

            attackRay = BitboardUtil.AddBit(attackRay, squareIndex);
        }

        // No attack ray found
        return 0;
    }
}

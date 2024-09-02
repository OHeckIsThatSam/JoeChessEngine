using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core.Bitboards;

/// <summary>
/// A static class that provides attack bitboards for pieces in any given 
/// position on the board. Pawn, Knight and King attacks are precalclated as 
/// their attack's are not affected by other pieces positions. Bishop, Rook and  
/// Queen are calculated on the fly with respect to all other pieces on the  
/// board (blockers).
/// 
/// Note: the attacks do not include any legality check, they are simply 
/// bitboards to be used elsewhere by the engine. Legality checking will be done 
/// during move searching as a legal moves generator.
/// 
/// TODO: Calculating attack bitboards on the fly is inefficent. An improved 
/// method is using Magic Bitboards to pre calculate sliding piece moves. THIS I
/// CALLED MAGIC FOR A REASON, I cannot understand how these work or how to  
/// implement them at the moment.
/// </summary>
public static class AttackBitboards
{
    // Pawn attacks is 2D for colour and position e.g. [side][sqaure]
    public static readonly Bitboard[,] PawnAttacks = new Bitboard[2, 64];

    public static readonly Bitboard[] KnightAttacks = new Bitboard[64];

    public static readonly Bitboard[] KingAttacks = new Bitboard[64];

    static AttackBitboards()
    {
        for (int square = 0; square < 64; square++)
        {
            PawnAttacks[0, square] = GeneratePawnAttacks(0, square);
            PawnAttacks[1, square] = GeneratePawnAttacks(1, square);

            KnightAttacks[square] = GenerateKnightAttacks(square);

            KingAttacks[square] = GenerateKingAttacks(square);
        }
    }

    private static Bitboard GeneratePawnAttacks(int side, int square)
    {
        Bitboard pawnBitboard = new();
        pawnBitboard.AddBit(square);

        Bitboard pawnAttacks = new();

        // Change direction of attacks based on piece colour
        if (side == Piece.White)
        {
            // Restrict attacks on the edge of board (a and h file)
            if (pawnBitboard.Mask(BitboardUtilities.NotHFileMask) != 0)
                pawnAttacks.Combine(pawnBitboard.ShiftRight(7));

            if (pawnBitboard.Mask(BitboardUtilities.NotAFileMask) != 0)
                pawnAttacks.Combine(pawnBitboard.ShiftRight(9));
        }
        else
        {
            if (pawnBitboard.Mask(BitboardUtilities.NotAFileMask) != 0)
                pawnAttacks.Combine(pawnBitboard.ShiftLeft(7));

            if (pawnBitboard.Mask(BitboardUtilities.NotHFileMask) != 0)
                pawnAttacks.Combine(pawnBitboard.ShiftLeft(9));
        }

        return pawnAttacks;
    }

    private static Bitboard GenerateKnightAttacks(int square)
    {
        Bitboard knightBitboard = new();
        knightBitboard.AddBit(square);

        Bitboard knightAttacks = new();

        // Restict attacks on the edge and to stop attacks overflowing
        if (knightBitboard.Mask(BitboardUtilities.NotAFileMask) != 0)
        {
            knightAttacks.Combine(knightBitboard.ShiftRight(17));
            knightAttacks.Combine(knightBitboard.ShiftLeft(15));
        }

        if (knightBitboard.Mask(BitboardUtilities.NotABFileMask) != 0)
        {
            knightAttacks.Combine(knightBitboard.ShiftRight(10));
            knightAttacks.Combine(knightBitboard.ShiftLeft(6));
        }

        if (knightBitboard.Mask(BitboardUtilities.NotGHFileMask) != 0)
        {
            knightAttacks.Combine(knightBitboard.ShiftLeft(10));
            knightAttacks.Combine(knightBitboard.ShiftRight(6));
        }

        if (knightBitboard.Mask(BitboardUtilities.NotHFileMask) != 0)
        {
            knightAttacks.Combine(knightBitboard.ShiftRight(15));
            knightAttacks.Combine(knightBitboard.ShiftLeft(17));
        }

        return knightAttacks;
    }

    private static Bitboard GenerateKingAttacks(int square)
    {
        Bitboard kingBitboard = new();
        kingBitboard.AddBit(square);

        Bitboard kingAttacks = new();

        if (kingBitboard.Mask(BitboardUtilities.NotHFileMask) != 0)
        {
            kingAttacks.Combine(kingBitboard.ShiftLeft(1));
            kingAttacks.Combine(kingBitboard.ShiftLeft(9));
            kingAttacks.Combine(kingBitboard.ShiftRight(7));
        }

        if (kingBitboard.Mask(BitboardUtilities.NotAFileMask) != 0)
        {
            kingAttacks.Combine(kingBitboard.ShiftRight(1));
            kingAttacks.Combine(kingBitboard.ShiftRight(9));
            kingAttacks.Combine(kingBitboard.ShiftLeft(7));
        }

        kingAttacks.Combine(kingBitboard.ShiftLeft(8));
        kingAttacks.Combine(kingBitboard.ShiftRight(8));

        return kingAttacks;
    }

    public static Bitboard GenerateBishopAttacks(int square, Bitboard blockers)
    {
        Bitboard bishopAttacks = new();

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

            bishopAttacks.AddBit(squareIndex);

            // Break loop if blocker is on this square as bishop can't attack
            // anything after it.
            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile + 1;
             rank >= 0 && file <= 7;
             rank--, file++)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank + 1, file = targetFile - 1;
             rank <= 7 && file >= 0;
             rank++, file--)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile - 1;
             rank >= 0 && file >= 0;
             rank--, file--)
        {
            int squareIndex = rank * 8 + file;

            bishopAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        return bishopAttacks;
    }

    public static Bitboard GenerateRookAttacks(int square, Bitboard blockers)
    {
        Bitboard rookAttacks = new();

        int rank, file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        // Mask rook attacks with checking for pieces stoping the attack
        for (rank = targetRank, file = targetFile + 1;
             file <= 7;
             file++)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank, file = targetFile - 1;
             file >= 0;
             file--)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank + 1, file = targetFile;
             rank <= 7;
             rank++)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        for (rank = targetRank - 1, file = targetFile;
             rank >= 0;
             rank--)
        {
            int squareIndex = rank * 8 + file;

            rookAttacks.AddBit(squareIndex);

            if (blockers.GetBit(squareIndex) != 0)
                break;
        }

        return rookAttacks;
    }

    public static Bitboard GenerateQueenAttacks(int square, Bitboard blockers)
    {
        Bitboard queenAttacks = new();

        queenAttacks.Combine(GenerateBishopAttacks(square, blockers));
        queenAttacks.Combine(GenerateRookAttacks(square, blockers));

        return queenAttacks;
    }
}

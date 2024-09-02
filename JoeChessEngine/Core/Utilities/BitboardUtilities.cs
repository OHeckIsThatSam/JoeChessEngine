namespace Chess_Bot.Core.Utilities;

public static class BitboardUtilities
{
    public enum Sqaures
    {
        a8, b8, c8, d8, e8, f8, g8, h8,
        a7, b7, c7, d7, e7, f7, g7, h7,
        a6, b6, c6, d6, e6, f6, g6, h6,
        a5, b5, c5, d5, e5, f5, g5, h5,
        a4, b4, c4, d4, e4, f4, g4, h4,
        a3, b3, c3, d3, e3, f3, g3, h3,
        a2, b2, c2, d2, e2, f2, g2, h2,
        a1, b1, c1, d1, e1, f1, g1, h1
    }

    /*
     * Pre generated masks for bitboards. If we mask a piece Bitboard with a mask
     * and the resulting bitboard is some value other than 0 (not empty). Then we
     * can determine if the piece was within the area of the board that the mask 
     * represents.
     */
    public const ulong NotAFileMask = 18374403900871474942;
    public const ulong NotABFileMask = 18229723555195321596;
    public const ulong NotHFileMask = 9187201950435737471;
    public const ulong NotGHFileMask = 4557430888798830399;

    public const ulong RankMask8 = 255;
    public const ulong RankMask7 = 65280;
    public const ulong RankMask2 = 71776119061217280;
    public const ulong RankMask1 = 18374686479671623680;

    /*
     * Integers for the index change of piece moves/attacks. Values are used with
     * by shifting the bitboard with the pieces position.
     */
    public const int PawnForward = 8;
}

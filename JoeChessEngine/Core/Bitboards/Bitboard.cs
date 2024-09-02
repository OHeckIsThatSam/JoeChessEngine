using System.Numerics;
using System.Text;

namespace Chess_Bot.Core.Bitboards;

public class Bitboard
{
    private ulong bitboard;

    public Bitboard() { }

    public Bitboard(ulong bitboard)
    {
        this.bitboard = bitboard;
    }

    public ulong GetBit(int squareIndex) => bitboard & 1UL << squareIndex;

    public void AddBit(int squareIndex) => bitboard |= 1UL << squareIndex;

    public void RemoveBit(int squareIndex)
    {
        if (GetBit(squareIndex) != 0)
            bitboard ^= 1UL << squareIndex;
    }

    public Bitboard ShiftLeft(int shiftCount) => new(bitboard << shiftCount);

    public Bitboard ShiftRight(int shiftCount) => new(bitboard >> shiftCount);

    /// <summary>
    /// Combines this bitboard with another by OR.
    /// </summary>
    /// <param name="bitboard">The other Bitboard to be combined.</param>
    public Bitboard Combine(Bitboard bitboard)
    {
        this.bitboard |= bitboard.bitboard;
        return this;
    }

    /// <summary>
    /// Combines this bitboard with another by XOR.
    /// </summary>
    /// <param name="bitboard">The other Bitboard to be combined.</param>
    public Bitboard ExclusiveCombine(Bitboard bitboard)
    {
        this.bitboard ^= bitboard.bitboard;
        return this;
    }

    /// <summary>
    /// Combines this bitboard with another by AND.
    /// </summary>
    /// <param name="bitboard">The other Bitboard to be combined.</param>
    public Bitboard Add(Bitboard bitboard)
    {
        this.bitboard &= bitboard.bitboard;
        return this;
    }

    public ulong Invert() => ~bitboard;

    public int Count() => BitOperations.PopCount(bitboard);

    /// <summary>
    /// Returns the index of the smallest active bit.
    /// </summary>
    /// <returns>Index of the smallest active bit.</returns>
    public int GetLeastSignificantBit()
    {
        return BitOperations.TrailingZeroCount(bitboard);
    }

    /// <summary>
    /// Returns an array of all positions with an active bit within the 
    /// Bitboard. 
    /// Loops through by position from smallest to largest.
    /// </summary>
    /// <returns>The indexs of active bits.</returns>
    public int[] GetActiveBits()
    {
        ulong bitboardCopy = bitboard;
        int[] results = new int[Count()];

        for (int i = 0; i < Count(); i++)
        {
            // Get smallest index of active bit
            results[i] = BitOperations.TrailingZeroCount(bitboardCopy);

            // Remove bit from bitboard
            bitboardCopy ^= 1UL << results[i];
        }

        return results;
    }

    /// <summary>
    /// Returns a copy of the current Bitboard.
    /// </summary>
    /// <returns>
    /// The Bitboard copied from the underlying bitboard value.
    /// </returns>
    public Bitboard Copy() => new(bitboard);

    /// <summary>
    /// Returns the result of applying a mask to the Bitboard.
    /// </summary>
    /// <param name="mask">The bit mask.</param>
    /// <returns>The resulting value of the mask.</returns>
    public ulong Mask(ulong mask) => bitboard & mask;

    public bool IsEmpty() => bitboard == 0;

    public override string ToString()
    {
        StringBuilder stringBuilder = new();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int squareIndex = rank * 8 + file;

                // Print rank number (descending)
                if (0.Equals(file))
                    stringBuilder.Append($" {8 - rank} ");

                // Convert the sqaure index to determine if occupied 
                var occupied = GetBit(squareIndex).Equals(0) ? 0 : 1;

                stringBuilder.Append($" {occupied}");
            }

            stringBuilder.AppendLine();
        }

        // Print files
        stringBuilder.AppendLine("\n    a b c d e f g h");

        // Print bit board decimal number
        stringBuilder.AppendLine($"\n    Bitboard: {bitboard}");

        return stringBuilder.ToString();
    }
}

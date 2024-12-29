using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core;

public static class Evaluation
{
    // Material value for all piece types (in centi pawns)
    private readonly static Dictionary<int, int> PieceValue = new()
    {
        { Piece.Pawn, 100 },
        { Piece.Knight, 350 },
        { Piece.Bishop, 350 },
        { Piece.Rook, 525 },
        { Piece.Queen, 1000 },
        { Piece.King, 10000 }
    };

    /// <summary>
    /// Evaluates the postion of the provided board. Factors that impact the score;  
    /// Balance of material, ...
    /// 
    /// Note:
    /// The score's sign + or - is subject to the colour to move. e.g. if the 
    /// position is evaluated to be 3 points better for white then if it's black's 
    /// move then the postion is -3 or +3 if it's white to move. 
    /// This is essential to work with the NegaMax search algorithm.
    /// </summary>
    /// <param name="board">The current state of the board</param>
    /// <returns>The score of the postion, subject to the colour to move</returns>
    public static int Evaluate(Board board)
    {
        int[] material = new int[2];

        foreach (int piece in Piece.AllPieceTypes)
        {
            ulong bitboard = board.PieceBitboards[piece];

            int colourIndex = Piece.GetPieceColour(piece) == Piece.White ? 0 : 1;

            material[colourIndex] += BitboardUtil.Count(bitboard) * 
                PieceValue[Piece.GetPieceType(piece)];
        }

        // TODO
        // Calculate mobility and apply weighting
        // (Total number of legal moves)

        int materialIndex = board.ColourToMove == Piece.White ? 0 : 1;

        return material[materialIndex] - material[materialIndex ^ 1];
    }
}

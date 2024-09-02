using Chess_Bot.Core.Bitboards;

namespace Chess_Bot.Core;

public static class Evaluation
{
    // Material value for all piece types (in centi pawns)
    public static Dictionary<int, int> PieceValue = new()
    {
        { Piece.Pawn, 100 },
        { Piece.Knight, 350 },
        { Piece.Bishop, 350 },
        { Piece.Rook, 525 },
        { Piece.Queen, 1000 },
        { Piece.King, 10000 }
    };

    // Simple static material evaluation returning the material balance
    // in terms of side to move
    public static int Evaluate(Board board)
    {
        int[] material = new int[2];

        foreach (int piece in Piece.AllPieceTypes)
        {
            Bitboard bitboard = board.PieceBitboards[piece];

            int colourIndex = Piece.GetPieceColour(piece) == Piece.White ? 0 : 1;

            material[colourIndex] += bitboard.Count() * PieceValue[Piece.GetPieceType(piece)];
        }

        // TODO
        // Calculate mobility and apply weighting
        // (Total number of legal moves)

        int materialIndex = board.ColourToMove == Piece.White ? 0 : 1;

        return material[materialIndex] - material[materialIndex ^ 1];
    }
}

using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core;

public class Board
{
    /// <summary>
    /// Represents all Pieces on the board. Each position in the array is a sqaure
    /// on the board, and the integer the Piece occupying it.
    /// </summary>
    public int[] BoardSquares = new int[64];

    /// <summary>
    /// Array of all piece bitboards with position corrisponding to Piece's number
    /// </summary>
    public Bitboard[] PieceBitboards = new Bitboard[15];

    public Bitboard OccupiedBitboard = new();
    public Bitboard EmptyBitboard = new();
    public Bitboard[] ColourAttacksBitboard = new Bitboard[2];

    public bool isCheck;
    public bool isCheckmate;
    public bool isStalemate;

    public bool CanWhiteKingSideCastle = true;
    public bool CanWhiteQueenSideCastle = true;
    public bool CanBlackKingSideCastle = true;
    public bool CanBlackQueenSideCastle = true;

    public int ColourToMove;
    public int OpositionColour => ColourToMove == Piece.White ? Piece.Black : Piece.White;

    private readonly Dictionary<int, Move> moveHistory = [];
    private int halfMoveCount;
    private int fullMoveCount = 1;

    public void SetPosition(string FENString)
    {
        var position = FENUtilities.FENToPosition(FENString);

        BoardSquares = position.BoardSquares;

        InitialisePieceBitboards();

        ColourToMove = position.ColourToMove;

        CanWhiteKingSideCastle = position.CanWhiteKingSideCastle;
        CanWhiteQueenSideCastle = position.CanWhiteQueenSideCastle;
        CanBlackKingSideCastle = position.CanBlackKingSideCastle;
        CanBlackQueenSideCastle = position.CanBlackQueenSideCastle;

        halfMoveCount = position.HalfMoveCount;
        fullMoveCount = position.FullMoveCount;
    }

    private void InitialisePieceBitboards()
    {
        for (int i = 0; i < PieceBitboards.Length; i++)
            PieceBitboards[i] = new();

        for (int squareIndex = 0; squareIndex < BoardSquares.Length; squareIndex++)
        {
            if (BoardSquares[squareIndex] == 0)
                continue;

            int piece = BoardSquares[squareIndex];
            int colour = Piece.GetPieceColour(BoardSquares[squareIndex]);

            // Put type into own bitboard
            PieceBitboards[piece].AddBit(squareIndex);

            // put colour into own bitboard
            PieceBitboards[colour].AddBit(squareIndex);

            OccupiedBitboard.AddBit(squareIndex);
        }

        EmptyBitboard = new(OccupiedBitboard.Invert());
    }

    public void MakeMove(Move move)
    {
        // Get peice from the start sqaure
        int piece = BoardSquares[move.StartSqaure];
        // Pick up piece
        BoardSquares[move.StartSqaure] = 0;

        // Make move
        BoardSquares[move.TargetSqaure] = piece;

        // Figure out if this is capture? info in move object?

        moveHistory.Add(fullMoveCount, move);
        fullMoveCount++;
    }

    public void ReverseMove(Move move)
    {
        int piece = BoardSquares[move.TargetSqaure];
        // Add captured piece to the board if required
        // else
        BoardSquares[move.TargetSqaure] = 0;

        BoardSquares[piece] = move.StartSqaure;

        moveHistory.Remove(fullMoveCount);
        fullMoveCount--;
    }
}

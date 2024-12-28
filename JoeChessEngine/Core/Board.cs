using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot.Core;

public class Board
{
    /// <summary>
    /// Represents all Pieces on the board. Each position in the array is a sqaure
    /// on the board, and the integer representing the Piece occupying it.
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

    public bool hasEnPassantTargetSquare;
    public int enPassantTargetSquare;

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

        hasEnPassantTargetSquare = position.HasEnPassantTargetSquare;
        enPassantTargetSquare = position.EnPassantTargetSquare;

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
        int piece = BoardSquares[move.StartSquare];

        // Pick up piece on all board representations
        BoardSquares[move.StartSquare] = 0;
        OccupiedBitboard.RemoveBit(move.StartSquare);
        PieceBitboards[piece].RemoveBit(move.StartSquare);

        // Remove captured piece or enPassant pawn from bitboard
        if (move.IsEnPassant)
        {
            PieceBitboards[BoardSquares[move.TargetPawnSquare]]
                .RemoveBit(move.TargetPawnSquare);
        }
        else if (move.IsCapture)
        {
            PieceBitboards[BoardSquares[move.TargetSquare]]
                .RemoveBit(move.TargetSquare);
        }

        // Make move on all board representations
        BoardSquares[move.TargetSquare] = piece;
        OccupiedBitboard.AddBit(move.TargetSquare);
        PieceBitboards[piece].AddBit(move.TargetSquare);

        // Move rook if castling
        if (move.IsCastling)
        {
            int rook = BoardSquares[move.RookStartSquare];

            BoardSquares[move.RookStartSquare] = 0;
            OccupiedBitboard.RemoveBit(move.StartSquare);
            PieceBitboards[rook].RemoveBit(move.StartSquare);

            BoardSquares[move.RookTargetSquare] = rook;
            OccupiedBitboard.AddBit(move.RookTargetSquare);
            PieceBitboards[rook].AddBit(move.RookTargetSquare);
        }
        
        // TODO: Promotion
        // TODO: 

        moveHistory.Add(halfMoveCount, move);
        
        // Increment move counts
        halfMoveCount++;
        if (halfMoveCount % 2 == 0) 
            fullMoveCount++;

        ToggleColourToMove();
    }

    public void ReverseMove(Move move)
    {
        // Reverse rook move if castle
        if (move.IsCastling)
        {
            int rook = BoardSquares[move.RookTargetSquare];

            BoardSquares[move.RookTargetSquare] = 0;
            OccupiedBitboard.RemoveBit(move.RookTargetSquare);
            PieceBitboards[rook].RemoveBit(move.RookTargetSquare);

            BoardSquares[move.RookStartSquare] = rook;
            OccupiedBitboard.AddBit(move.StartSquare);
            PieceBitboards[rook].AddBit(move.StartSquare);
        }

        int piece = BoardSquares[move.TargetSquare];
        BoardSquares[move.TargetSquare] = 0;
        OccupiedBitboard.RemoveBit(move.TargetSquare);
        PieceBitboards[piece].RemoveBit(move.TargetSquare);

        if (move.IsCapture)
        {
            int capturedPiece = move.CapturedPiece;
            int capturedPieceSquare = move.TargetSquare;

            if (move.IsEnPassant)
                capturedPieceSquare = move.TargetPawnSquare;

            // Add captured piece back to boards
            BoardSquares[capturedPieceSquare] = capturedPiece;
            OccupiedBitboard.AddBit(capturedPieceSquare);
            PieceBitboards[capturedPiece].AddBit(capturedPieceSquare);
        }

        // Add moved piece back to original position
        BoardSquares[move.StartSquare] = piece;
        OccupiedBitboard.AddBit(move.StartSquare);
        PieceBitboards[piece].AddBit(move.StartSquare);
        
        // Decrement move counts
        halfMoveCount--;
        if (halfMoveCount % 2 != 0)
            fullMoveCount--;

        moveHistory.Remove(halfMoveCount);

        ToggleColourToMove();
    }

    private void ToggleColourToMove() => ColourToMove = OpositionColour;
}

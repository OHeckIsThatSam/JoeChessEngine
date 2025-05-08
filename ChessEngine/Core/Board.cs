using ChessEngine.Core.Utilities;

namespace ChessEngine.Core;

public class Board : ICloneable
{
    /// <summary>
    /// Represents all Pieces on the board. Each position in the array is a sqaure
    /// on the board, and the integer representing the Piece occupying it.
    /// </summary>
    public int[] BoardSquares = new int[64];

    /// <summary>
    /// Array of all piece bitboards with position corrisponding to Piece's number
    /// </summary>
    public ulong[] PieceBitboards = new ulong[15];

    public ulong OccupiedBitboard = 0;
    public ulong EmptyBitboard => ~OccupiedBitboard;
    public ulong[] ColourAttacksBitboard = new ulong[2];

    public bool IsCheck;
    public bool IsCheckmate;
    public bool IsStalemate;

    public bool CanWhiteKingSideCastle = true;
    public bool CanWhiteQueenSideCastle = true;
    public bool CanBlackKingSideCastle = true;
    public bool CanBlackQueenSideCastle = true;

    public int ColourToMove;
    public int OpositionColour => ColourToMove == Piece.White ? Piece.Black : Piece.White;

    public bool HasEnPassantTargetSquare;
    public int EnPassantTargetSquare;
    private bool _initalHasEnPassant;
    private int _initalEnPassantTargetSquare;

    private Dictionary<int, Move> moveHistory = [];
    private int halfMoveCount;
    private int fullMoveCount = 1;

    public void SetPosition(string FENString)
    {
        var position = FENUtil.FENToPosition(FENString);

        BoardSquares = position.BoardSquares;

        InitialisePieceBitboards();

        ColourToMove = position.ColourToMove;

        CanWhiteKingSideCastle = position.CanWhiteKingSideCastle;
        CanWhiteQueenSideCastle = position.CanWhiteQueenSideCastle;
        CanBlackKingSideCastle = position.CanBlackKingSideCastle;
        CanBlackQueenSideCastle = position.CanBlackQueenSideCastle;

        HasEnPassantTargetSquare = position.HasEnPassantTargetSquare;
        EnPassantTargetSquare = position.EnPassantTargetSquare;
        _initalHasEnPassant= position.HasEnPassantTargetSquare;
        _initalEnPassantTargetSquare = position.EnPassantTargetSquare;

        halfMoveCount = position.HalfMoveCount;
        fullMoveCount = position.FullMoveCount;

        UpdateCheck();
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
            PieceBitboards[piece] = BitboardUtil.AddBit(
                PieceBitboards[piece], squareIndex);

            // Put colour into own bitboard
            PieceBitboards[colour]= BitboardUtil.AddBit(
                PieceBitboards[colour], squareIndex);

            OccupiedBitboard = BitboardUtil.AddBit(OccupiedBitboard, squareIndex);
        }
    }

    public void MakeMove(Move move)
    {
        // Get peice from the start sqaure
        int piece = BoardSquares[move.StartSquare];
        ulong pieceBitboard = PieceBitboards[piece];

        // Pick up piece on all board representations
        BoardSquares[move.StartSquare] = 0;
        OccupiedBitboard = BitboardUtil.RemoveBit(OccupiedBitboard, move.StartSquare);
        pieceBitboard = BitboardUtil.RemoveBit(pieceBitboard, move.StartSquare);
        
        // Remove captured piece or enPassant pawn from bitboard
        if (move.IsEnPassant)
        {
            int enPassantPiece = BoardSquares[move.TargetPawnSquare];
            PieceBitboards[enPassantPiece] = BitboardUtil.RemoveBit(
                PieceBitboards[enPassantPiece], move.TargetPawnSquare);
        }
        else if (move.IsCapture)
        {
            int capturedPiece = BoardSquares[move.TargetSquare];
            PieceBitboards[capturedPiece] = BitboardUtil.RemoveBit(
                PieceBitboards[capturedPiece], move.TargetSquare);
        }

        if (move.IsPromotion)
        {
            // Swap pawnbitboard for promotion type bitboard
            PieceBitboards[piece] = pieceBitboard;
            piece = move.PromotionType;
            pieceBitboard = PieceBitboards[piece];
        }

        // Make move on all board representations
        BoardSquares[move.TargetSquare] = piece;
        OccupiedBitboard = BitboardUtil.AddBit(OccupiedBitboard, move.TargetSquare);
        pieceBitboard = BitboardUtil.AddBit(pieceBitboard, move.TargetSquare);

        PieceBitboards[piece] = pieceBitboard;

        // Move rook if castling
        if (move.IsCastling)
        {
            int rook = BoardSquares[move.RookStartSquare];
            ulong rookBitboard = PieceBitboards[rook];

            BoardSquares[move.RookStartSquare] = 0;
            OccupiedBitboard = BitboardUtil.RemoveBit(
                OccupiedBitboard, move.RookStartSquare);
            rookBitboard = BitboardUtil.RemoveBit(rookBitboard, move.RookStartSquare);

            BoardSquares[move.RookTargetSquare] = rook;
            OccupiedBitboard = BitboardUtil.AddBit(
                OccupiedBitboard, move.RookTargetSquare);
            rookBitboard = BitboardUtil.AddBit(rookBitboard, move.RookTargetSquare);

            PieceBitboards[rook] = rookBitboard;
        }

        moveHistory.Add(halfMoveCount, move);
        
        // Increment move counts
        halfMoveCount++;
        if (halfMoveCount % 2 == 0) 
            fullMoveCount++;

        ToggleColourToMove();
        UpdateColourBitboards();
        UpdateCheck();
        UpdateEnPassant();
    }

    public void ReverseMove(Move move)
    {
        // Reverse rook move if castle
        if (move.IsCastling)
        {
            int rook = BoardSquares[move.RookTargetSquare];
            ulong rookBitboard = PieceBitboards[rook];

            BoardSquares[move.RookTargetSquare] = 0;
            OccupiedBitboard = BitboardUtil.RemoveBit(
                OccupiedBitboard, move.RookTargetSquare);
            rookBitboard = BitboardUtil.RemoveBit(rookBitboard, move.RookTargetSquare);
            
            BoardSquares[move.RookStartSquare] = rook;
            OccupiedBitboard = BitboardUtil.AddBit(
                OccupiedBitboard, move.RookStartSquare);
            rookBitboard = BitboardUtil.AddBit(rookBitboard, move.RookStartSquare);

            PieceBitboards[rook] = rookBitboard;
        }

        int piece = BoardSquares[move.TargetSquare];
        ulong pieceBitboard = PieceBitboards[piece];

        BoardSquares[move.TargetSquare] = 0;
        OccupiedBitboard = BitboardUtil.RemoveBit(
            OccupiedBitboard, move.TargetSquare);
        pieceBitboard = BitboardUtil.RemoveBit(pieceBitboard, move.TargetSquare);

        PieceBitboards[piece] = pieceBitboard;

        if (move.IsCapture)
        {
            int capturedPiece = move.CapturedPiece;
            int capturedPieceSquare = move.TargetSquare;

            if (move.IsEnPassant)
                capturedPieceSquare = move.TargetPawnSquare;

            // Add captured piece back to boards
            BoardSquares[capturedPieceSquare] = capturedPiece;
            OccupiedBitboard = BitboardUtil.AddBit(
                OccupiedBitboard, capturedPieceSquare);
            PieceBitboards[capturedPiece] = BitboardUtil.AddBit(
                PieceBitboards[capturedPiece], capturedPieceSquare);
        }

        if (move.IsPromotion)
        {
            piece = Piece.Pawn | OpositionColour;
            pieceBitboard = PieceBitboards[piece];
        }

        // Add moved piece back to original position
        BoardSquares[move.StartSquare] = piece;
        OccupiedBitboard = BitboardUtil.AddBit(OccupiedBitboard, move.StartSquare);
        PieceBitboards[piece] = BitboardUtil.AddBit(pieceBitboard, move.StartSquare);

        // Decrement move counts
        halfMoveCount--;
        if (halfMoveCount % 2 != 0)
            fullMoveCount--;

        moveHistory.Remove(halfMoveCount);

        ToggleColourToMove();
        UpdateColourBitboards();
        UpdateCheck();
        UpdateEnPassant();
    }

    private void ToggleColourToMove() => ColourToMove = OpositionColour;

    private void UpdateColourBitboards()
    {
        PieceBitboards[Piece.White] = 
            PieceBitboards[Piece.WhitePawn] |
            PieceBitboards[Piece.WhiteKnight] |
            PieceBitboards[Piece.WhiteBishop] |
            PieceBitboards[Piece.WhiteRook] |
            PieceBitboards[Piece.WhiteQueen] |
            PieceBitboards[Piece.WhiteKing];

        PieceBitboards[Piece.Black] =
            PieceBitboards[Piece.BlackPawn] |
            PieceBitboards[Piece.BlackKnight] |
            PieceBitboards[Piece.BlackBishop] |
            PieceBitboards[Piece.BlackRook] |
            PieceBitboards[Piece.BlackQueen] |
            PieceBitboards[Piece.BlackKing];
    }

    private void UpdateCheck()
    {
        ulong oppAttacks = AttackBitboards.GetAllAttacks(
            OpositionColour, OccupiedBitboard, PieceBitboards);

        IsCheck = (oppAttacks & PieceBitboards[Piece.King | ColourToMove]) != 0;
    }

    private void UpdateEnPassant()
    {
        if (moveHistory.TryGetValue(halfMoveCount - 1, out Move move))
        {
            if (move.HasEnPassant)
            {
                HasEnPassantTargetSquare = true;
                EnPassantTargetSquare = move.EnPassantTargetSquare;
            }
            else
            {
                HasEnPassantTargetSquare = false;
                EnPassantTargetSquare = 0;
            }
        }
        else
        {
            HasEnPassantTargetSquare = _initalHasEnPassant;
            EnPassantTargetSquare = _initalEnPassantTargetSquare;
        }
    }

    public object Clone()
    {
        return new Board()
        {
            BoardSquares = (int[])BoardSquares.Clone(),
            PieceBitboards = (ulong[])PieceBitboards.Clone(),
            OccupiedBitboard = OccupiedBitboard,

            IsCheck = IsCheck,
            IsCheckmate = IsCheckmate,
            IsStalemate = IsStalemate,

            CanWhiteKingSideCastle = CanWhiteKingSideCastle,
            CanWhiteQueenSideCastle = CanWhiteQueenSideCastle,
            CanBlackKingSideCastle = CanBlackKingSideCastle,
            CanBlackQueenSideCastle = CanBlackQueenSideCastle,

            ColourToMove = ColourToMove,

            HasEnPassantTargetSquare = HasEnPassantTargetSquare,
            EnPassantTargetSquare = EnPassantTargetSquare,
            moveHistory = moveHistory.ToDictionary(e => e.Key, e => e.Value),

            halfMoveCount = halfMoveCount,
            fullMoveCount = fullMoveCount,
        };
    }
}

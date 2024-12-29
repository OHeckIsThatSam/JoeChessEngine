namespace Chess_Bot.Core;

public struct Move
{
    public int StartSquare;
    public int TargetSquare;

    public bool IsCapture;
    public int CapturedPiece;

    public bool IsEnPassant;
    public int TargetPawnSquare;

    public bool IsCastling;
    public int RookStartSquare;
    public int RookTargetSquare;

    public bool IsPromotion;
    public int PromotionType;
}

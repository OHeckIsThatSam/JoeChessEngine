namespace Chess_Bot.Core;

public class Move
{
    public int StartSquare;
    public int TargetSquare;

    public bool IsCapture = false;
    public int CapturedPiece;

    public bool IsEnPassant = false;
    public int TargetPawnSquare;

    public bool IsCastling = false;
    public int RookStartSquare;
    public int RookTargetSquare;

    public bool IsPromotion;
}

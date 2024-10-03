namespace Chess_Bot.Core;

public class Move
{
    public int StartSquare;
    public int TargetSquare;

    public bool IsCapture = false;

    public bool IsEnPassant = false;
    public int TargetPawnSqaure;

    public bool IsCastling = false;
    public int RookStartSquare;
    public int RookTargetSquare;
}

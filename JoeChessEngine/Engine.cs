using Chess_Bot.Core;
using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot;

public class Engine
{
    Board board;

    public Engine()
    {
        // Dummy call to trigger AttackBitboard generation
        AttackBitboards.PawnAttacks[0, 0].Count();

        board = new Board();
    }

    public void CreateGame(string FENString = "")
    {
        if (string.IsNullOrEmpty(FENString))
            FENString = FENUtilities.STARTING_FEN_STRING;

        board.SetPosition(FENString);   

        Search search = new();
        Search.GenerateMoves(board);

        Console.WriteLine(Evaluation.Evaluate(board));
    }
}

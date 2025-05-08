using ChessEngine.Testing;

namespace ChessEngine;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing....");
        Test.TestMoveGeneration();

        Console.WriteLine("Chess");

        Engine engine = new();

        String input = String.Empty;

        while (input != "quit")
        {
            engine.CreateGame("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

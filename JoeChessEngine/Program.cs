using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;
using JoeChessEngine.Testing;

namespace Chess_Bot;

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
            engine.CreateGame("2k5/2p5/8/8/8/8/8/2RK4 b - - 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

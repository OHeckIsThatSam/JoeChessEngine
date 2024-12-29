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
            engine.CreateGame("1r1q4/2P2k2/8/8/8/5K2/3P2p1/5R1R b - - 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

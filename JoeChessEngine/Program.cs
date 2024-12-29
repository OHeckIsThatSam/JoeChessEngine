using Chess_Bot.Core.Utilities;
using JoeChessEngine.Testing;

namespace Chess_Bot;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing....");
        //Test.TestMoveGeneration();

        Console.WriteLine("Chess");

        Engine engine = new();

        String input = String.Empty;

        while (input != "quit")
        {
            engine.CreateGame("8/5k2/8/8/2p5/5K2/B2P4/8 b - - 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

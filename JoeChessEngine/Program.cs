using Chess_Bot.Core.Bitboards;
using Chess_Bot.Core.Utilities;

namespace Chess_Bot;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Chess");

        Engine engine = new();

        String input = String.Empty;

        while (input != "quit")
        {
            engine.CreateGame("r3k1nr/ppppp1pp/3q4/7Q/8/8/PPP1P1PP/R3K2R b KQkq - 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

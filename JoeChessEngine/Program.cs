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
            engine.CreateGame("8/8/8/1k6/3Pp3/8/8/4KQ2 b - d3 0 1");

            input = Console.ReadLine()?? "";
        }
    }
}

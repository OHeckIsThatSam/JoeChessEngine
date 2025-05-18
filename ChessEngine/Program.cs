using ChessEngine.Testing;

namespace ChessEngine;

internal class Program
{
    static void Main(string[] args)
    {
        // If running within Perftree test application (comparison to Stockfish)
        if (args.Length >= 2)
        {
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Replace("\"", "");
            }

            int depth = Convert.ToInt32(args[0]);
            string fen = args[1];
            string moves = "";
            if (args.Length == 3)
                moves = args[2];
            
            Perft.Create(depth, fen, moves);
            return;
        }

        // Enter cli interface
        Console.WriteLine("BACTS (Better At Chess Than Sam) v0.0.1");
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine()?? "";
            
            // Skip empty input
            if (input == "" || input == " ")
                continue;

            string[] tokens = input.Split(' ');
            string command = tokens[0];

            // Add UCI command logic
            switch (command.ToLower())
            {
                case "perft":
                    // Run default perft function
                    string path = Path.Combine(Directory.GetCurrentDirectory(),
                        "Testing/Positions/perft.txt");
                    string[] lines = File.ReadAllLines(path);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] line = lines[i].Split('"', StringSplitOptions.RemoveEmptyEntries);
                        string fen = line[0];
                        string[] other = line[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        int depth = Convert.ToInt32(other[0]);

                        Perft.Create(depth, fen, "");
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown command: \"{command}\"");
                    break;
            }
        }
    }
}

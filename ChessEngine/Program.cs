using ChessEngine.Core;
using ChessEngine.Core.Utilities;
using ChessEngine.Testing;
using System.Diagnostics;

namespace ChessEngine;

internal class Program
{
    static void Main(string[] args)
    {
        // Init Things
        AttackBitboards.Initialise();
        Magic.Initialise();

        for (int i = 0; i < args.Length; i++)
        {
            args[i] = args[i].Replace("\"", "");
        }

        string d = args[0];
        int depth = Convert.ToInt32(args[0]);
        string fen = args[1];

        Board position = new();
        position.SetPosition(fen);

        // If there are moves to make make them
        if (args.Length > 2) 
        {
            string[] moves = args[2].Split(' ');

            for (int i = 0; i < moves.Length; i++) 
            { 
                string move = moves[i];
                position.MakeMove(MoveUtil.UCIToMove(move, position));
            }
        }
        var sw = Stopwatch.StartNew();
        Test.CreatePerftree(position, depth);
        sw.Stop();
        File.AppendAllText(
            @"C:\Users\sam\OneDrive\Desktop\nps.txt", 
            $"{fen} {depth} {sw.ElapsedMilliseconds}\n");
    }
}

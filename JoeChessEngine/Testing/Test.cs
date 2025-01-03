using Chess_Bot.Core;
using JoeChessEngine.Core.Utilities;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Chess_Bot.Core.Utilities;

namespace JoeChessEngine.Testing;

static class Test
{
    static private Dictionary<int, int> _nodeCount = [];
    static private int _totalNodeCount;

    public static void TestMoveGeneration()
    {
        string path = Directory.GetCurrentDirectory();
        path += @"\Testing\MoveGeneration\testPositions.txt";

        string jsonText = File.ReadAllText(path);

        var json = JsonSerializer.Deserialize<JsonArray>(jsonText);

        foreach (var item in json)
        {
            ulong[] originalPieceValues = new ulong[15];
            _nodeCount = [];
            _totalNodeCount = 0;

            int targetDepth = Convert.ToInt32(item["depth"].ToString());
            int targetNodes = Convert.ToInt32(item["nodes"].ToString());
            string fen = item["fen"].ToString();

            Console.WriteLine("Creating game...");

            Board board = new();
            board.SetPosition(fen);

            Console.WriteLine($"Position: {fen}\n");

            Console.WriteLine(BoardUtil.BoardToString(board));

            // Time the search function
            Stopwatch stopwatch = Stopwatch.StartNew();
            Search.SearchMoves(board, targetDepth);
            stopwatch.Stop();

            // Build move tree to analyse accuracy of moves generated
            MoveTreeNode moves = CreateMoveTree(board, targetDepth);

            double timeTaken = stopwatch.Elapsed.TotalSeconds;
            double nodesPerSeconds = _totalNodeCount / timeTaken;

            Console.WriteLine($"Seconds: {timeTaken}");
            Console.WriteLine($"Total Nodes: {_totalNodeCount}");
            Console.WriteLine($"NPS: {nodesPerSeconds}\n");

            Console.WriteLine("Node Tree Breakdown:");
            foreach(var kv in _nodeCount)
            {
                Console.WriteLine($"Depth: {kv.Key} | Nodes: {kv.Value}");
            }
            Console.WriteLine();

            Console.WriteLine($"Test result: {(_totalNodeCount == targetNodes ? "SUCCESS" : "FAILURE")}");
            Console.WriteLine($"Expected nodes count: {targetNodes} - Actual node count: {_totalNodeCount}\n");

            Console.WriteLine("Moves generated. Enter to proceed... ");
            Console.ReadLine();
            Thread.Sleep(500);
            Console.Clear();
        }
    }

    private static MoveTreeNode CreateMoveTree(Board position, int depth)
    {
        MoveTreeNode root = new(position);

        if (depth == 0)
            return root;

        if (!_nodeCount.ContainsKey(depth))
            _nodeCount[depth] = 0;

        foreach (Move move in MoveGeneration.GenerateMoves(position))
        {
            position.MakeMove(move);
            MoveTreeNode child = CreateMoveTree(position, depth - 1);
            root.Add(child);
            position.ReverseMove(move);
        }

        _totalNodeCount += root.Count();
        _nodeCount[depth] += root.Count();

        return root;
    }
}

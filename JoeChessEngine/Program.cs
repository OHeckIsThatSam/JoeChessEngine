﻿namespace Chess_Bot;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Chess");

        Engine engine = new();

        String input = String.Empty;

        while (input != "quit")
        {

            //engine.CreateGame(FENUtilities.STARTING_FEN_STRING);
            engine.CreateGame("rnb1k1nr/p2p1p2/1pp3p1/B2Rp2p/3P1P2/b2QP1PN/1PP5/RN2K2R w kq - 0 1");
            //Bitboard bitboard = new Bitboard();
            //for (int r = 0; r < 8; r++)
            //{
            //    for (int f = 0; f < 8; f++)
            //    {
            //        int square = r * 8 + f;

            //        if (r == 7)
            //            bitboard.AddBit(square);
            //    }
            //}

            //Console.WriteLine(bitboard.ToString());

            //Bitboard blockers = new();
            //blockers.AddBit((int)BitboardUtilities.Sqaures.c4);
            //blockers.AddBit((int)BitboardUtilities.Sqaures.d4);
            //blockers.AddBit((int)BitboardUtilities.Sqaures.f5);
            //blockers.AddBit((int)BitboardUtilities.Sqaures.d8);
            //blockers.AddBit((int)BitboardUtilities.Sqaures.b7);
            //blockers.AddBit((int)BitboardUtilities.Sqaures.g2);
            //blockers.AddBit(36);

            //Console.WriteLine(blockers);


            //Console.WriteLine(AttackBitboards.GenerateRookAttacks((int)BitboardUtilities.Sqaures.d5, blockers));

            input = Console.ReadLine();
        }
    }
}
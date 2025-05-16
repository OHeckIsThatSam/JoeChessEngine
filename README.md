# ChessEngine

## Overview

I suck at Chess. But less so at coding, maybe? The aim of this project is to create an engine which is better than me at chess. I was inspired by [Sebastian Lague’s YouTube series](https://www.youtube.com/watch?v=_vqlIPDR2TU&list=PLFt_AvWsXl0cvHyu32ajwh2qU1i6hl77c), which is worth watching if you’re at all interested. My engine will be using a lot of the techniques Sebastian’s Chess Bot does. Mainly due to the quality of his explanations but also the use of these techniques in other engines and online resources means they're nicely digestible for a beginner like myself.

## Current features

- Bitboard board representation (magic bitboards)
- Legal move generation
- Negamax search
- Simple material evaluation

## Currently working on

- Move generation optimisation
- Alpha beta pruning

## Performance

### Move Generation

Average nodes per second: 17,105,132

|FEN                                                                      |Depth|Time (ms)|Nodes      |NPS       |
|:------------------------------------------------------------------------|:---:|--------:|----------:|---------:|
|"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"               |  6  |    8,450|119,060,324|14,089,979|
|"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"   |  5  |   10,887|193,690,690|17,791,006|
|"8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"                              |  7  |   11,526|178,633,661|15,498,322|
|"r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"       |  6  |   37,137|706,045,033|19,011,902|
|"rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"              |  5  |    4,826| 89,941,194|18,636,799|
|"r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 1"|  5  |    9,321|164,075,551|17,602,784|

## How to play

With the engine still being an early Work In Progress I haven’t yet implemented the functionality to make my engine playable. Once I have solid game representation, move generation and position evaluation I will make my engine playable. The aim would be to calculate an elo from games with rated players, which can then be used as a benchmark for future improvements.

## Useful resources

[The Chess Programming Wiki](https://www.chessprogramming.org/Main_Page) -  
A fantastic source of chess programming knowledge. A great starting point for suggestions of techniques and links to further reading. Personally there are times when reading pages on a subject I do not understand whole sentences and paragraphs. As such I found it’s best used in conjunction with plenty other resources (some more beginner friendly).

[Legal move generation article](https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/) - by Peter Elis Jones  
A really good article covering legal move generation, including gotchas and getting round them. I’m sure this article has saved me hours of debugging strange edge cases.

[Bitboard chess engine series](https://www.youtube.com/watch?v=QUNP-UjujBM&list=PLmN0neTso3Jxh8ZIylk74JpwfiWNI76Cs) - by ChessProgramming  
This series is a tutorial for a bitboard based chess engine written in C. The explanations and demos are incredibly helpful for the development of my engine and bridges the gap between theoretical approach and implementation.

[This article](https://analog-hors.github.io/site/magic-bitboards/) by Analog Hors and [This article](https://essays.jwatzman.org/essays/chess-move-generation-with-magic-bitboards.html) by Josh Watzman on Magic Bitboards -  
Really clear explanations on what Magic Bitboards are, the background on why they're used and, most importantly, how they work. Essential for me implementing them.

The [perftree](https://github.com/agausmann/perftree) tool - by agausmann  
A very useful cli tool for comparing the results of your move gen function against [Stockfish's](https://stockfishchess.org/) move gen. Has saved me loads of time debugging the final few edge cases in move gen that I missed to begin with. Particularly En Passant moves...  

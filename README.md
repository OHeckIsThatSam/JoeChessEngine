# JoeChessEngine

## Overview

I suck at Chess. But less so at coding, maybe? The aim of this project is to create an engine which is better than me at chess. I was inspired by Sebastian Lague’s YouTube series: https://www.youtube.com/watch?v=_vqlIPDR2TU&list=PLFt_AvWsXl0cvHyu32ajwh2qU1i6hl77c. Which is worth watching if you’re at all interested. My engine will be using a lot of the techniques Sebastian’s Chess Bot does. Mainly due to the quality of his explanations but also the wide use of the techniques means it’s nicely digestible for a beginner like myself.

Like a lot of people online chess was a great distraction during the lockdowns. I was taught chess by my granddad Joe but never got the chance to share the interest as an adult. I’m sure he’d have been interested having been a programmer himself. 

## Current features

- Bitboard board representation
- Almost legal move generation
- Negamax search

## Currently working on

- Castling
- Pins
- Move generation testing
- Magic bitboards for attack generation
- Alpha beta pruning

## How to play

With the engine still being an early Work In Progress I haven’t yet implemented the functionality to make my engine playable. Once I have solid game representation, move generation and position evaluation I will make my engine playable. The aim would be to calculate an elo from games with rated players, which can then be used as a benchmark for future improvements.

## Useful resorces

The Chess Programming Wiki - https://www.chessprogramming.org/Main_Page  
A fantastic source of chess programming knowledge. A great starting point for suggestions of techniques and links to further reading. Personally there are times when reading pages on a subject I do not understand whole sentences and paragraphs. As such I found it’s best used in conjunction with plenty other resources (some more beginner friendly).

Legal move generation article - https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/  
A really good article covering legal move generation, including gotchas and getting round them. I’m sure this article has saved me hours of debugging strange edge cases.

Bitboard chess engine series - https://www.youtube.com/watch?v=QUNP-UjujBM&list=PLmN0neTso3Jxh8ZIylk74JpwfiWNI76Cs  
This series by Chess Programming is a tutorial of a bitboard based chess engine written in C. The explanations and demos are incredibly helpful for the development of my engine and bridges the gap between theoretical approach and implementation.

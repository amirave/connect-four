using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AI
{
    // Standard minimax algorithm with alpha-beta pruning.
    // Returns a pair of the best move (used in game) and its corresponding score (used in recursive calls of the algorithm)
    // Called by the AI player
    public static Pair<int, int> Minimax(Board state, SlotType type, int depth, int alpha, int beta, bool myTurn)
    {
        // Each step of the recursion the turn swaps
        SlotType otherType = type == SlotType.One ? SlotType.Two : SlotType.One;

        // A board that is full has no valid moves (-1) and a score of zero
        if (state.IsFull())
            return new Pair<int, int>(-1, 0);

        if (depth == 0)
            return new Pair<int, int>(-1, EvaluateBoard(state, type));
        
        if (state.WinningMove() == type)
            return new Pair<int, int>(-1, 1000000);
        if (state.WinningMove() == otherType)
            return new Pair<int, int>(-1, -1000000);

        // If its the AI's turn, try to maximize score, if its the other player's turn, try to minimize score
        if (myTurn)
        {
            // The column with the highest score and its score
            int bestValue = int.MinValue;
            int bestC = 0;

            for (int c = 0; c < state.GetWidth(); c++)
            {
                if (!state.ValidMove(c))
                    continue;

                // Clone the board to not affect the old one
                Board next = new Board(state);
                int r = next.Set(c, type);
                int nextScore = Minimax(next, type, depth - 1, alpha, beta, false).GetSecond();

                if (nextScore > bestValue)
                {
                    bestValue = nextScore;
                    bestC = c;
                }

                alpha = Mathf.Max(alpha, bestValue);
                if (alpha >= beta)
                    break;
            }

            return new Pair<int, int>(bestC, bestValue);
        }
        else
        {
            // The column with the lowest score and its score
            int worseValue = int.MaxValue;
            int worstC = 0;

            for (int c = 0; c < state.GetWidth(); c++)
            {
                if (!state.ValidMove(c))
                    continue;

                // Clone the board to not affect the old one
                Board next = new Board(state);
                int r = next.Set(c, otherType);
                int nextScore = Minimax(next, type, depth - 1, alpha, beta, true).GetSecond();

                if (nextScore < worseValue)
                {
                    worseValue = nextScore;
                    worstC = c;
                }

                beta = Mathf.Min(beta, worseValue);
                if (alpha >= beta)
                    break;
            }

            return new Pair<int, int>(worstC, worseValue);
        }
    }

    // Give the whole board a score based on how close to winning a certain player is
    // Called when the minimax algorithm reaches its max depth
    private static int EvaluateBoard(Board board, SlotType piece)
    {
        int score = 0;

        // Score center column
        int centerCount = 0;
        for (int r = 0; r < board.GetHeight(); r++)
        {
            centerCount += board.Get(board.GetWidth() / 2, r) == piece ? 1 : 0;
        }
        score += centerCount * 3;

        // Score Horizontal
        for (int r = 0; r < board.GetHeight(); r++)
        {
            for (int c = 0; c < board.GetWidth() - 3; c++)
            {
                SlotType[] window = {board.Get(c, r), board.Get(c + 1, r), board.Get(c + 2, r), board.Get(c + 3, r)};
                score += EvaluateWindow(window, piece);
            }
        }

        // Score Vertical
        for (int c = 0; c < board.GetWidth(); c++)
        {
            for (int r = 0; r < board.GetHeight() - 3; r++)
            {
                SlotType[] window = {board.Get(c, r), board.Get(c, r + 1), board.Get(c, r + 2), board.Get(c, r + 3)};
                score += EvaluateWindow(window, piece);
            }
        }

        // Score positive sloped diagonal
        for (int r = 0; r < board.GetHeight() - 3; r++)
        {
            for (int c = 0; c < board.GetWidth() - 3; c++)
            {
                SlotType[] window = { board.Get(c, r), board.Get(c + 1, r + 1), board.Get(c + 2, r + 2), board.Get(c + 3, r + 3) };
                score += EvaluateWindow(window, piece);
            }
        }

        for (int r = 0; r < board.GetHeight() - 3; r++)
        {
            for (int c = 0; c < board.GetWidth() - 3; c++)
            {
                SlotType[] window = { board.Get(c, r + 3), board.Get(c + 1, r + 2), board.Get(c + 2, r + 1), board.Get(c + 3, r) };
                score += EvaluateWindow(window, piece);
            }
        }

        return score;
    }
    
    // Give each line in the board a score based on how close the given type is to winning in it
    private static int EvaluateWindow(SlotType[] line, SlotType type)
    {
        SlotType otherType = type == SlotType.One ? SlotType.Two : SlotType.One;

        int score = 0;

        if (CountLine(line, type) == 4)
            score += 100;
        else if (CountLine(line, type) == 3 && CountLine(line, SlotType.Empty) == 1)
            score += 20;
        else if (CountLine(line, type) == 2 && CountLine(line, SlotType.Empty) == 2)
            score += 2;

        if (CountLine(line, otherType) == 4)
            score -= 100;
        else if (CountLine(line, otherType) == 3 && CountLine(line, SlotType.Empty) == 1)
            score -= 20;
        else if (CountLine(line, otherType) == 2 && CountLine(line, SlotType.Empty) == 2)
            score -= 2;

        return score;
    }

    // Count
    private static int CountLine(SlotType[] line, SlotType type)
    {
        int count = 0;

        for (int i = 0; i < line.Length; i++)
            if (line[i] == type)
                count++;

        return count;
    }
}

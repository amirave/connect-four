using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minmax
{
    int depth;

    public Minmax(int depth)
    {
        this.depth = depth;
    }

    // Returns the chosen row
    public int MakeMove(Board board)
    {
        int column = Minimax(board, depth, int.MinValue, int.MaxValue, true).GetFirst();
        return column;
    }

    private Pair<int, int> Minimax(Board state, int depth, int a, int b, bool botTurn)
    {
        if (state.IsFull())
            return new Pair<int, int>(-1, 0);

        if (depth == 0)
            return new Pair<int, int>(-1, ScoreState(state));

        if (botTurn)
        {
            int value = int.MinValue + 10;
            int bestC = -1;

            for (int c = 0; c < state.GetWidth(); c++)
            {
                if (!state.ValidMove(c))
                    continue;

                // clone
                Board next = new Board(state);
                int r = next.Set(c, SlotType.Bot);

                int nextScore;

                if (next.IsWinPos(c, r, SlotType.Bot))
                    nextScore = int.MaxValue;
                else
                    nextScore = Minimax(next, depth - 1, a, b, !botTurn).GetSecond();

                if (nextScore > value)
                {
                    value = nextScore;
                    bestC = c;
                }

                a = Mathf.Max(a, value);
                if (a >= b)
                    break;
            }

            return new Pair<int, int>(bestC, value);
        }
        else
        {
            int value = int.MaxValue - 10;
            int worstC = -1;

            for (int c = 0; c < state.GetWidth(); c++)
            {
                if (!state.ValidMove(c))
                    continue;

                // clone
                Board next = new Board(state);
                int r = next.Set(c, SlotType.Player);

                int nextScore;

                if (next.IsWinPos(c, r, SlotType.Player))
                    nextScore = int.MinValue;
                else
                    nextScore = Minimax(next, depth - 1, a, b, !botTurn).GetSecond();

                if (nextScore < value)
                {
                    value = nextScore;
                    worstC = c;
                }

                b = Mathf.Min(b, value);
                if (a >= b)
                    break;
            }

            return new Pair<int, int>(worstC, value);
        }
    }

    private int ScoreState(Board state)
    {
        int score = 0;

        // get all lines and score for each one
        SlotType[,] allLines = state.GenerateLines();

        for (int i = 0; i < allLines.GetLength(0); i++)
        {
            score += ScoreLine(Utility.SliceRow(allLines, i));
        }

        return score;
    }

    private int ScoreLine(SlotType[] line)
    {
        int score = 0;

        if (CountLine(line, SlotType.Bot) == 4)
            score += 1000;

        else if (CountLine(line, SlotType.Bot) == 3 && CountLine(line, SlotType.Empty) == 1)
            score += 5;

        else if (CountLine(line, SlotType.Bot) == 2 && CountLine(line, SlotType.Empty) == 2)
            score += 2;

        if (CountLine(line, SlotType.Player) == 3 && CountLine(line, SlotType.Empty) == 1)
            score -= 1000;

        return score;
    }

    private int CountLine(SlotType[] line, SlotType type)
    {
        int count = 0;

        for (int i = 0; i < line.Length; i++)
            if (line[i] == type)
                count++;

        return count;
    }
}

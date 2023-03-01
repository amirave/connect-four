using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    int width;
    int height;

    SlotType[,] curState;

    public Board(int width, int height)
    {
        this.width = width;
        this.height = height;

        curState = new SlotType[width, height];
    }

    public Board(Board b)
    {
        this.width = b.width;
        this.height = b.height;
        this.curState = (SlotType[,])b.curState.Clone();
    }

    // First row that is empty in a column
    public int FirstEmpty(int c)
    {
        for (int i = 0; i < curState.GetLength(1); i++)
        {
            if (curState[c, i] == SlotType.Empty)
                return i;
        }

        return -1;
    }

    // Check if a move is valid (the given column is not full)
    public bool ValidMove(int c)
    {
        return Get(c, height-1) == SlotType.Empty;
    }

    public int GetWidth()
    {
        return curState.GetLength(0);
    }

    public int GetHeight()
    {
        return curState.GetLength(1);
    }

    public bool InBounds(int c, int r)
    {
        return (c < GetWidth() && r < GetHeight() && c >= 0 && r >= 0);
    }

    public SlotType Get(int x, int y)
    {
        return curState[x, y];
    }

    public int Set(int c, SlotType t)
    {
        int y = FirstEmpty(c);

        curState[c, y] = t;
        return y;
    }
    
    public bool IsFull()
    {
        for (int i = 0; i < GetWidth(); i++)
            for (int j = 0; j < GetHeight(); j++)
                if (Get(i, j) == SlotType.Empty)
                    return false;

        return true;
    }

    // Checks if a win occured in a certain position on the board by a certain player
    public SlotType WinningMove()
    {
        for (int x = 0; x < GetWidth(); x++)
        {
            for (int y = 0; y < GetHeight() - 3; y++)
            {
                if (curState[x, y] != SlotType.Empty &&
                    curState[x, y] == curState[x, y + 1] &&
                    curState[x, y] == curState[x, y + 2] &&
                    curState[x, y] == curState[x, y + 3])
                {
                    return curState[x, y];
                }
            }
        }

        //Check Vertical
        for (int x = 0; x < GetWidth() - 3; x++)
        {
            for (int y = 0; y < GetHeight(); y++)
            {
                if (curState[x, y] != SlotType.Empty &&
                    curState[x, y] == curState[x + 1, y] &&
                    curState[x, y] == curState[x + 2, y] &&
                    curState[x, y] == curState[x + 3, y])
                    return curState[x, y];
            }
        }

        //Check Diagonal (top left to bottom right)
        for (int x = 0; x < GetWidth() - 3; x++)
        {
            for (int y = 0; y < GetHeight() - 3; y++)
            {
                if (curState[x, y] != SlotType.Empty &&
                    curState[x, y] == curState[x + 1, y + 1] &&
                    curState[x, y] == curState[x + 2, y + 2] &&
                    curState[x, y] == curState[x + 3, y + 3])
                    return curState[x, y];
            }
        }

        //Check Diagonal (top right to bottom left)
        for (int x = 0; x < GetWidth() - 3; x++)
        {
            for (int y = 3; y < GetHeight(); y++)
            {
                if (curState[x, y] != SlotType.Empty &&
                    curState[x, y] == curState[x + 1, y - 1] &&
                    curState[x, y] == curState[x + 2, y - 2] &&
                    curState[x, y] == curState[x + 3, y - 3])
                    return curState[x, y];
            }
        }

        return SlotType.Empty;
    }
    
    public bool IsWinPos(int c, int r, SlotType t)
    {
        int combo = 0;
        // Horizontal
        for (int i = -3; i <= 3; i++)
        {
            if (InBounds(c + i, r) && Get(c + i, r) == t || i == 0)
                combo++;
            else
                combo = 0;
            if (combo >= 4)
                return true;
        }
        combo = 0;
        // Vertical
        for (int i = -3; i <= 3; i++)
        {
            if (InBounds(c, r + i) && Get(c, r + i) == t || i == 0)
                combo++;
            else
                combo = 0;
            if (combo >= 4)
                return true;
        }
        combo = 0;
        // Positive slopes
        for (int i = -3; i <= 3; i++)
        {
            if (InBounds(c + i, r + i) && Get(c + i, r + i) == t || i == 0)
                combo++;
            else
                combo = 0;
            if (combo >= 4)
                return true;
        }
        combo = 0;
        // Negative slopes
        for (int i = -3; i <= 3; i++)
        {
            if (InBounds(c - i, r + i) && Get(c - i, r + i) == t || i == 0)
                combo++;
            else
                combo = 0;
            if (combo >= 4)
                return true;
        }
        return false;
    }

    // If the game is won, returns the positions of the four-in-a-row
    // Called at the end of the game to draw a line where the win happened
    public int[] WinData(SlotType t)
    {
        for (int r = 0; r < GetHeight(); r++)
        {
            for (int c = 0; c < GetWidth() - 3; c++)
            {
                bool valid = true;

                for (int i = 0; i < 4; i++)
                    if (Get(c + i, r) != t)
                        valid = false;

                if (valid)
                    return new int[]{ c, r, c + 3, r};
            }
        }

        // vertical
        for (int r = 0; r < GetHeight() - 3; r++)
        {
            for (int c = 0; c < GetWidth(); c++)
            {
                bool valid = true;

                for (int i = 0; i < 4; i++)
                    if (Get(c, r + i) != t)
                        valid = false;

                if (valid)
                    return new int[] { c, r, c, r + 3 };
            }
        }

        // positive slopes
        for (int r = 0; r < GetHeight() - 3; r++)
        {
            for (int c = 0; c < GetWidth() - 3; c++)
            {
                bool valid = true;

                for (int i = 0; i < 4; i++)
                    if (Get(c + i, r + i) != t)
                        valid = false;

                if (valid)
                    return new int[] { c, r, c + 3, r + 3 };
            }
        }

        // negative slopes
        for (int r = 0; r < GetHeight() - 3; r++)
        {
            for (int c = 3; c < GetWidth(); c++)
            {
                bool valid = true;

                for (int i = 0; i < 4; i++)
                    if (Get(c - i, r + i) != t)
                        valid = false;

                if (valid)
                    return new int[] { c, r, c - 3, r + 3 };
            }
        };

        return null;
    }
}

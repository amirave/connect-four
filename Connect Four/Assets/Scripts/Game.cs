using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    Board board;
    Minmax bot;

    BoardDisplay display;

    public Game(BoardDisplay display, int width, int height, int searchDepth)
    {
        board = new Board(width, height);
        this.display = display;

        bot = new Minmax(searchDepth);
    }

    public void PlayerTurn(int c)
    {
        Set(c, SlotType.Player);

        int[] playerWinData = board.WinData(SlotType.Player);

        if (playerWinData != null)
        {
            display.Win(playerWinData, SlotType.Player);
            return;
        }

        if (board.IsFull())
        {
            display.Win(null, SlotType.Empty);
        }
    }

    public void BotTurn()
    {
        int botC = bot.MakeMove(board);
        Set(botC, SlotType.Bot);

        int[] BotWinData = board.WinData(SlotType.Bot);

        if (BotWinData != null)
        {
            display.Win(BotWinData, SlotType.Bot);
            return;
        }

        if (board.IsFull())
        {
            display.Win(null, SlotType.Empty);
        }
    }

    public Board GetBoard()
    {
        return board;
    }

    private void Set(int c, SlotType t)
    {
        int y = board.Set(c, t);
        display.DropPiece(t, c, y);
    }
}

public struct Pair<T, V>
{
    private T t;
    private V v;

    public Pair(T t, V v)
    {
        this.t = t;
        this.v = v;
    }

    public T GetFirst()
    {
        return t;
    }

    public V GetSecond()
    {
        return v;
    }
}
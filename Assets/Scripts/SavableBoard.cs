using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavableBoard
{
    public int[] state;
    public int width;
    public int height;
    public int[] playerTypes;

    public SavableBoard(Game game)
    {
        state = BoardToArray(game.GetBoard());
        width = game.GetBoard().GetWidth();
        height = game.GetBoard().GetHeight();
        
        playerTypes = new[] { Game.playerOneType, Game.playerOneDifficulty, Game.playerTwoType, Game.playerTwoDifficulty };
    }

    // Converts a board into a serializable one dimensional array
    // Called on save
    private int[] BoardToArray(Board b)
    {
        int[] arr = new int[b.GetWidth() * b.GetHeight()];

        for (int x = 0; x < b.GetWidth(); x++)
            for (int y = 0; y < b.GetHeight(); y++)
                arr[x * b.GetHeight() + y] = (int)b.Get(x, y);

        return arr;
    }

    // Converts a one dimensional integer array that is used for serialization to a board 
    // Called on load
    public Board ArrayToBoard()
    {
        Board b = new Board(width, height);

        for (int i = 0; i < state.Length; i++)
            b.Set(i / height, (SlotType)state[i]);

        return b;
    }
}

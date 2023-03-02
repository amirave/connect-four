using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    // Transferred between the main menu and game
    public static int playerOneType = 0;
    public static int playerTwoType = 1;

    public static int playerOneDifficulty;
    public static int playerTwoDifficulty;

    private Board board;

    public bool midTurn;
    private int turnCount;
    private bool gameEnded;

    private Player playerOne;
    private Player playerTwo;

    private BoardDisplay display;
    // Difficulty to depth - Easy: 1, Medium: 5, Hard: 8
    private int[] difficultySettings = { 2, 5, 7 };
    
    public bool firstsTurn => turnCount % 2 == 0;
    public Player curPlayer => firstsTurn ? playerOne : playerTwo;

    public Game(BoardDisplay display, Board board)
    {
        this.board = board;
        this.display = display;
        
        for (int x = 0; x < board.GetWidth(); x++)
            for (int y = 0; y < board.GetHeight(); y++)
                if (board.Get(x, y) != SlotType.Empty)
                    turnCount++;

        // Set up player types based on what was selected in the main menu
        if ((PlayerType) playerOneType == PlayerType.Human)
            playerOne = new HumanPlayer(SlotType.One, display, display.gamePiece);
        else
            playerOne = new AIPlayer(SlotType.One, difficultySettings[playerOneDifficulty]);
        
        if ((PlayerType) playerTwoType == PlayerType.Human)
            playerTwo = new HumanPlayer(SlotType.Two, display, display.gamePiece);
        else
            playerTwo = new AIPlayer(SlotType.Two, difficultySettings[playerTwoDifficulty]);
        
        StartTurn();
    }
    
    public void StartTurn()
    {
        string curPlayerName = firstsTurn ? "ONE" : "TWO";
        string curPlayerStatus = curPlayer.GetType() == typeof(HumanPlayer) ? "Waiting" : "Computing";
        display.UpdateTurnStatus($"PLAYER {curPlayerName}: {curPlayerStatus}...", firstsTurn);
        
        midTurn = true;
        curPlayer.StartTurn(this, EndTurn);
    }

    public void EndTurn(int c)
    {
        // Update board according to player
        SlotType curType = curPlayer.slotType;
        Set(c, curType);

        display.EndTurn();
        
        // Update game state
        midTurn = false;
        turnCount++;

        // Win state is draw
        if (board.IsFull())
        {
            gameEnded = true;
            display.Win(null, SlotType.Empty);
            return;
        }
        
        // Check win state
        int[] winData = board.WinData(curType);

        if (winData != null)
        {
            gameEnded = true;
            display.Win(winData, curType);
            return;
        }

        if (gameEnded == false)
            display.StartCoroutine(WaitForStartTurn());
    }

    // Small wait before starting next turn, to give time to the drop animation to finish
    private IEnumerator WaitForStartTurn()
    {
        yield return new WaitForSeconds(display.dropAnimSpeed * 1.1f);
        StartTurn();
    }

    private void Set(int c, SlotType t)
    {
        int y = board.Set(c, t);
        display.DropPiece(t, c, y);
    }

    // Takes a savable board that was serialized and converts it into a board object
    // Called when loading a previous save
    public static Game FromSavableBoard(SavableBoard sb, BoardDisplay display)
    {
        Game.playerOneType = sb.playerTypes[0];
        Game.playerOneDifficulty = sb.playerTypes[1];
        Game.playerTwoType = sb.playerTypes[2];
        Game.playerTwoDifficulty = sb.playerTypes[3];

        Board board = sb.ArrayToBoard();
        Game game = new Game(display, board);

        return game;
    }

    // Destroys unused players to not cause conflicts with new ones
    // Called when loading a previous save (the old one is not needed)
    public void Destroy()
    {
        playerOne.Destroy();
        playerTwo.Destroy();
    }

    public Player GetPlayerOne()
    {
        return playerOne;
    }

    public Player GetPlayerTwo()
    {
        return playerTwo;
    }

    public Board GetBoard()
    {
        return board;
    }

    public int GetTurnCount()
    {
        return turnCount;
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

public enum SlotType
{
    Empty = 0,
    One = 1,
    Two = 2
}

public enum PlayerType
{
    Human = 0,
    AI = 1
}
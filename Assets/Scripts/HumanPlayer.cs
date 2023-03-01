using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class HumanPlayer : Player
{
    private Action<int> callback = null;
    
    private BoardDisplay display;
    private Game game;
    private Transform hoverPiece;
    
    public override string playerKind => "Human";
    public override bool canSuggest => true;

    public HumanPlayer(SlotType slotType, BoardDisplay display, GameObject gamePiece) : base(slotType)
    {
        this.display = display;
        
        GameObject piece = GameObject.Instantiate(gamePiece);
        
        Color c = slotType == SlotType.One ? display.playerOneColor : display.playerTwoColor;
        piece.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 0.2f);
        
        piece.SetActive(false);
        
        hoverPiece = piece.transform;
    }

    public override void StartTurn(Game game, Action<int> callback)
    {
        this.callback = callback;
        this.game = game;
        
        hoverPiece.gameObject.SetActive(true);
    }

    public override void Update()
    {
        if (destroyed || callback == null)
            return;
        
        int c = display.ClosestColumn();
        int r = game.GetBoard().FirstEmpty(c);
        
        // Update hover piece to follow the user's cursor
        if (r != -1)
        {
            Vector3 hoverTarget = display.BoardToWorldPosition(c, r);
            hoverPiece.position = Vector3.Lerp(hoverPiece.position, hoverTarget, 1 * Time.deltaTime * 30);
        }

        // When user clicks, check if move is valid and if so set it
        if (Input.GetMouseButtonDown(0))
        {
            if (game.GetBoard().ValidMove(c) && display.IsMouseOverUI(Input.mousePosition) == false)
            {
                EndTurn(c);
            }
        }
    }

    // Calls back to the game logic to say that the player's turn is finished
    public void EndTurn(int c)
    {
        hoverPiece.gameObject.SetActive(false);
        callback(c);
    }

    public override void Destroy()
    {
        destroyed = true;
        Object.Destroy(hoverPiece.gameObject);
        game = null;
        callback = null;
    }
}
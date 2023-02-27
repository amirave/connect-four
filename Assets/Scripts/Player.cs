using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
    public SlotType slotType;
    public string playerName;

    protected bool destroyed;

    // The type of the player: "Human", "AI", etc...
    public abstract string playerKind { get; }
    // Whether the game should suggest a move to this type of player
    public abstract bool canSuggest { get; }
    
    public Player(SlotType slotType)
    {
        this.slotType = slotType;
    }

    public abstract void StartTurn(Game game, Action<int> callback);

    public abstract void Update();

    public abstract void Destroy();
}

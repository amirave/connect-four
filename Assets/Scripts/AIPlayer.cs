using System;
using Random = UnityEngine.Random;

public class AIPlayer : Player
{
    private Action<int> callback = null;
    private Game game;
    int depth;

    public override string playerKind => "AI";
    public override bool canSuggest => false;

    public AIPlayer(SlotType slotType, int depth) : base(slotType)
    {
        this.depth = depth;
    }


    public override void StartTurn(Game game, Action<int> callback)
    {
        this.callback = callback;
        this.game = game;
    }

    public override void Update()
    {
        if (destroyed || callback == null)
            return;

        // If AI is first, pick a random spot to add variation to the game
        if (game.GetTurnCount() == 0)
            callback(Random.Range(0, game.GetBoard().GetWidth()));
        else
        {
            int column = Algorithms.Minimax(game.GetBoard(), slotType, depth, int.MinValue, int.MaxValue, true).GetFirst();
            callback(column);
        }
    }

    public override void Destroy()
    {
        destroyed = true;
        game = null;
        callback = null;
    }
}

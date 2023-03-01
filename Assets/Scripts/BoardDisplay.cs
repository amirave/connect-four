using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BoardDisplay : MonoBehaviour
{
    public Vector2Int boardSize = new Vector2Int(0, 0);
    public float dropSpeed = 1f;
    public int[] difficultySettings = {1, 3, 5};

    public GameObject gamePiece;
    // For displaying where a piece will be placed
    public LineRenderer line;
    public GameObject winScreen;

    public GameObject ui;
    public GameObject suggestUI;
    public GameObject suggestionHighlightUI;
    public TMP_Text turnStatusText;

    public Color playerOneColor;
    public Color playerTwoColor;

    private Game game;

    private List<RaycastResult> uiRaycastBuffer;
    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        uiRaycastBuffer = new List<RaycastResult>();
        
        Board board = new Board(boardSize.x, boardSize.y);
        game = new Game(this, board);
    }

    private void Update()
    {
        if (game.midTurn)
        {
            suggestUI.SetActive(game.curPlayer.canSuggest);
            game.curPlayer.Update();
        }
    }
    
    public void EndTurn()
    {
        suggestUI.SetActive(false);
        suggestionHighlightUI.SetActive(false);
    }
    
    // Draws a circle around the what the AI thinks is the optimal move
    // Called by Unity when "Suggest a move" button is pressed in game
    public void OnSuggest()
    {
        int c = AI.Minimax(game.GetBoard(), game.curPlayer.slotType, 3, int.MinValue, int.MaxValue, true).GetFirst();
        int r = game.GetBoard().FirstEmpty(c);

        Vector3 pos = BoardToWorldPosition(c, r);
        pos = new Vector3(pos.x + 0.2f, pos.y, pos.z);
        suggestionHighlightUI.SetActive(true);
        suggestionHighlightUI.transform.position = Camera.main.WorldToScreenPoint(pos);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void SaveBoard()
    {
        SaveManager.Save(new SavableBoard(game));
    }

    public void LoadGame()
    {
        SavableBoard sb = SaveManager.Load();

        if (sb == null)
            return;
        
        game.Destroy();
        
        game = Game.FromSavableBoard(sb, this);
        ResetBoardDisplay();
    }

    public void DropPiece(SlotType t, int x, int y)
    {
        Vector3 pos = BoardToWorldPosition(x, y);

        GameObject piece = Instantiate(gamePiece, pos, Quaternion.Euler(0, 0, 90));
        piece.transform.parent = transform;

        Color c = t == SlotType.One ? playerOneColor : playerTwoColor;
        piece.GetComponent<MeshRenderer>().material.color = new Color(c.r, c.g, c.b, 0.2f);
        StartCoroutine(DropAnim(piece.transform, x, y));
    }

    // Drops a piece in place
    public IEnumerator DropAnim(Transform t, int x, int y)
    {
        Vector3 start = BoardToWorldPosition(x, y + 1.5f);
        Vector3 end = BoardToWorldPosition(x, y);

        Material mat = t.GetComponent<MeshRenderer>().material;

        float startTime = Time.time;
        float endTime = startTime + dropSpeed;

        while (Time.time <= endTime)
        {
            var i = (Time.time - startTime) / dropSpeed;
            
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, i);
            t.position = Vector3.Lerp(start, end, i);
            
            yield return null;
        }

        t.position = end;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1);
    }

    // Destroys all game pieces and places new ones
    // Used when loading a saved state
    public void ResetBoardDisplay()
    {
        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);

        Board b = game.GetBoard();

        for (int x = 0; x < b.GetWidth(); x++)
            for (int y = 0; y < b.GetHeight(); y++)
                if (b.Get(x, y) != SlotType.Empty)
                    DropPiece(b.Get(x, y), x, y);
    }

    public void Win(int[] points, SlotType winner)
    {
        StartCoroutine(WinAnim(points, winner));
    }

    // Draws the line where the win happened and animates the victory screen
    IEnumerator WinAnim(int[] points, SlotType winner)
    {
        // Draw a line across the four-in-a-row only if one of the players won
        if (winner != SlotType.Empty)
        {
            Vector3 start = BoardToWorldPosition(points[0] - 0.1f, points[1] - 0.1f);
            Vector3 end = BoardToWorldPosition(points[2] - 0.1f, points[3] - 0.1f);
            end += (Camera.main.transform.position - end) / 3;
            start += (Camera.main.transform.position - start) / 3;

            yield return new WaitForSeconds(1);

            yield return StartCoroutine(LineAnim(start, end));

            yield return new WaitForSeconds(1.5f);
        }
        
        winScreen.SetActive(true);
        
        TMP_Text t1 = winScreen.transform.GetChild(0).GetComponent<TMP_Text>();
        TMP_Text t2 = winScreen.transform.GetChild(1).GetComponent<TMP_Text>();
            
        string blueType = game.GetPlayerOne().playerKind;
        string redType = game.GetPlayerTwo().playerKind;

        if (winner == SlotType.Empty)
        {
            t1.text = "Draw!";
            t2.text = "the board is completely full but nobody won.";
        }
        else if (winner == SlotType.One)
        {
            t1.text = "BLUE Won!";
            t2.text = "Blue (" + blueType + ") has defeated red (" + redType + ") in " + game.GetTurnCount() + " moves.";
        }
        else if (winner == SlotType.Two)
        {
            t1.text = "RED Won!";
            t2.text = "Red (" + redType + ") has defeated blue (" + blueType + ") in " + game.GetTurnCount() + " moves.";
        }
    }

    IEnumerator LineAnim(Vector3 start, Vector3 end)
    {
        float speed = 1;
        float i = 0;

        while (i < 1)
        {
            line.SetPositions(new Vector3[] { start, Vector3.Lerp(start, end, i) });

            i += speed / 100;
            speed *= 1.001f;
            yield return null;
        }
    }

    // Updates the on-screen status text to show the user what is currently happening.
    // Called at the start of a turn
    public void UpdateTurnStatus(string newText, bool firstsTurn)
    {
        turnStatusText.text = newText;
        turnStatusText.color = firstsTurn ? playerOneColor : playerTwoColor;
    }

    // Converts a position on the board to a 3D position in the world
    public Vector3 BoardToWorldPosition(float x, float y)
    {
        // Using bounding box of object to determine the board size
        Vector3 bottomLeft = new Vector3(0, -boxCollider.bounds.size.y / 2, -boxCollider.bounds.size.z / 2) + boxCollider.bounds.center;
        float width = boxCollider.bounds.size.z;
        float height = boxCollider.bounds.size.y;

        Vector3 pos = bottomLeft + Vector3.forward * ((x + 1f) / (boardSize.x + 1f) * width) +
                      Vector3.up * ((y + 1f) / (boardSize.y + 1f) * height);

        return pos;
    }
    
    // Returns the closest column to the mouse's position
    public int ClosestColumn()
    {
        Vector2 cursor = Input.mousePosition;
        int closest = -1;
        float sqrMinDist = float.MaxValue;

        for (int i = 0; i < boardSize.x; i++)
        {
            Vector3 cWorldPos = BoardToWorldPosition(i, 0);
            Vector2 cPos = Camera.main.WorldToScreenPoint(cWorldPos);

            if ((cursor - cPos).sqrMagnitude < sqrMinDist)
            {
                sqrMinDist = (cursor - cPos).sqrMagnitude;
                closest = i;
            }
        }

        return closest;
    }
    
    public bool IsMouseOverUI(Vector2 position)
    {
        EventSystem eventSystem = ui.GetComponent<EventSystem>();
        uiRaycastBuffer.Clear();
            
        PointerEventData currentPositionData = new PointerEventData(eventSystem)
        {
            position = position
        };
            
        EventSystem.current.RaycastAll(currentPositionData, uiRaycastBuffer);
        uiRaycastBuffer.RemoveAll(res => res.gameObject.CompareTag("Suggestion"));
        return uiRaycastBuffer.Count > 0;
    }
}
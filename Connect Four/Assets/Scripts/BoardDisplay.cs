using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardDisplay : MonoBehaviour
{
    public Vector2Int boardSize = new Vector2Int(0, 0);
    public float dropSpeed = 1f;
    public int searchDepth = 5;

    public GameObject playerPiece;
    public GameObject botPiece;
    // For displaying where a piece will be placed
    public GameObject hoverPiece;
    public LineRenderer line;
    public GameObject winScreen;

    private Game gameBoard;

    private bool gameEnded = false;
    private bool playerTurn = true;
    private bool canPlace = true;
    private int moves = 0;

    void Start()
    {
        gameBoard = new Game(this, boardSize.x, boardSize.y, searchDepth);
    }

    void Update()
    {
        int c = ClosestColumn();
        int r = gameBoard.GetBoard().FirstEmpty(c);

        hoverPiece.SetActive(!gameEnded && playerTurn);
        if (r != -1)
        {
            Vector3 hoverTarget = BoardToWorldPosition(c, r);
            hoverPiece.transform.position = Vector3.Lerp(hoverPiece.transform.position, hoverTarget, 1 * Time.deltaTime * 30);
        }

        if (playerTurn)
        {
            if (Input.GetMouseButtonDown(0) && !gameEnded)
            {
                if (gameBoard.GetBoard().ValidMove(c))
                {
                    gameBoard.PlayerTurn(c);
                    moves++;
                }

                playerTurn = false;
                canPlace = false;
            }
        }
        else
        {
            gameBoard.BotTurn();
            playerTurn = true;
            canPlace = true;
        }
        
    }

    int ClosestColumn()
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

    public void DropPiece(SlotType t, int x, int y)
    {
        Vector3 pos = BoardToWorldPosition(x, y);

        GameObject piece = Instantiate((t == SlotType.Player ? playerPiece : botPiece), pos, Quaternion.Euler(0, 0, 90));
        piece.transform.parent = transform;
        StartCoroutine(DropAnim(piece.transform, x, y));
    }

    public IEnumerator DropAnim(Transform t, int x, int y)
    {
        Vector3 start = BoardToWorldPosition(x, y + 1.5f);
        Vector3 end = BoardToWorldPosition(x, y);

        Material mat = t.GetComponent<MeshRenderer>().material;

        float i = 0;

        while (i < 1.1f)
        {
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, i);
            t.position = Vector3.Lerp(start, end, i);

            i += 0.1f;
            yield return new WaitForSeconds(1 / dropSpeed);
        }
    }

    public void Win(int[] points, SlotType winner)
    {
        StartCoroutine(WinAnim(points, winner));
    }

    IEnumerator WinAnim(int[] points, SlotType winner)
    {
        gameEnded = true;

        if (winner == SlotType.Empty)
        {
            Transform t = winScreen.transform;

            t.GetChild(0).GetComponent<TMP_Text>().text = "Draw!";
            t.GetChild(1).GetComponent<TMP_Text>().text = "the board is completely full and yet nobody won.";
        }
        else
        {
            Vector3 start = BoardToWorldPosition(points[0] - 0.1f, points[1] - 0.1f);
            Vector3 end = BoardToWorldPosition(points[2] - 0.1f, points[3] - 0.1f);
            end += (Camera.main.transform.position - end) / 3;
            start += (Camera.main.transform.position - start) / 3;

            yield return new WaitForSeconds(1);

            yield return StartCoroutine(LineAnim(start, end));

            yield return new WaitForSeconds(1.5f);

            winScreen.SetActive(true);
            SetWinData(winner == SlotType.Player, moves);
        }
    }

    void SetWinData(bool playerWon, int moves)
    {
        Transform t = winScreen.transform;

        if (playerWon)
        {
            t.GetChild(0).GetComponent<TMP_Text>().text = "You Win!";
            t.GetChild(1).GetComponent<TMP_Text>().text = "you have defeated the AI in " + moves + " moves.";
        }
        else
        {
            t.GetChild(0).GetComponent<TMP_Text>().text = "You Lose!";
            t.GetChild(1).GetComponent<TMP_Text>().text = "you have been defeated by the AI in " + moves + " moves.";
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

    Vector3 BoardToWorldPosition(float x, float y)
    {
        // Using bounding box of object to determine the board size
        BoxCollider col = GetComponent<BoxCollider>();

        Vector3 bottomLeft = new Vector3(0, -col.bounds.size.y / 2, -col.bounds.size.z / 2) + col.bounds.center;
        float width = col.bounds.size.z;
        float height = col.bounds.size.y;

        Vector3 pos = bottomLeft + Vector3.forward * ((x + 1f) / (boardSize.x + 1f) * width) + 
                                Vector3.up * ((y + 1f) / (boardSize.y + 1f) * height);

        return pos;
    }
}

public enum SlotType
{
    Empty,
    Player,
    Bot
}
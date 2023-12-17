using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridLogic : MonoBehaviour
{
    public static int PlayerScore { get; private set; } = 0;
    private float runningScoreInterval = 0;

    public GameObject TileObject;
    private static readonly int Width = 10;
    private static readonly int Height = 10;

    [SerializeField] private GameObject[,] tiles = new GameObject[Width, Height];
    private readonly float[,] tileValue = new float[Width, Height];

    public int PlayerPosX { get; private set; }
    public int PlayerPosY { get; private set; }
    private int prevPosX = 0;
    private int prevPosY = 0;
    private float playerValue;

    void Start()
    {
        // We create game board.
        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Height; x++)
            {
                // Instantiate the tile
                tiles[x, y] = Instantiate(TileObject,
                                        new Vector3(x, 0, y),
                                        TileObject.transform.rotation);
                // Instantiate tile values
                tileValue[x, y] = Mathf.Sin(x) + Mathf.Cos(y) + 5;
                tiles[x, y].GetComponent<MeshRenderer>().material.color = Color.white / tileValue[x,y];
            }
        }

        UpdatePlayerPos(PlayerPosX, PlayerPosY);
    }

    void Update()
    {
        if (restClient.loggedIn || restClient.offlinePlay)
        {
            runningScoreInterval += playerValue * Time.deltaTime;
            if (runningScoreInterval >= 1)
            {
                PlayerScore++;
                runningScoreInterval--;
            }
            UpdatePlayerPos(PlayerPosX, PlayerPosY);
        }

    }

    public void MovePlayer(int addX, int addY)
    {
        // We ignore movement while we are not logged in and not playing offline.
        if (!restClient.loggedIn && !restClient.offlinePlay)
            return;

        // We update player position, if movement is legal.
        int temp = PlayerPosX + addX;
        if (temp >= 0 || temp <= 9)
        {
            PlayerPosX += addX;
        }
        temp = PlayerPosY + addY;
        if (temp >= 0 || temp <= 9)
        {
            PlayerPosY += addY;
        }
    }

    private void UpdatePlayerPos(int posX, int posY)
    {
        // Make sure our position is valid.
        if (posX < 0 || posX > 9 || posY < 0 || posY > 9)
        {
            PlayerPosX = prevPosX;
            PlayerPosY = prevPosY;
            return;
        }
        // Change previous location color back to normal.
        tiles[prevPosX, prevPosY].GetComponent<MeshRenderer>().material.color = Color.white / tileValue[prevPosX, prevPosY];

        // Set player position value to new tile and color to player color.
        playerValue = tileValue[posX, posY];
        tiles[posX, posY].GetComponent<MeshRenderer>().material.color = Color.red;

        // Keep track of previous position in case of errors.
        prevPosX = posX;
        prevPosY = posY;
    }
}

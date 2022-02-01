using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "Boards", order = 1)]
public class BoardSO : ScriptableObject
{
    //TODO: Integrate Level Goals into this script.

    [SerializeField] string boardName;
    public string BoardName { get => boardName; }

    [SerializeField] Vector2Int boardSize;
    public Vector2Int BoardSize { get => boardSize; }

    [SerializeField] int borderSize;
    public int Bordersize { get => borderSize; }

    [SerializeField] BoardCounter boardCounter;
    public BoardCounter BoardCounter { get => boardCounter; }

    [SerializeField] int startingTime = 60;
    public int StartingTime { get => startingTime; }



    [SerializeField] int startingMoves = 30;
    public int StartingMoves { get => startingMoves; }

    [SerializeField] int[] scoreGoals = new int[3] { 1000, 2000, 3000 };
    public int[] ScoreGoals { get => scoreGoals; }

    [SerializeField] PieceToCollect[] piecesToCollect;
    public PieceToCollect[] PiecesToCollect { get => piecesToCollect; }


    [SerializeField] GameObject tilePrefabNormal;
    public GameObject TilePrefabNormal { get => tilePrefabNormal; }

    [SerializeField] GameObject tilePrefabObstacle;
    public GameObject TilePrefabObstacle { get => tilePrefabObstacle; }

    [SerializeField] GameObject tilePrefabBrakable1;
    public GameObject TilePrefabBrakable1 { get => tilePrefabBrakable1; }

    [SerializeField] GameObject tilePrefabBrakable2;
    public GameObject TilePrefabBrakable2 { get => tilePrefabBrakable2; }



    [SerializeField] StartingObject[] startingTiles;
    public StartingObject[] StartingTiles { get => startingTiles; }

    [SerializeField] StartingObject[] startingGamePieces;
    public StartingObject[] StartingGamePieces { get => startingGamePieces; }



    [SerializeField] GameObject[] gamePiecePrefabs;
    public GameObject[] GamePiecePrefabs { get => gamePiecePrefabs; }

    [SerializeField] GameObject[] adjacentBombPrefabs;
    public GameObject[] AdjacentBombPrefabs { get => adjacentBombPrefabs; }

    [SerializeField] GameObject[] columnBombPrefabs;
    public GameObject[] ColumnBombPrefabs { get => columnBombPrefabs; }

    [SerializeField] GameObject[] rowBombPrefabs;
    public GameObject[] RowBombPrefabs { get => rowBombPrefabs; }

    [SerializeField] GameObject colorBombPrefab;
    public GameObject ColorBombPrefab { get => colorBombPrefab; }



    [SerializeField] int maxCollectibles = 3;
    public int MaxCollectibles { get => maxCollectibles; }

    [Range(0, 1)]
    [SerializeField] float chanceForCollectible = 0.1f;
    public float ChanceForCollectible { get => chanceForCollectible; }

    [SerializeField] GameObject[] collectiblePrefabs;
    public GameObject[] CollectiblePrefabs { get => collectiblePrefabs; }

}

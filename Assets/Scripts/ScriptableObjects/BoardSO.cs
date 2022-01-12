using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "Boards", order = 1)]
public class BoardSO : ScriptableObject
{
    //TODO: Integrate Level Goals into this script.

    [SerializeField] string boardName;
    public Vector2Int boardSize;
    public int borderSize;
    public LevelCounter levelCounter;

    public GameObject tilePrefabNormal;
    public GameObject tilePrefabObstacle;
    public GameObject tilePrefabBrakable1;
    public GameObject tilePrefabBrakable2;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    public GameObject[] gamePiecePrefabs;
    public GameObject[] adjacentBombPrefabs;
    public GameObject[] columnBombPrefabs;
    public GameObject[] rowBombPrefabs;
    public GameObject colorBombPrefab;

    public int maxCollectibles = 3;
    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;
    public GameObject[] collectiblePrefabs;
}

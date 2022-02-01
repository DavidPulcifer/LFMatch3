using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class StartingObject
{
    public GameObject prefab;
    public int x;
    public int y;
    public int z;
}

[RequireComponent(typeof(BoardDeadlock))]
[RequireComponent(typeof(BoardShuffler))]
public class Board : Singleton<Board>
{
    [SerializeField] BoardSO boardSO;

    public BoardSO BoardSO
    {
        get { return boardSO; }
    }

    int width;
    int height;

    GameObject m_clickedTileBomb;
    GameObject m_targetTileBomb;

    public float swapTime = 0.5f;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;

    bool m_playerInputEnabled = true;

    ParticleManager m_particleManager;    

    [SerializeField] int fillYOffset = 10;
    [SerializeField] float fillMoveTime = 0.5f;

    int m_scoreMultiplier = 0;
    int collectibleCount = 0;

    public bool isRefilling = false;

    BoardDeadlock m_boardDeadlock;
    BoardShuffler m_boardShuffler;

    public override void Awake()
    {
        base.Awake();
        width = boardSO.BoardSize.x;
        height = boardSO.BoardSize.y;
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
        m_boardDeadlock = GetComponent<BoardDeadlock>();
        m_boardShuffler = GetComponent<BoardShuffler>();        
    }   

    public void SetupBoard()
    {
        SetupTiles();
        SetupGamePieces();

        List<GamePiece> startingCollectibles = FindAllCollectibles();
        collectibleCount = startingCollectibles.Count;

        SetupCamera();
        FillBoard(fillYOffset, fillMoveTime);
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if(prefab == null) return;

        if (!IsWithinBounds(x, y)) return;

        GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
        tile.name = "Tile (" + x + "," + y + ")";

        m_allTiles[x, y] = tile.GetComponent<Tile>();

        tile.transform.parent = transform;
        m_allTiles[x, y].Init(x, y, this);
    }

    void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (prefab == null) return;

        if (!IsWithinBounds(x, y)) return;

        prefab.GetComponent<GamePiece>().Init(this);
        PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

        if (falseYOffset != 0)
        {
            prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
            prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
        }

        prefab.transform.parent = transform;        
    }

    GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if (prefab == null) return null;

        if (!IsWithinBounds(x, y)) return null;

        GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        bomb.GetComponent<Bomb>().Init(this);
        bomb.GetComponent<Bomb>().SetCoord(x, y);
        bomb.transform.parent = transform;
        return bomb;
    }

    public void MakeColorBombBooster(int x, int y)
    {
        if (!IsWithinBounds(x, y)) return;

        GamePiece pieceToReplace = m_allGamePieces[x, y];

        if (pieceToReplace == null) return;

        ClearPieceAt(x, y);
        GameObject bombObject = MakeBomb(boardSO.ColorBombPrefab, x, y);
        ActivateBomb(bombObject);        
    }

    void SetupTiles()
    {
        foreach (StartingObject startingTile in boardSO.StartingTiles)
        {
            if (startingTile == null) continue;
            
            MakeTile(startingTile.prefab, startingTile.x, startingTile.y, startingTile.z);
            
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(m_allTiles[x,y] == null)
                {
                    MakeTile(boardSO.TilePrefabNormal, x, y);
                }                
            }
        }
    }    

    void SetupGamePieces()
    {
        foreach (StartingObject sPiece in boardSO.StartingGamePieces)
        {
            if (sPiece == null) continue;

            GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity);
            MakeGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width-1) / 2f, (float)(height-1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)height / 2f + (float)boardSO.Bordersize;

        float horizontalSize = ((float)width / 2f + (float)boardSO.Bordersize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIndex = Random.Range(0, objectArray.Length);
        if(objectArray[randomIndex] == null)
        {
            Debug.LogWarning("BOARD.GetRandomObject: " + randomIndex + "does not contain a valid object!");
        }
        return objectArray[randomIndex];
    }

    GameObject GetRandomGamePiece()
    {
        return GetRandomObject(boardSO.GamePiecePrefabs);
    }

    GameObject GetRandomCollectible()
    {
        return GetRandomObject(boardSO.CollectiblePrefabs);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if(gamePiece == null)
        {
            Debug.LogWarning("Board:  Invalid GamePiece!");
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }       
        gamePiece.SetCoord(x, y);
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime=0.1f)
    {
        if (!IsWithinBounds(x, y)) return null;

        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
        MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
        return randomPiece.GetComponent<GamePiece>();
    }

    GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (!IsWithinBounds(x, y)) return null;

        GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
        MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);        
        return randomPiece.GetComponent<GamePiece>();
    }

    void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> unusedPieces = new Queue<GamePiece>(gamePieces);

        int maxIterations = 100;
        int iterations = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(m_allGamePieces[x,y] == null && m_allTiles[x,y].tileType != TileType.Obstacle)
                {
                    m_allGamePieces[x, y] = unusedPieces.Dequeue();

                    iterations = 0;

                    while (HasMatchOnFill(x, y))
                    {
                        unusedPieces.Enqueue(m_allGamePieces[x, y]);

                        m_allGamePieces[x, y] = unusedPieces.Dequeue();

                        iterations++;

                        if(iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int iterations = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] != null) continue;

                if (m_allTiles[x, y].tileType == TileType.Obstacle) continue;

                //GamePiece piece = null;
                    
                if(y==height-1 && CanAddCollectible())
                {
                    FillRandomCollectibleAt(x, y, falseYOffset, moveTime);
                    collectibleCount++;
                }
                else
                {
                    FillRandomGamePieceAt(x, y, falseYOffset, moveTime);
                    iterations = 0;

                    while (HasMatchOnFill(x, y))
                    {
                        ClearPieceAt(x, y);
                        FillRandomGamePieceAt(x, y, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            Debug.Log("Board.cs: FillBoard(): Break. could not place non-matching piece.");
                            break;
                        }
                    }
                }
                
            }
        }
    }   
    
    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    public void ClickTile(Tile tile)
    {
        if(m_clickedTile == null)
        {
            m_clickedTile = tile;            
        }
    }

    public void DragToTile(Tile tile)
    {
        if(m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if(m_clickedTile !=null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (!m_playerInputEnabled || GameManager.Instance.IsGameOver)
        {
            yield break;
        }

        GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

        if(targetPiece != null && clickedPiece != null)
        {
            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            List<GamePiece> colorMatches = ProcessColorBombs(clickedPiece, targetPiece);

            if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
            {
                clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            }
            else
            {
                yield return new WaitForSeconds(swapTime);

                #region drop bombs
                Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipeDirection, clickedPieceMatches);
                m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetPieceMatches);

                if (m_clickedTileBomb != null && targetPiece != null)
                {
                    GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                    if (!IsColorBomb(clickedBombPiece))
                    {
                        clickedBombPiece.ChangeColor(targetPiece);
                    }
                }

                if (m_targetTileBomb != null && clickedPiece != null)
                {
                    GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                    if (!IsColorBomb(targetBombPiece))
                    {
                        targetBombPiece.ChangeColor(clickedPiece);
                    }
                }
                #endregion

                List<GamePiece> piecesToClear = clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList();

                yield return StartCoroutine(ClearAndRefillBoardRoutine(piecesToClear));

                if (GameManager.Instance != null)
                {                    
                    GameManager.Instance.UpdateMoves();
                }
            }
        }
    }

    private List<GamePiece> ProcessColorBombs(GamePiece clickedPiece, GamePiece targetPiece,
                                                bool clearNonBlockers = false)
    {
        List<GamePiece> colorMatches = new List<GamePiece>();

        GamePiece colorBombPiece = null;
        GamePiece otherPiece = null;

        if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
        {
            colorBombPiece = clickedPiece;
            otherPiece = targetPiece;
        }
        else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
        {
            colorBombPiece = targetPiece;
            otherPiece = clickedPiece;
        }
        else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
        {
            foreach (GamePiece piece in m_allGamePieces)
            {
                if (piece == null) continue;

                if (!colorMatches.Contains(piece))
                {
                    colorMatches.Add(piece);
                }
            }
        }

        if(colorBombPiece != null)
        {
            colorBombPiece.matchValue = otherPiece.matchValue;
            colorMatches = FindAllMatchValue(otherPiece.matchValue);
        }

        if (!clearNonBlockers)
        {
            List<GamePiece> collectedAtBottom = FindAllCollectibles(true);

            if (collectedAtBottom.Contains(otherPiece))
            {
                return new List<GamePiece>();
            }
            else
            {
                foreach (GamePiece piece in collectedAtBottom)
                {
                    colorMatches.Remove(piece);
                }
            }
        }
        return colorMatches;
    }

    bool IsNextTo(Tile start, Tile end)
    {
        if(Mathf.Abs(start.xIndex - end.xIndex)==1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if(IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        if(startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue-1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY)) break;

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece == null) break;
            
            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }            
        }

        if (matches.Count >= minLength)
        {            
            return matches;
        }
        
        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if(upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<GamePiece> matches = FindMatchesAt(x, y);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType == TileType.Breakable) return;

        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

    }

    void HighlightTileOn(int x, int y, Color color)
    {
        if (m_allTiles[x, y].tileType == TileType.Breakable) return;

        SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                HighlightMatchesAt(x, y);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];

        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;
            
            ClearPieceAt(piece.xIndex, piece.yIndex);

            int bonus = 0;

            if(gamePieces.Count >= 4)
            {
                bonus = 20;
            }

            if(GameManager.Instance != null)
            {
                GameManager.Instance.ScorePoints(piece, m_scoreMultiplier, bonus);
                TimeBonus timeBonus = piece.GetComponent<TimeBonus>();

                if(timeBonus != null)
                {
                    GameManager.Instance.AddTime(timeBonus.bonusValue);
                }

                GameManager.Instance.UpdateCollectionGoals(piece);
            }

            //piece.ScorePoints(m_scoreMultiplier, bonus);

            if (m_particleManager != null)
            {
                if (bombedPieces.Contains(piece))
                {
                    m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
                }
                else
                {
                    m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                }                
            }            
        }
    }

    void ClearBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ClearPieceAt(x, y);

                if(m_particleManager != null)
                {
                    m_particleManager.ClearPieceFXAt(x, y);
                }
            }
        }
    }

    void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];
        
        if (tileToBreak == null) return;

        if (tileToBreak.tileType != TileType.Breakable) return;

        if(m_particleManager != null)
        {
            m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
        }

        tileToBreak.BreakTile();
    }

    void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;

            BreakTileAt(piece.xIndex, piece.yIndex);
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] != null) continue;

            if (m_allTiles[column, i].tileType == TileType.Obstacle) continue;
            
            for (int j = i+1; j < height; j++)
            {
                if (m_allGamePieces[column, j] == null) continue;
                
                m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                m_allGamePieces[column, i] = m_allGamePieces[column, j];
                m_allGamePieces[column, i].SetCoord(column, i);

                if (!movingPieces.Contains(m_allGamePieces[column, i]))
                {
                    movingPieces.Add(m_allGamePieces[column, i]);
                }

                m_allGamePieces[column, j] = null;

                break;
                
            }
                        
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<int> columnsToCollapse)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;

            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }

        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        
        m_playerInputEnabled = false;
        isRefilling = true;

        List<GamePiece> matches = gamePieces;

        m_scoreMultiplier = 0;

        do
        {
            m_scoreMultiplier++;

            // clear and collapse
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            //refill
            yield return StartCoroutine(RefillRoutine());
            
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.2f); //TODO: Magic number remove
        }
        while (matches.Count != 0);

        //deadlock Check
        if(m_boardDeadlock.IsDeadlocked(m_allGamePieces, 3))
        {
            yield return new WaitForSeconds(1f);
            //ClearBoard();
            yield return StartCoroutine(ShuffleBoardRoutine());

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(RefillRoutine());
        }

        m_playerInputEnabled = true;
        isRefilling = false;
    }

    public void ClearAndRefillBoard(int x, int y)
    {
        if (!IsWithinBounds(x, y)) return;

        GamePiece pieceToClear = m_allGamePieces[x, y];
        List<GamePiece> listOfOne = new List<GamePiece>();
        listOfOne.Add(pieceToClear);
        ClearAndRefillBoard(listOfOne);
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, fillMoveTime);
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.2f);  //TODO: Magic number remove

        bool isFinished = false;

        while (!isFinished)
        {
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            List<GamePiece> collectedPieces = FindCollectiblesAt(0, clearedAtBottomOnly:true);

            List<GamePiece> allCollectibles = FindAllCollectibles();
            List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();

            collectibleCount -= collectedPieces.Count;

            gamePieces = gamePieces.Union(collectedPieces).ToList();

            List<int> columnsToCollapse = GetColumns(gamePieces);

            ClearPieceAt(gamePieces, bombedPieces);
            BreakTileAt(gamePieces);

            if(m_clickedTileBomb != null)
            {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }

            if(m_targetTileBomb != null)
            {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f); //TODO: Magic number remove

            movingPieces = CollapseColumn(columnsToCollapse);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f); //TODO: Magic number remove

            matches = FindMatchesAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0, clearedAtBottomOnly:true);
            matches = matches.Union(collectedPieces).ToList();

            if(matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                m_scoreMultiplier++;
                if(SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBonusSound();
                }
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;
            
            if(piece.transform.position.y - (float) piece.yIndex > 0.001f)
            {
                return false;
            }

            if (piece.transform.position.x - (float)piece.xIndex > 0.001f)
            {
                return false;
            }

        }

        return true;
    }

    List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] == null) continue;
            gamePieces.Add(m_allGamePieces[i, row]);            
        }
        return gamePieces;
    }

    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] == null) continue;
            gamePieces.Add(m_allGamePieces[column, i]);
        }
        return gamePieces;
    }

    List<GamePiece> GetAdjacentGamePieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x-offset; i <= x+offset; i++)
        {
            for (int j = y-offset; j <= y+offset; j++)
            {
                if (!IsWithinBounds(i, j)) continue;

                gamePieces.Add(m_allGamePieces[i, j]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;

            List<GamePiece> piecesToClear = new List<GamePiece>();

            Bomb bomb = piece.GetComponent<Bomb>();

            if (bomb == null) continue;
            
            switch (bomb.bombType)
            {
                case BombType.Column:
                    piecesToClear = GetColumnPieces(bomb.xIndex);
                    break;
                case BombType.Row:
                    piecesToClear = GetRowPieces(bomb.yIndex);
                    break;
                case BombType.Adjacent:
                    piecesToClear = GetAdjacentGamePieces(bomb.xIndex, bomb.yIndex, 1);
                    break;
                case BombType.Color:

                    break;
            }

            allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
            allPiecesToClear = RemoveCollectibles(allPiecesToClear);
            
        }

        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;

            if(xStart == -1 || yStart == -1)
            {
                xStart = piece.xIndex;
                yStart = piece.yIndex;
                continue;
            }

            if(piece.xIndex != xStart && piece.yIndex == yStart)
            {
                horizontal = true;
            }

            if (piece.xIndex == xStart && piece.yIndex != yStart)
            {
                vertical = true;
            }
        }

        return (horizontal && vertical);
    }

    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        
        if (gamePieces == null) return null;
        
        MatchValue matchValue = FindMatchValue(gamePieces);
        
        if (gamePieces.Count < 4 || matchValue == MatchValue.None) return null;
        
        if (gamePieces.Count >= 5 && !IsCornerMatch(gamePieces))
        {
            if (boardSO.ColorBombPrefab == null) return null;

            return MakeBomb(boardSO.ColorBombPrefab, x, y);
        }
        else if (IsCornerMatch(gamePieces))
        {
            GameObject adjacentBomb = FindGamePieceByMatchValue(boardSO.AdjacentBombPrefabs, matchValue);
            if (adjacentBomb == null) return null;

            return MakeBomb(adjacentBomb, x, y);
        }
        else if(swapDirection.x != 0)
        {
            GameObject rowBomb = FindGamePieceByMatchValue(boardSO.RowBombPrefabs, matchValue);

            if (rowBomb == null) return null;            

            return MakeBomb(rowBomb, x, y);
        }
        else
        {
            GameObject columnBomb = FindGamePieceByMatchValue(boardSO.ColumnBombPrefabs, matchValue);

            if (columnBomb == null) return null;            

            return MakeBomb(columnBomb, x, y);
        }        
    }

    void ActivateBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue mValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] == null) continue;

                if(m_allGamePieces[x,y].matchValue == mValue)
                {
                    foundPieces.Add(m_allGamePieces[x, y]);
                }                
            }
        }
        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if (bomb == null) return false;

        return (bomb.bombType == BombType.Color);
    }

    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int x = 0; x < width; x++)
        {
            if (m_allGamePieces[x, row] == null) continue;

            Collectible collectibleComponent = m_allGamePieces[x, row].GetComponent<Collectible>();

            if (collectibleComponent == null) continue;

            if (clearedAtBottomOnly && !collectibleComponent.clearedAtBottom) continue;

            foundCollectibles.Add(m_allGamePieces[x,row]);            
        }
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles(bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int y = 0; y < height; y++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(y, clearedAtBottomOnly);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }

        return foundCollectibles;
    }

    bool CanAddCollectible()
    {
        return (Random.value <= boardSO.ChanceForCollectible 
            && boardSO.CollectiblePrefabs.Length > 0 
            && collectibleCount < boardSO.MaxCollectibles);
    }

    List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces)
    {
        List<GamePiece> collectiblePieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (GamePiece piece in collectiblePieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();

            if (collectibleComponent == null) continue;

            if(collectibleComponent.clearedByBomb) continue;

            piecesToRemove.Add(piece);
        }

        return bombedPieces.Except(piecesToRemove).ToList();
    }

    MatchValue FindMatchValue(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece == null) continue;
            return piece.matchValue;
        }
        return MatchValue.None;
    }

    GameObject FindGamePieceByMatchValue(GameObject[] gamePiecePrefabs, MatchValue matchValue)
    {
        if (matchValue == MatchValue.None) return null;

        foreach (GameObject go in gamePiecePrefabs)
        {
            GamePiece piece = go.GetComponent<GamePiece>();

            if (piece == null) return null;

            if (piece.matchValue == matchValue)
            {
                return go;
            }
        }

        return null;
    }

    public void TestDeadlock()
    {
        m_boardDeadlock.IsDeadlocked(m_allGamePieces, 3);
    }

    public void ShuffleBoard()
    {
        if (!m_playerInputEnabled) return;

        StartCoroutine(ShuffleBoardRoutine());
    }

    IEnumerator ShuffleBoardRoutine()
    {
        List<GamePiece> allPieces = new List<GamePiece>();
        foreach (GamePiece piece in m_allGamePieces)
        {
            allPieces.Add(piece);
        }

        while (!IsCollapsed(allPieces))
        {
            yield return null;
        }

        List<GamePiece> normalPieces = m_boardShuffler.RemoveNormalPieces(m_allGamePieces);

        m_boardShuffler.ShuffleList(normalPieces);

        FillBoardFromList(normalPieces);

        m_boardShuffler.MovePieces(m_allGamePieces, swapTime);

        List<GamePiece> matches = FindAllMatches();
        StartCoroutine(ClearAndRefillBoardRoutine(matches));
    }

    

}

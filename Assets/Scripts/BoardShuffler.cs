using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardShuffler : MonoBehaviour
{
    public List<GamePiece> RemoveNormalPieces(GamePiece[,] allPieces)
    {
        List<GamePiece> normalPieces = new List<GamePiece>();

        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allPieces[x, y] == null) continue;

                Bomb bomb = allPieces[x, y].GetComponent<Bomb>();
                Collectible collectible = allPieces[x, y].GetComponent<Collectible>();

                if (bomb != null || collectible != null) continue;

                normalPieces.Add(allPieces[x, y]);
                allPieces[x, y] = null;
            }
        }

        return normalPieces;
    }

    public void ShuffleList(List<GamePiece> piecesToShuffle)
    {
        int maxCount = piecesToShuffle.Count;

        for (int i = 0; i < maxCount; i++)
        {
            int r = Random.Range(i, maxCount);

            if (r == i) continue;

            GamePiece temp = piecesToShuffle[r];
            piecesToShuffle[r] = piecesToShuffle[i];
            piecesToShuffle[i] = temp;
        }
    }

    public void MovePieces(GamePiece[,] allPieces, float swapTime = 0.5f)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allPieces[x, y] == null) continue;

                allPieces[x, y].Move(x, y, swapTime);
            }
        }
    }
}

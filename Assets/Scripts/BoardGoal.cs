using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardCounter
{
    Timer,
    Moves
}

[System.Serializable]
public class PieceToCollect
{
    public GamePiece pieceToCollect;
    [Range(0, 50)]
    public int numberToCollect;       

    public void CollectPiece(GamePiece pieceToCheck)
    {
        if (pieceToCheck == null) return;        

        if (pieceToCollect.SpriteRenderer.sprite == pieceToCheck.SpriteRenderer.sprite 
            && pieceToCollect.matchValue == pieceToCheck.matchValue)
        {
            numberToCollect--;
            numberToCollect = Mathf.Clamp(numberToCollect, 0, numberToCollect);
        }
    }
}

public class BoardGoal : MonoBehaviour
{
    int scoredStars = 0;
    public int ScoredStars { get => scoredStars; }
    
    BoardSO m_boardSO;
    int[] m_scoreGoals = new int[3] { 1000, 2000, 3000 };
    public int[] ScoreGoals { get => m_scoreGoals; }

    int m_movesLeft = 30;
    public int MovesLeft { get => m_movesLeft; set => m_movesLeft = value; }

    int m_timeLeft = 60;
    public int TimeLeft { get => m_timeLeft; }

    int m_maxTime;
    BoardCounter m_boardCounter = BoardCounter.Moves;
    public BoardCounter BoardCounter { get => m_boardCounter; }

    PieceToCollect[] m_piecesToCollect;
    public PieceToCollect[] PiecesToCollect { get => m_piecesToCollect; }

    bool m_hasPiecesToCollect = false;
    public bool HasPiecesToCollect { get => m_hasPiecesToCollect; }

    void Awake()
    {        
        m_boardSO = GameObject.FindWithTag("Board").GetComponent<Board>().BoardSO;
        if(m_boardSO != null)
        {
            m_scoreGoals = m_boardSO.scoreGoals;
            m_movesLeft = m_boardSO.startingMoves;
            m_timeLeft = m_boardSO.startingTime;
            m_boardCounter = m_boardSO.boardCounter;
            m_piecesToCollect = m_boardSO.piecesToCollect;
        }

        if(m_piecesToCollect != null && m_piecesToCollect.Length > 0)
        {
            m_hasPiecesToCollect = true;
        }
        
        if(m_boardCounter == BoardCounter.Timer)
        {
            m_maxTime = m_timeLeft;

            if (UIManager.Instance != null && UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.InitTimer(m_timeLeft);
            }
        }
        Init();
    }

    void Init()
    {
        scoredStars = 0;
        for (int i = 1; i < m_scoreGoals.Length; i++)
        {
            if (m_scoreGoals[i] < m_scoreGoals[i - 1])
            {
                Debug.LogWarning("LEVELGOAL Setup score goals in increasing order!");
            }
        }
    }

    public void UpdateGoals(GamePiece pieceToCheck)
    {
        if (pieceToCheck == null) return;

        foreach (PieceToCollect piece in m_piecesToCollect)
        {
            if (piece == null) continue;

            piece.CollectPiece(pieceToCheck);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCollectionGoalLayout();
        }
    }

    int UpdateScore(int score)
    {
        for (int i = 0; i < m_scoreGoals.Length; i++)
        {
            if (score < m_scoreGoals[i])
            {
                return i;
            }
        }
        return m_scoreGoals.Length;
    }

    public void UpdateScoreStars(int score)
    {
        scoredStars = UpdateScore(score);
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        while (m_timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            m_timeLeft--;

            if (UIManager.Instance != null && UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.UpdateTimer(m_timeLeft);
            }
        }
    }
    public void AddTime(int timeValue)
    {
        m_timeLeft += timeValue;
        m_timeLeft = Mathf.Clamp(m_timeLeft, 0, m_maxTime);

        if (UIManager.Instance != null && UIManager.Instance.timer != null)
        {
            UIManager.Instance.timer.UpdateTimer(m_timeLeft);
        }
    }

    bool AreAllPiecesCollected(PieceToCollect[] piecesToCollect)
    {
        if (!m_hasPiecesToCollect) return true;

        foreach (PieceToCollect piece in piecesToCollect)
        {
            if (piece == null || piecesToCollect == null) return false;

            if (piece.numberToCollect != 0)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsGameOver()
    {
        if (AreAllPiecesCollected(m_piecesToCollect) && ScoreManager.Instance != null)
        {
            int maxScore = m_scoreGoals[m_scoreGoals.Length - 1];
            if (ScoreManager.Instance.CurrentScore >= maxScore) return true;
        }

        if (m_boardCounter == BoardCounter.Timer)
        {
            return (m_timeLeft <= 0);
        }
        else
        {
            return (m_movesLeft <= 0);
        }
    }

    public bool IsWinner()
    {
        if (ScoreManager.Instance == null) return false;

        return (ScoreManager.Instance.CurrentScore >= m_scoreGoals[0] && AreAllPiecesCollected(m_piecesToCollect));
    }
}

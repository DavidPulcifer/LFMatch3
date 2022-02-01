using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalCollected : LevelGoal
{
    public CollectionGoal[] collectionGoals;    

    public void UpdateGoals(GamePiece pieceToCheck)
    {
        if (pieceToCheck == null) return;

        foreach (CollectionGoal goal in collectionGoals)
        {
            if (goal == null) continue;

            goal.CollectPiece(pieceToCheck);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCollectionGoalLayout();
        }
    }

    bool AreGoalsComplete(CollectionGoal[] goals)
    {
        foreach (CollectionGoal goal in goals)
        {
            if (goal == null || goals == null) return false;

            if(goal.numberToCollect != 0)
            {
                return false;
            }
        }

        return true;
    }

    public override bool IsGameOver()
    {
        if (AreGoalsComplete(collectionGoals) && ScoreManager.Instance != null)
        {
            int maxScore = scoreGoals[scoreGoals.Length - 1];
            if (ScoreManager.Instance.CurrentScore >= maxScore) return true;
        }

        if(levelCounter == LevelCounter.Timer)
        {
            return (timeLeft <= 0);
        }
        else
        {
            return (movesLeft <= 0);
        }        
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Instance == null) return false;

        return (ScoreManager.Instance.CurrentScore >= scoreGoals[0] && AreGoalsComplete(collectionGoals));
    }
}

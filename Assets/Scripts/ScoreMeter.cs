using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ScoreMeter : MonoBehaviour
{
    public Slider slider;
    public ScoreStar[] scoreStars = new ScoreStar[3];
    BoardGoal m_boardGoal;
    int m_maxScore;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetupStars(BoardGoal boardGoal)
    {
        if(boardGoal == null)
        {
            Debug.LogWarning("SCOREMETER Invalid level goal.");
            return;
        }

        m_boardGoal = boardGoal;

        m_maxScore = m_boardGoal.ScoreGoals[m_boardGoal.ScoreGoals.Length-1];

        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;

        if (m_maxScore <= 0) return;

        for (int i = 0; i < boardGoal.ScoreGoals.Length; i++)
        {
            if (scoreStars[i] == null) continue;
            float newX = (sliderWidth * boardGoal.ScoreGoals[i] / m_maxScore) - (sliderWidth * 0.5f);
            RectTransform starRectXform = scoreStars[i].GetComponent<RectTransform>();
            if (starRectXform == null) continue;
            starRectXform.anchoredPosition = new Vector2(newX, starRectXform.anchoredPosition.y);
        }
    }

    public void UpdateScoreMeter(int score, int starCount)
    {
        if(m_boardGoal != null)
        {
            slider.value = (float)score / (float)m_maxScore;
        }

        for (int i = 0; i < starCount; i++)
        {
            if (scoreStars[i] == null) continue;

            scoreStars[i].Activate();
        }
    }
}

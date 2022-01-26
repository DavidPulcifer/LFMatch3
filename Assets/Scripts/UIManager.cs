using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public GameObject collectionGoalLayout;
    public int collectionGoalBaseWidth = 125;
    CollectionGoalPanel[] m_collectionGoalPanels;

    public ScreenFader screenFader;
    public Text levelNameText;
    public Text movesLeftText;
    public ScoreMeter scoreMeter;
    public MessageWindow messageWindow;

    public GameObject movesCounter;

    public Timer timer;

    public override void Awake()
    {
        base.Awake();

        if(messageWindow != null)
        {
            messageWindow.gameObject.SetActive(true);
        }

        if(screenFader != null)
        {
            screenFader.gameObject.SetActive(true);
        }
    }

    public void SetupCollectionGoalLayout(PieceToCollect[] piecesToCollect, GameObject goalLayout, int spacingWidth)
    {
        if (goalLayout == null || piecesToCollect == null || piecesToCollect.Length == 0) return;

        RectTransform rectXform = goalLayout.GetComponent<RectTransform>();
        rectXform.sizeDelta = new Vector2(piecesToCollect.Length * spacingWidth,
                                                    rectXform.sizeDelta.y);

        CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();

        for (int i = 0; i < panels.Length; i++)
        {
            if (i < piecesToCollect.Length && piecesToCollect[i] != null)
            {
                panels[i].gameObject.SetActive(true);
                panels[i].PieceToCollect = piecesToCollect[i];
                panels[i].SetupPanel();
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }            
        }
    }

    public void SetupCollectionGoalLayout(PieceToCollect[] piecesToCollect)
    {
        SetupCollectionGoalLayout(piecesToCollect, collectionGoalLayout, collectionGoalBaseWidth);
    }

    public void UpdateCollectionGoalLayout(GameObject goalLayout)
    {
        if (goalLayout == null) return;

        CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();

        if (panels == null || panels.Length == 0) return;

        foreach (CollectionGoalPanel panel in panels)
        {
            if (panel != null && panel.isActiveAndEnabled)
            {
                panel.UpdatePanel();
            }
        }
    }    
    
    public void UpdateCollectionGoalLayout()
    {
        UpdateCollectionGoalLayout(collectionGoalLayout);
    }

    public void EnableTimer(bool state)
    {
        if (timer != null)
        {
            timer.gameObject.SetActive(state);
        }
    }

    public void EnableMovesCounter(bool state)
    {
        if(movesCounter != null)
        {
            movesCounter.SetActive(state);
        }
    }

    public void EnableCollectionGoalLayout(bool state)
    {
        if(collectionGoalLayout != null)
        {
            collectionGoalLayout.SetActive(state);
        }
    }
}

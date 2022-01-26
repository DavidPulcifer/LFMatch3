using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionGoalPanel : MonoBehaviour
{
    public Text numberLeftText;
    public Image prefabImage;

    PieceToCollect m_pieceToCollect;
    public PieceToCollect PieceToCollect { get => m_pieceToCollect; set => m_pieceToCollect = value; }

    BoardGoal m_boardGoal;

    // Start is called before the first frame update
    void Start()
    {
        SetupPanel();
    }

    public void SetupPanel()
    {
        if (m_pieceToCollect == null || numberLeftText == null || prefabImage == null) return;

        SpriteRenderer pieceSprite = m_pieceToCollect.pieceToCollect.SpriteRenderer;

        if(pieceSprite != null)
        {
            prefabImage.sprite = pieceSprite.sprite;
            prefabImage.color = pieceSprite.color;
        }        
        numberLeftText.text = m_pieceToCollect.numberToCollect.ToString();
    }

    public void UpdatePanel()
    {
        if (m_pieceToCollect == null || numberLeftText == null) return;

        numberLeftText.text = m_pieceToCollect.numberToCollect.ToString();
    }
}

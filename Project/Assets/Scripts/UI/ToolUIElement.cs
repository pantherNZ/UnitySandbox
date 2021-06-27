using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolUIElement : MonoBehaviour
{
    [SerializeField] private Text toolName;
    [SerializeField] private Image displayImage;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Color highlightColour;
    private bool isHighlighted;

    public void SetData( string text, Sprite image )
    {
        toolName.text = text;
        displayImage.sprite = image;
    }

    public void SetIsHighlighted( bool highlight )
    {
        isHighlighted = highlight;
        highlightImage.color = highlight ? highlightColour : new Color( 1.0f, 1.0f, 1.0f, 0.0f );
    }

    private void Start()
    {
        SetIsHighlighted( isHighlighted );
    }
}

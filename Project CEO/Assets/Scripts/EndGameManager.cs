using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    public Image background;
    public Sprite goodEndingSprite;
    public Sprite badEnding;
    public TextMeshProUGUI endText;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateBackground(UnityToGemini.Instance.isGoodEnding);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBackground(bool goodEnding)
    {
        if (goodEnding)
        {
            background.sprite = goodEndingSprite;
            endText.text = "Great job! Want to try again?";
        }
        else
        {
            background.sprite = badEnding;
            endText.text = "Tough luck. Want to try again?";

        }
    }
}

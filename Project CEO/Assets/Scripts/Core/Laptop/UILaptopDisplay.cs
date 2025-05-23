using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILaptopDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public Transform scrollViewContent;
    public Transform scrollViewVisitor;
    public GameObject playerBubblePrefab;
    public GameObject journalistBubblePrefab;
    public GameObject visitorNameTagPrefab;
    public Button sendButton;
    public Button endInterviewButton;


    private void OnEnable()
    {
        NewsGenerator.onNewsGenerated += LaptopOnNewsGenerated;
        InputManager.OnEnterKeyDownLate += AddPlayerDialog;
    }

    private void OnDisable()
    {
        NewsGenerator.onNewsGenerated -= LaptopOnNewsGenerated;
        InputManager.OnEnterKeyDownLate -= AddPlayerDialog;
    }

    private void Start()
    {
        // Register the button click listener
        sendButton.onClick.AddListener(AddPlayerDialog);
        LaptopUIInit();
    }

    public void LaptopUIInit()
    {
        ClearVisitor();
        ClearDialog();
    }
    

    private void AddVisitor()
    {
        // Instantiate dialog prefab
        GameObject journalistNameTag = Instantiate(visitorNameTagPrefab, scrollViewVisitor);

        TMP_Text dialogText = journalistNameTag.GetComponentInChildren<TMP_Text>();
        if (dialogText != null)
        {
            dialogText.text = RandomNameGenerator.GenerateRandomName();
        }
    }

    private void ClearVisitor()
    {
        foreach (Transform child in scrollViewVisitor.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearDialog()
    {
        foreach (Transform child in scrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddJournalistDialog(string question)
    {

        // Instantiate dialog prefab
        GameObject journalistDialog = Instantiate(journalistBubblePrefab, scrollViewContent);

        // Set the dialog text
        TMP_Text dialogText = journalistDialog.GetComponentInChildren<TMP_Text>();
        if (dialogText != null)
        {
            dialogText.text = question;
        }

        // Optional: clear input field after sending
        inputField.text = "";
    }

    public void LaptopOnNewsGenerated(string news)
    {
        ClearDialog();
        ClearVisitor();
        sendButton.enabled = true;
        inputField.ActivateInputField();
        inputField.enabled = true;
        GameplayManager.canPlayerSend = true;
        string question = GenerateRandomQuestion(news);
        AddVisitor();
        AddJournalistDialog(question);
    }

    private void AddPlayerDialog()
    {

        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) 
            return;

        // Instantiate dialog prefab
        GameObject playerDialog = Instantiate(playerBubblePrefab, scrollViewContent);

        // Set the dialog text
        TMP_Text dialogText = playerDialog.GetComponentInChildren<TMP_Text>();
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        //clear input field after sending
        inputField.text = "";
        inputField.DeactivateInputField();
        inputField.enabled = false;

        sendButton.enabled = false;
    }

    //Random Question Generation
    private static readonly List<string> questionTemplates = new List<string>
    {
        "What's your take on [XXX]?",
        "How do you view the recent developments around [XXX]?",
        "What's your perspective on the situation with [XXX]?",
        "Where do you stand on [XXX]?",
        "How do you interpret the news about [XXX]?",
        "Do you see [XXX] as a threat or an opportunity?",
        "What implications do you think [XXX] might have for the industry?",
        "How are you personally reacting to the news around [XXX]?",
        "What message would you share in response to [XXX]?",
        "What concerns, if any, do you have about [XXX]?"
    };

    public static string GenerateRandomQuestion(string newsTitle)
    {
        int randomIndex = UnityEngine.Random.Range(0, questionTemplates.Count);
        string selectedTemplate = questionTemplates[randomIndex];
        return selectedTemplate.Replace("[XXX]", newsTitle);
    }
}



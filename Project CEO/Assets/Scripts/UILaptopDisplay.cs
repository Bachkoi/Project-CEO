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

    private void OnEnable()
    {
        NewsGenerator.onNewsGenerated += LaptopOnNewsGenerated;
        InputManager.OnEnterKeyDown += AddPlayerDialog;
    }

    private void OnDisable()
    {
        NewsGenerator.onNewsGenerated -= LaptopOnNewsGenerated;
        InputManager.OnEnterKeyDown -= AddPlayerDialog;
    }

    private void Start()
    {
        // Register the button click listener
        sendButton.onClick.AddListener(AddPlayerDialog);
        LaptopUIInit();
    }

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

    public void LaptopUIInit()
    {
        ClearVisitor();
        AddVisitor();
        ClearDialog();
    }

    public static string GenerateRandomQuestion(string newsTitle)
    {
        int randomIndex = UnityEngine.Random.Range(0, questionTemplates.Count);
        string selectedTemplate = questionTemplates[randomIndex];
        return selectedTemplate.Replace("[XXX]", newsTitle);
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

        //Set panel visible (For some reasons IDK they were set to inactive)
        //UIUtilities.SetActiveStatusAll(journalistNameTag,true);
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

        sendButton.enabled = false;
    }
}

public class RandomNameGenerator
{
    private static readonly System.Random random = new System.Random();

    private static readonly List<string> lastNames = new List<string>
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Miller", "Davis", "Garcia", "Rodriguez", "Wilson"
    };

    private static readonly List<string> maleFirstNames = new List<string>
    {
        "James", "John", "Robert", "Michael", "William",
        "David", "Richard", "Joseph", "Thomas", "Daniel"
    };

    private static readonly List<string> femaleFirstNames = new List<string>
    {
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth",
        "Barbara", "Susan", "Jessica", "Sarah", "Karen"
    };

    public static string GenerateRandomName(int gender = 0)
    {
        string lastName = lastNames[random.Next(lastNames.Count)];

        string firstName;
        int actualGender = gender;

        if (actualGender == 0)
        {
            actualGender = random.Next(1, 3); // 1 or 2
        }

        if (actualGender == 1)
        {
            firstName = maleFirstNames[random.Next(maleFirstNames.Count)];
        }
        else
        {
            firstName = femaleFirstNames[random.Next(femaleFirstNames.Count)];
        }

        return $"{firstName} {lastName}";
    }
}

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
    public GameObject playerBubblePrefab;
    public GameObject journalistBubblePrefab;
    public Button sendButton;

    private void Start()
    {
        // Register the button click listener
        sendButton.onClick.AddListener(AddPlayerDialog);
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

        // Optional: clear input field after sending
        inputField.text = "";
    }
}

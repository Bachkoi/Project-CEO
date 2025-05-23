using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] protected TMP_InputField apiKeyField;
    [SerializeField] protected TMP_InputField companyNameField;
    [SerializeField] protected TMP_InputField companyDescriptionField;
    [SerializeField] protected TMP_InputField companyAcronymField;
    [SerializeField] protected Button verifyBtn;
    [SerializeField] protected Button startBtn;
    [SerializeField] protected TextMeshProUGUI statusText;

    [SerializeField] protected string gameStateName;
    
    private bool isValidating = false;
    
    void Start()
    {
        // Initialize UI components
        if (startBtn != null)
            startBtn.interactable = false;
        
        if (verifyBtn != null)
            verifyBtn.onClick.AddListener(VerifyApiKey);
        
        if (apiKeyField != null)
            apiKeyField.onEndEdit.AddListener(OnApiKeyInput);
        
        // Display status message if component exists
        UpdateStatusText("Please enter your Gemini API key and company information to continue.");
        
        startBtn.onClick.AddListener(GoToL1);
        
    }
    
    void OnEnable()
    {
        // Subscribe to UnityToGemini validation results
        UnityToGemini.GeminiResponseCallback += OnGeminiValidationResponse;
    }
    
    void OnDisable()
    {
        // Unsubscribe from UnityToGemini validation results
        UnityToGemini.GeminiResponseCallback -= OnGeminiValidationResponse;
    }
    
    private void OnApiKeyInput(string apiKey)
    {
        // Enable the verify button if there's text in the input field
        if (verifyBtn != null)
            verifyBtn.interactable = !string.IsNullOrWhiteSpace(apiKey);
    }
    
    private void VerifyApiKey()
    {
        // Validate API key
        if (apiKeyField == null || string.IsNullOrWhiteSpace(apiKeyField.text))
        {
            UpdateStatusText("Please enter a valid API key.");
            return;
        }
        
        // Validate company name
        if (companyNameField == null || string.IsNullOrWhiteSpace(companyNameField.text))
        {
            UpdateStatusText("Please enter a company name.");
            return;
        }
        
        // Validate company description
        if (companyDescriptionField == null || string.IsNullOrWhiteSpace(companyDescriptionField.text))
        {
            UpdateStatusText("Please enter a company description.");
            return;
        }
        
        // Validate company acronym
        if (companyAcronymField == null || string.IsNullOrWhiteSpace(companyAcronymField.text) || companyAcronymField.text.Trim().Length > 4)
        {
            UpdateStatusText("Please enter a company acronym, no longer than 4 characters.");
            return;
        }
        
        // Disable UI while validating
        isValidating = true;
        verifyBtn.interactable = false;
        apiKeyField.interactable = false;
        companyNameField.interactable = false;
        companyDescriptionField.interactable = false;
        companyAcronymField.interactable = false;
        UpdateStatusText("Validating API key...");
        
        // Send validation request
        StartCoroutine(UnityToGemini.Instance.SendKeyValidationToGemini(apiKeyField.text));
    }
    
    private void OnGeminiValidationResponse(string response, GeminiRequestType type)
    {
        // Only process the response if we're currently validating
        if (!isValidating)
            return;
        
        isValidating = false;
        
        // Check if this is an error response
        if (response.StartsWith("ERROR:"))
        {
            string errorMessage = response.Substring(6); // Remove "ERROR:" prefix
            Debug.LogError($"API key validation failed: {errorMessage}");
            
            UpdateStatusText("Invalid API key. Please try again.");
            
            // Re-enable input for trying again
            if (apiKeyField != null)
                apiKeyField.interactable = true;
            
            if (companyNameField != null)
                companyNameField.interactable = true;
                
            if (companyDescriptionField != null)
                companyDescriptionField.interactable = true;
            
            if (companyAcronymField != null)
                companyAcronymField.interactable = true;
            
            if (verifyBtn != null && !string.IsNullOrWhiteSpace(apiKeyField.text))
                verifyBtn.interactable = true;
                
            return;
        }
        
        try
        {
            // If we received a response without errors, validation was successful
            Debug.Log("API key validation successful");
            
            UpdateStatusText("API key validated successfully!");
            
            // Store the API key in UnityToGemini for future use
            UnityToGemini.Instance.apiKey = apiKeyField.text.Trim();
            
            // Store company information in UnityToGemini
            UnityToGemini.Instance.companyName = companyNameField.text.Trim();
            UnityToGemini.Instance.companyDescription = companyDescriptionField.text.Trim();
            UnityToGemini.Instance.companyAcronym = companyAcronymField.text.Trim();

            Debug.Log($"Company info saved - Name: {UnityToGemini.Instance.companyName}, Description: {UnityToGemini.Instance.companyDescription}, Acronym {UnityToGemini.Instance.companyAcronym}");
            
            // Enable the start button
            if (startBtn != null)
                startBtn.interactable = true;
        }
        catch (System.Exception e)
        {
            // If there was an error parsing the response, validation failed
            Debug.LogError($"Error validating API key: {e.Message}");
            
            UpdateStatusText("Invalid API key. Please try again.");
            
            // Re-enable input for trying again
            if (apiKeyField != null)
                apiKeyField.interactable = true;
            
            if (verifyBtn != null && !string.IsNullOrWhiteSpace(apiKeyField.text))
                verifyBtn.interactable = true;
        }
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
        else
            Debug.Log(message);
    }

    public void GoToL1()
    {
        SceneManager.LoadScene(gameStateName);
    }

    public void GoToEnd()
    {
        SceneManager.LoadScene("EndState");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MenuState");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

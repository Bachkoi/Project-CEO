using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using Backend;

public class NewsGenerator : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI newsTitleText;
    
    [SerializeField]
    private float scrollSpeed = 150f; // Scrolling speed
    
    [SerializeField]
    private float resetPositionX = -1100f; // Reset position X coordinate (left side of screen)
    
    [SerializeField]
    private float startPositionX = 1100f; // Start position X coordinate (right side of screen)
    
    // Control variables
    private bool hasStartedMoving = false;
    private bool updatePending = false;
    private List<string> pendingNewsText = new List<string>();
    private bool isScrollingActive = false;
    private int currentNewsIndex = 0;
    [SerializeField] private float newsCooldownTime = 10f;
    private bool isCoolingDown = false;
    
    [SerializeField]
    private Color debugLineColor = Color.blue; // Debug line color
    
    [SerializeField]
    private bool showDebugLines = true; // Toggle to show debug lines

    [SerializeField] protected GameObject breakingNewsContainer;

    [SerializeField, BoxGroup("Prompt"), TextArea(7, 7)] private string newsPrompt;
    [SerializeField, BoxGroup("Prompt"), TextArea(7, 7)] private string updatedNewsPrompt;
    [SerializeField, BoxGroup("Prompt"), ReadOnly] private List<string> generatedActions = new List<string>();
    
    [SerializeField, ReadOnly] private PanelManager panelManager;
    
    public string UpdatedNewsPrompt { get => updatedNewsPrompt; set => updatedNewsPrompt = value; }

    public static Action<string> onNewsGenerated;

    private void OnEnable()
    {
       UnityToGemini.GeminiResponseCallback += UnpackNewsResponse;
        //PanelManager.switchPanel += OnSwitchPanel;
        CameraManager.onChangeCamera += OnChangeCamera;
        GameplayManager.onPublicReact += OnPublicReact;
        TimeManager.onDayChange += OnDayChange;
    }
    
    private void OnDisable()
    {
        UnityToGemini.GeminiResponseCallback -= UnpackNewsResponse;
        //PanelManager.switchPanel -= OnSwitchPanel;
        CameraManager.onChangeCamera -= OnChangeCamera;
        GameplayManager.onPublicReact -= OnPublicReact;
        TimeManager.onDayChange -= OnDayChange;
    }
    
    /// <summary>
    /// Handles day change events to generate initial news for the new day
    /// </summary>
    /// <param name="day">The new day number</param>
    private void OnDayChange(int day)
    {
        GenerateInitialDailyNews();
    }
    
    /// <summary>
    /// Generates initial daily news when a new day starts
    /// </summary>
    private void GenerateInitialDailyNews()
    {
        Debug.Log("Generating initial news for new day");
        UpdatePrompt();
        UnityToGemini.Instance.SendRequest(updatedNewsPrompt, GeminiRequestType.News);
    }

    void Start()
    {
        //panelManager = FindObjectOfType<PanelManager>();
        if (newsTitleText != null)
        {
            // Force TextMeshPro to update its layout
            Canvas.ForceUpdateCanvases();
            newsTitleText.ForceMeshUpdate();
            
            // Ensure the text has proper sizing by refreshing it
            if (!string.IsNullOrEmpty(newsTitleText.text))
            {
                newsTitleText.text = newsTitleText.text;
                
                // Only start scrolling if there's actual content
                StartScrollingIfNeeded();
            }
        }
        else
        {
            Debug.LogError("newsTitleText is not assigned!");
        }
            }
            
            /// <summary>
            /// Starts the scrolling coroutine if the text is not empty and scrolling is not already active
            /// </summary>
            private void StartScrollingIfNeeded()
            {
        if (newsTitleText != null && !string.IsNullOrWhiteSpace(newsTitleText.text) && !isScrollingActive)
        {
            StartCoroutine(ScrollTitleText());
            Debug.Log("Started scrolling news title: " + newsTitleText.text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (showDebugLines && newsTitleText != null)
        {
            // Draw debug lines at runtime
            DrawDebugLines();
        }

        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    UpdatePrompt();
        //    
        //    UnityToGemini.Instance.SendNewsRequest(updatedNewsPrompt);
        //}
    }

    public void UpdatePrompt()
    {
        if (!string.IsNullOrEmpty(newsPrompt))
        {
            updatedNewsPrompt = newsPrompt;

            if (updatedNewsPrompt.Contains("{0}"))
            {
                updatedNewsPrompt = updatedNewsPrompt.Replace("{0}", UnityToGemini.Instance.companyName);
            }
            if (updatedNewsPrompt.Contains("{1}"))
            {
                updatedNewsPrompt = updatedNewsPrompt.Replace("{1}", UnityToGemini.Instance.companyDescription);
            }
            if (updatedNewsPrompt.Contains("{2}"))
            {
                string replacingString = "";
                if (generatedActions.Count > 0)
                {
                    replacingString += "You have given me these actions already: ";
                    for (int  i = 0;  i < generatedActions.Count;  i++)
                    {
                        replacingString += generatedActions[i];
                        if (i < generatedActions.Count - 1)
                            replacingString += ", ";
                        else
                            replacingString += ". ";
                    }
                }
                updatedNewsPrompt = updatedNewsPrompt.Replace("{2}", replacingString);
            }
        }
    }
    
    private IEnumerator ScrollTitleText()
    {
        // Don't start scrolling if there's no text
        if (newsTitleText == null || string.IsNullOrWhiteSpace(newsTitleText.text))
        {
            isScrollingActive = false;
            yield break;
        }
        
        isScrollingActive = true;
        RectTransform textRectTransform = newsTitleText.rectTransform;
        
        // Wait for a couple of frames to ensure everything is initialized
        yield return null;
        yield return null;
        
        // Main news display loop
        while (true)
        {
            // Check if we need to update news from pending list
            if (updatePending && pendingNewsText.Count > 0)
            {
                // Display one news item at a time
                currentNewsIndex = 0;
                StartCoroutine(DisplayNewsSequentially());
                
                // Reset state after starting the sequential display
                updatePending = false;
            }
            
            // Continue the standard scrolling for current visible text
            if (!string.IsNullOrEmpty(newsTitleText.text))
            {
                yield return ScrollSingleNews(textRectTransform);
            }
            else
            {
                // If no text, just wait a frame
                yield return null;
            }
        }
    }
    
    private IEnumerator DisplayNewsSequentially()
    {
        // Get a copy of the current pending news for display
        List<string> currentBatch = new List<string>(pendingNewsText);
        pendingNewsText.Clear();
        
        // Process each news item in the list with cooldown between them
        currentNewsIndex = 0;
        while (currentNewsIndex < currentBatch.Count)
        {
            // Check if we have been interrupted by a priority reaction
            if (pendingNewsText.Count > 0)
            {
                // Save remaining news from current batch to display later
                for (int i = currentNewsIndex; i < currentBatch.Count; i++)
                {
                    pendingNewsText.Add(currentBatch[i]);
                }
                
                Debug.Log($"News sequence interrupted. {currentBatch.Count - currentNewsIndex} items requeued.");
                
                // Start a new sequence to show priority news
                currentNewsIndex = 0;
                updatePending = true;
                break;
            }
            
            // Show breaking news animation for the current item
            StartCoroutine(ShowBreakingNews(breakingNewsContainer.transform));
            
            // Display the current news item
            string currentNews = currentBatch[currentNewsIndex];
            newsTitleText.text = currentNews;
            newsTitleText.ForceMeshUpdate();
            
            // Notify listeners about the new news item
            onNewsGenerated?.Invoke(currentNews);
            
            Debug.Log($"Displaying news {currentNewsIndex + 1}/{currentBatch.Count}: '{currentNews}'");
            
            // Move to the next item
            currentNewsIndex++;
            
            // Wait for cooldown before showing the next news, unless there's a priority interrupt
            if (currentNewsIndex < currentBatch.Count)
            {
                float cooldownTimer = 0f;
                isCoolingDown = true;
                Debug.Log($"Cooling down for {newsCooldownTime} seconds before next news");
                
                // Use a timer instead of yield return to allow for interruption
                while (cooldownTimer < newsCooldownTime && isCoolingDown)
                {
                    // Check if cooldown has been interrupted
                    if (!isCoolingDown)
                    {
                        Debug.Log("Cooldown interrupted for priority news");
                        break;
                    }
                    
                    // If new priority items were added, break immediately
                    if (pendingNewsText.Count > 0)
                    {
                        Debug.Log("Breaking cooldown for new priority news");
                        break;
                    }
                    
                    cooldownTimer += Time.deltaTime;
                    yield return null;
                }
                
                isCoolingDown = false;
            }
        }
        
        // Check if new items were added during display
        if (pendingNewsText.Count > 0)
        {
            // Start a new sequence for newly added items
            updatePending = true;
        }
        
        // Reset the index for next batch
        currentNewsIndex = 0;
    }
    
    private IEnumerator ScrollSingleNews(RectTransform textRectTransform)
    {
        // Force Canvas and text to update
        Canvas.ForceUpdateCanvases();
        newsTitleText.ForceMeshUpdate();
        
        // Get text width (this should now be reliable)
        float textWidth = newsTitleText.preferredWidth;
        
        // If still 0, try getting it from the rect
        if (textWidth <= 0)
        {
            textWidth = textRectTransform.rect.width;
        }
        
        // If still 0, use text content length as a rough estimate
        if (textWidth <= 0 && newsTitleText.text.Length > 0)
        {
            textWidth = newsTitleText.text.Length * 20f; // Rough estimate based on character count
        }
        
        // Final fallback
        if (textWidth <= 0)
        {
            textWidth = 500f;
        }
        
        // Set initial position - start offscreen to the right
        Vector2 position = textRectTransform.anchoredPosition;
        position.x = startPositionX;
        textRectTransform.anchoredPosition = position;
        
        // Wait another frame before starting the loop
        yield return null;
        
        hasStartedMoving = false;
        bool completedOneCycle = false;
        
        // Scroll this single news headline until it completes one cycle
        while (!completedOneCycle)
        {
            // If we're cooling down between news, pause scrolling
            if (isCoolingDown)
            {
                yield return null;
                continue;
            }
            
            // Get current position
            position = textRectTransform.anchoredPosition;
            
            // Current right edge position 
            float rightEdgePosition = position.x + textWidth;
            
            if (!hasStartedMoving)
            {
                hasStartedMoving = true;
            }
            
            // Move the text from right to left
            position.x -= scrollSpeed * Time.deltaTime;
            textRectTransform.anchoredPosition = position;
            
            // Reset when the RIGHT edge moves past the LEFT boundary
            if (position.x + textWidth < resetPositionX)
            {
                position.x = startPositionX;
                textRectTransform.anchoredPosition = position;
                
                completedOneCycle = true;
            }
            
            yield return null;
        }
    }
    
    // Draw debug lines in the editor
    private void OnDrawGizmos()
    {
        if (showDebugLines && newsTitleText != null)
        {
            DrawDebugLines();
        }
    }
    
    // Method to draw debug lines
    private void DrawDebugLines()
    {
        RectTransform canvasRect = newsTitleText.canvas.GetComponent<RectTransform>();
        RectTransform textRect = newsTitleText.rectTransform;
        
        // Calculate world space position of points on canvas
        Vector3 startPoint = GetWorldPositionFromCanvasPosition(canvasRect, textRect, new Vector2(startPositionX, textRect.anchoredPosition.y));
        Vector3 resetPoint = GetWorldPositionFromCanvasPosition(canvasRect, textRect, new Vector2(resetPositionX, textRect.anchoredPosition.y));
        
        // Ensure lines are long enough to be visible
        float lineHeight = 1000f;
        Vector3 startTop = startPoint + Vector3.up * lineHeight / 2;
        Vector3 startBottom = startPoint - Vector3.up * lineHeight / 2;
        Vector3 resetTop = resetPoint + Vector3.up * lineHeight / 2;
        Vector3 resetBottom = resetPoint - Vector3.up * lineHeight / 2;
        
        // Set line color
        Gizmos.color = debugLineColor;
        if (Application.isPlaying)
        {
            // Draw during game runtime
            Debug.DrawLine(startTop, startBottom, debugLineColor);
            Debug.DrawLine(resetTop, resetBottom, debugLineColor);
        }
        else
        {
            // Draw in editor
            Gizmos.DrawLine(startTop, startBottom);
            Gizmos.DrawLine(resetTop, resetBottom);
        }
    }
    
    // Convert canvas coordinates to world coordinates
    private Vector3 GetWorldPositionFromCanvasPosition(RectTransform canvasRect, RectTransform elementRect, Vector2 anchoredPosition)
    {
        // Create a temporary RectTransform for calculation
        GameObject tempObj = new GameObject("TempCalcPoint");
        tempObj.transform.SetParent(elementRect.parent);
        RectTransform tempRect = tempObj.AddComponent<RectTransform>();
        tempRect.anchorMin = elementRect.anchorMin;
        tempRect.anchorMax = elementRect.anchorMax;
        tempRect.pivot = elementRect.pivot;
        tempRect.anchoredPosition = anchoredPosition;
        
        // Get world position
        Vector3 worldPos = tempRect.position;
        
        // Delete temporary object
        if (Application.isPlaying)
            Destroy(tempObj);
        else
            DestroyImmediate(tempObj);
            
        return worldPos;
    }
    
    /// <summary>
    /// Force an immediate update of the news text without waiting for the current cycle to complete
    /// This should be used only when absolutely necessary
    /// </summary>
    /// <param name="newText">The new text to display</param>
    public void ForceNewsUpdate(string newText)
    {
        if (newsTitleText != null)
        {
            // Stop any current scrolling and news display sequence
            StopAllCoroutines();
            isScrollingActive = false;
            isCoolingDown = false;
            
            // Update the text immediately
            newsTitleText.text = newText;
            newsTitleText.ForceMeshUpdate();
            
            // Clear any pending updates
            updatePending = false;
            pendingNewsText.Clear();
            currentNewsIndex = 0;
            
            Debug.Log($"Forced news update: '{newText}'");
            
            // Only restart scrolling if there's actual content
            if (!string.IsNullOrWhiteSpace(newText))
            {
                StartScrollingIfNeeded();
            }
        }
    }
    
    // Using Backend.GeminiResponse for JSON deserialization
    
    /// <summary>
    /// Unpacks a Gemini API JSON response string and queues a news title update
    /// The update will apply after the current scroll cycle completes
    /// Handles Gemini API response format from Backend.GeminiResponse
    /// </summary>
    /// <param name="rawResponse">The raw JSON string to unpack from Gemini API</param>
    protected void UnpackNewsResponse(string rawResponse, GeminiRequestType type)
    {
        if (type != GeminiRequestType.News)
            return;
            
        // Check if we've reached the daily news limit
        if (!TimeManager.Instance.CanGenerateMoreNews())
        {
            Debug.Log("Daily news limit reached. Skipping news generation.");
            return;
        }
            
        try
        {
            // Deserialize the JSON string to Backend.GeminiResponse object
            Backend.GeminiResponse response = JsonConvert.DeserializeObject<Backend.GeminiResponse>(rawResponse);
            
            // Validate response structure and extract the text content
            if (response != null && 
                response.Candidates != null && 
                response.Candidates.Count > 0 && 
                response.Candidates[0].Contents != null &&
                response.Candidates[0].Contents.Parts != null &&
                response.Candidates[0].Contents.Parts.Count > 0 &&
                !string.IsNullOrEmpty(response.Candidates[0].Contents.Parts[0].Text))
            {
                // Extract the actual news headline text from the Gemini response
                string newsHeadline = response.Candidates[0].Contents.Parts[0].Text;
                
                // Remove any JSON formatting or quotes that might be in the text
                newsHeadline = CleanResponseText(newsHeadline);
                
                // Update the news title text
                if (newsTitleText != null)
                {
                    // Increment the daily news counter
                    if (TimeManager.Instance.CanGenerateMoreNews())
                    {
                        // Queue the update instead of applying immediately
                        pendingNewsText.Add(newsHeadline);
                        generatedActions.Add(newsHeadline);
                        updatePending = true;
                        
                        // If no active scrolling and we have content, start it now
                        if (!isScrollingActive && !string.IsNullOrWhiteSpace(newsHeadline))
                        {
                            StartScrollingIfNeeded();
                            // The event will be invoked when the news is actually displayed
                        }
                    }
                    else
                    {
                        Debug.Log($"News limit reached but tried to queue: '{newsHeadline}'");
                    }
                }
                else
                {
                    Debug.LogError("Cannot update news title: newsTitleText is null");
                }
            }
            else
            {
                Debug.LogWarning("Invalid Gemini response format or missing text content");
                Debug.LogWarning($"Raw response: {rawResponse}");
            }
        }
        catch (JsonException e)
        {
            Debug.LogError($"Error parsing Gemini response JSON: {e.Message}");
            Debug.LogError($"Raw response: {rawResponse}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error in UnpackNewsResponse: {e.Message}");
        }
    }
    
    /// <summary>
    /// Cleans the response text by removing any JSON formatting or extra quotes
    /// </summary>
    /// <param name="text">The raw text from Gemini response</param>
    /// <returns>Cleaned text suitable for display</returns>
    private string CleanResponseText(string text)
    {
        // Remove any JSON code blocks that might be in the text
        if (text.Contains("```json"))
        {
            int startIndex = text.IndexOf("```json");
            int endIndex = text.IndexOf("```", startIndex + 6);
            if (endIndex > startIndex)
            {
                string jsonPart = text.Substring(startIndex + 7, endIndex - startIndex - 7);
                try
                {
                    // Try to parse as JSON to extract a news headline from various possible formats
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPart);
                    if (jsonObj != null)
                    {
                        // Check for common field names that might contain the headline
                        if (jsonObj.ContainsKey("Action"))
                            return jsonObj["Action"];
                        if (jsonObj.ContainsKey("action"))
                            return jsonObj["action"];
                        if (jsonObj.ContainsKey("headline"))
                            return jsonObj["headline"];
                        if (jsonObj.ContainsKey("Headline"))
                            return jsonObj["Headline"];
                        if (jsonObj.ContainsKey("text"))
                            return jsonObj["text"];
                        if (jsonObj.ContainsKey("Text"))
                            return jsonObj["Text"];
                        if (jsonObj.ContainsKey("news"))
                            return jsonObj["news"];
                        if (jsonObj.ContainsKey("News"))
                            return jsonObj["News"];
                            
                        // If we have any value, just return the first one
                        if (jsonObj.Count > 0)
                            return jsonObj.Values.First();
                    }
                }
                catch
                {
                    // If JSON parsing failed, just clean up the text
                    // Remove the JSON markers and try to parse the whole text
                    text = text.Replace("```json", "").Replace("```", "").Trim();
                }
            }
        }
        
        // Remove leading/trailing quotes
        text = text.Trim();
        if (text.StartsWith("\"") && text.EndsWith("\""))
        {
            text = text.Substring(1, text.Length - 2);
        }
        
        return text;
    }

    /// <summary>
    /// Gradually scales up a transform from zero to its full size, creating a visual "breaking news" animation effect.
    /// </summary>
    /// <param name="t">The Transform to be scaled up (typically a UI element for breaking news)</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    /// <remarks>
    /// This coroutine scales the transform by multiplying its current scale by a growth factor each frame.
    /// The animation continues until the X scale (width) reaches or exceeds 1.0.
    /// The growth rate is set to 2 times delta time, which provides a balanced animation speed 
    /// that works well across different frame rates.
    /// </remarks>
    IEnumerator ShowBreakingNews(Transform t)
    {
        t.gameObject.SetActive(true);
        t.localScale = Vector3.zero;
        float time = 0.0f;
        while (t.localScale.x < 1)
        {
            time += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2);
        time = 0;
        while (t.localScale.x > 0)
        {
            time += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnChangeCamera(int cameraIndex)
    {
        if (cameraIndex == 2)
        {
            breakingNewsContainer.SetActive(false);
        }
    }
    
    private void OnSwitchPanel(int panelIndex)
    {
        if (panelIndex == 2)
        {
            breakingNewsContainer.SetActive(false);
        }
    }
    
    /// <summary>
    /// Handles public reaction events from GameplayManager
    /// Displays the reaction immediately, skipping any cooldown
    /// </summary>
    /// <param name="reaction">The formatted reaction text</param>
    /// <param name="score">Reaction score from -2 to 2</param>
    /// <summary>
    /// Formats a public reaction text with an appropriate prefix based on score
    /// Handles various JSON formats that might be returned by the AI
    /// </summary>
    /// <param name="react">Raw reaction text or JSON</param>
    /// <param name="score">Reaction score from -2 to 2</param>
    /// <returns>Formatted reaction ready for display</returns>
    private string FormatPublicReaction(string react, int score)
    {
        Debug.Log("BBBBBBBBBBBB " + react);
        // First, clean up markdown code blocks if present
        string cleanedReact = react;
        if (react.Contains("```"))
        {
            // Remove markdown code block markers (backticks)
            cleanedReact = react.Replace("```json", "").Replace("```", "").Trim();
        }
        
        string extractedReaction = "";
        int extractedScore = score;
        
        // Try to extract the public reaction text from JSON
        try
        {
            // Try to parse as JSON
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(cleanedReact);
            if (jsonObj != null)
            {
                // Check for public reaction with case insensitivity
                foreach (var key in jsonObj.Keys)
                {
                    if (key.ToLower() == "publicreaction")
                    {
                        extractedReaction = jsonObj[key].ToString();
                        break;
                    }
                }
                
                // Try to get score from JSON if available (case insensitive)
                foreach (var key in jsonObj.Keys)
                {
                    if (key.ToLower() == "responsequality" || key.ToLower() == "savequality")
                    {
                        try
                        {
                            extractedScore = Convert.ToInt32(jsonObj[key]);
                            break;
                        }
                        catch
                        {
                            // Keep using the passed-in score if conversion fails
                        }
                    }
                }
                
                // If we found a reaction in the JSON, format it
                if (!string.IsNullOrEmpty(extractedReaction))
                {
                    return FormatWithPrefix(extractedReaction, extractedScore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error parsing reaction JSON: {e.Message}");
        }
        
        // If JSON parsing fails, try to extract directly from text
        try
        {
            if (cleanedReact.Contains("publicreaction") || cleanedReact.Contains("publicReaction"))
            {
                string searchTerm = cleanedReact.Contains("publicReaction") ? "publicReaction" : "publicreaction";
                int startIndex = cleanedReact.IndexOf(searchTerm) + searchTerm.Length;
                
                // Skip past any characters like : or " or whitespace
                while (startIndex < cleanedReact.Length && 
                      (cleanedReact[startIndex] == ':' || cleanedReact[startIndex] == '"' || 
                       char.IsWhiteSpace(cleanedReact[startIndex])))
                {
                    startIndex++;
                }
                
                int endIndex = cleanedReact.IndexOf("\"", startIndex);
                if (endIndex < 0)
                {
                    // Try to find the end using a comma or closing brace
                    endIndex = cleanedReact.IndexOf(",", startIndex);
                    if (endIndex < 0)
                    {
                        endIndex = cleanedReact.IndexOf("}", startIndex);
                    }
                }
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    extractedReaction = cleanedReact.Substring(startIndex, endIndex - startIndex)
                        .Replace("\"", "").Trim();
                    
                    if (!string.IsNullOrEmpty(extractedReaction))
                    {
                        return FormatWithPrefix(extractedReaction, extractedScore);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to extract reaction from text: {e.Message}");
        }
        
        // If all parsing fails, just add a prefix to the original text if it's too long
        if (cleanedReact.Length > 100)
        {
            // It's probably still JSON or something we couldn't parse well
            // Just use a generic message based on the score
            string genericMessage = "";
            switch (extractedScore)
            {
                case -2:
                    genericMessage = "The public is outraged by the CEO's response!";
                    break;
                case -1:
                    genericMessage = "The public is disappointed with the CEO's statement.";
                    break;
                case 0:
                    genericMessage = "The public has mixed feelings about the response.";
                    break;
                case 1:
                    genericMessage = "The public generally approves of the CEO's message.";
                    break;
                case 2:
                    genericMessage = "The public is thrilled with the CEO's brilliant response!";
                    break;
                default:
                    genericMessage = "Public reacts to CEO's statement.";
                    break;
            }
            return FormatWithPrefix(genericMessage, extractedScore);
        }
        
        // Last resort - just format the cleaned text
        return FormatWithPrefix(cleanedReact, extractedScore);
    }
    
    /// <summary>
    /// Adds an appropriate prefix to the reaction text based on score
    /// </summary>
    private string FormatWithPrefix(string reaction, int score)
    {
        // Format with a prefix based on score
        string prefix = "BREAKING: ";
        switch (score)
        {
            case -2:
                prefix += "PUBLIC OUTRAGE! ";
                break;
            case -1:
                prefix += "Public Dissatisfied - ";
                break;
            case 0:
                prefix += "Mixed Reactions: ";
                break;
            case 1:
                prefix += "Positive Response: ";
                break;
            case 2:
                prefix += "OVERWHELMING SUPPORT! ";
                break;
        }
        
        // return prefix + reaction;
        return reaction;
    }
    
    private void OnPublicReact(string react, int score)
    {
        // Format the public reaction for news display
        string formattedReaction = FormatPublicReaction(react, score);
        
        // If we're currently in a cooldown, skip it
        if (isCoolingDown)
        {
            Debug.Log("Interrupting cooldown to display public reaction");
            isCoolingDown = false;
        }
        
        // If we're displaying news sequentially, we need to handle differently
        if (isScrollingActive)
        {
            // Save any pending news items before forcing update
            List<string> savedPendingNews = new List<string>(pendingNewsText);
            
            // Force the immediate display of this reaction
            ForceNewsUpdate(formattedReaction);
            
            // Add the previously pending news back to the queue
            if (savedPendingNews.Count > 0)
            {
                // Add them back to the queue
                foreach (string news in savedPendingNews)
                {
                    pendingNewsText.Add(news);
                }
                
                Debug.Log($"Re-queuing {savedPendingNews.Count} pending news items after reaction");
                updatePending = true;
                
                // Restart the display sequence which will reset the cooldown properly
                StartCoroutine(DisplayNewsSequentially());
            }
        }
        else
        {
            // If no active scrolling, insert at first position and start display
            if (pendingNewsText.Count > 0)
            {
                pendingNewsText.Insert(0, formattedReaction);
            }
            else
            {
                pendingNewsText.Add(formattedReaction);
            }
            
            updatePending = true;
            StartScrollingIfNeeded();
        }
        
        // Ensure cooldown is properly reset for next news item
        isCoolingDown = false;
        
        Debug.Log($"Public reaction added to news: '{formattedReaction}' with score {score}");
    }
}
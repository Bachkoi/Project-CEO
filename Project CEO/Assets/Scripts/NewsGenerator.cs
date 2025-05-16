using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System;
using Sirenix.OdinInspector;
using System.Linq;

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
    private string pendingNewsText = "";
    private bool isScrollingActive = false;
    
    [SerializeField]
    private Color debugLineColor = Color.blue; // Debug line color
    
    [SerializeField]
    private bool showDebugLines = true; // Toggle to show debug lines

    [SerializeField] protected GameObject breakingNewsContainer;

    [SerializeField, BoxGroup("Prompt"), TextArea(7, 7)] private string newsPrompt;
    [SerializeField, BoxGroup("Prompt"), TextArea(7, 7)] private string updatedNewsPrompt;
    [SerializeField, BoxGroup("Prompt"), ReadOnly] private List<string> generatedActions = new List<string>();
    
    [SerializeField, ReadOnly] private PanelManager panelManager;

    private void OnEnable()
    {
        UnityToGemini.GeminiResponseCallback += UnpackNewsResponse;
        //PanelManager.switchPanel += OnSwitchPanel;
    }

    private void OnDisable()
    {
        UnityToGemini.GeminiResponseCallback -= UnpackNewsResponse;
        //PanelManager.switchPanel -= OnSwitchPanel;
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

        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdatePrompt();
            UnityToGemini.Instance.SendNewsRequest(updatedNewsPrompt);
        }
    }

    private void UpdatePrompt()
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
        
        Debug.Log($"Starting scroll. Text width: {textWidth}, Position: {position.x}, Reset at: {resetPositionX}");
        
        // Wait another frame before starting the loop
        yield return null;
        
        hasStartedMoving = false;
        bool completedOneCycle = false;
        
        // Now we can start scrolling
        while (true)
        {
            // Get current position
            position = textRectTransform.anchoredPosition;
            
            // Current right edge position 
            float rightEdgePosition = position.x + textWidth;
            
            if (!hasStartedMoving)
            {
                Debug.Log($"Initial position - X: {position.x}, Right Edge: {rightEdgePosition}");
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
                
                // Check if there's a pending update after completing at least one cycle
                if (updatePending && completedOneCycle)
                {
                    Debug.Log("Applying pending news update after cycle completion");
                    StartCoroutine(ShowBreakingNews(breakingNewsContainer.transform));

                    //if (panelManager.activePanel != 2)
                    //{
                    //    StartCoroutine(ShowBreakingNews(breakingNewsContainer.transform));
                    //}
                    
                    // Update the text with the pending news
                    newsTitleText.text = pendingNewsText;
                    newsTitleText.ForceMeshUpdate();
                    
                    // Clear the pending state
                    updatePending = false;
                    pendingNewsText = "";
                    
                    // Get updated width and reset position
                    textWidth = newsTitleText.preferredWidth;
                    if (textWidth <= 0)
                    {
                        if (newsTitleText.text.Length > 0)
                        {
                            textWidth = newsTitleText.text.Length * 20f;
                        }
                        else
                        {
                            textWidth = 500f;
                        }
                    }
                    
                    // Start from the beginning with the new text
                    position.x = startPositionX;
                    textRectTransform.anchoredPosition = position;
                    
                    // Reset cycle completion flag to ensure at least one full cycle of the new text
                    completedOneCycle = false;
                    
                    Debug.Log($"Updated news title text: '{newsTitleText.text}' with width: {textWidth}");
                }
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
            // Stop any current scrolling
            StopAllCoroutines();
            isScrollingActive = false;
            
            // Update the text immediately
            newsTitleText.text = newText;
            newsTitleText.ForceMeshUpdate();
            
            // Clear any pending updates
            updatePending = false;
            pendingNewsText = "";
            
            Debug.Log($"Forced news update: '{newText}'");
            
            // Only restart scrolling if there's actual content
            if (!string.IsNullOrWhiteSpace(newText))
            {
                StartScrollingIfNeeded();
            }
        }
    }
    
    // News response class to deserialize JSON from Gemini API
    [Serializable]
    private class NewsResponse
    {
        [JsonProperty("candidates")]
        public List<NewsCandidate> Candidates { get; set; }
    }
    
    [Serializable]
    private class NewsCandidate
    {
        [JsonProperty("content")]
        public NewsContent Content { get; set; }
    }
    
    [Serializable]
    private class NewsContent
    {
        [JsonProperty("parts")]
        public List<NewsPart> Parts { get; set; }
        
        [JsonProperty("role")]
        public string Role { get; set; }
    }
    
    [Serializable]
    private class NewsPart
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    
    /// <summary>
    /// Unpacks a Gemini API JSON response string and queues a news title update
    /// The update will apply after the current scroll cycle completes
    /// Handles Gemini API response format from Backend.GeminiResponse
    /// </summary>
    /// <param name="rawResponse">The raw JSON string to unpack from Gemini API</param>
    protected void UnpackNewsResponse(string rawResponse)
    {
        try
        {
            // Deserialize the JSON string to NewsResponse object
            NewsResponse response = JsonConvert.DeserializeObject<NewsResponse>(rawResponse);
            
            // Validate response structure and extract the text content
            if (response != null && 
                response.Candidates != null && 
                response.Candidates.Count > 0 && 
                response.Candidates[0].Content != null &&
                response.Candidates[0].Content.Parts != null &&
                response.Candidates[0].Content.Parts.Count > 0 &&
                !string.IsNullOrEmpty(response.Candidates[0].Content.Parts[0].Text))
            {
                // Extract the actual news headline text from the Gemini response
                string newsHeadline = response.Candidates[0].Content.Parts[0].Text;
                
                // Remove any JSON formatting or quotes that might be in the text
                newsHeadline = CleanResponseText(newsHeadline);
                
                // Update the news title text
                if (newsTitleText != null)
                {
                    // Queue the update instead of applying immediately
                    pendingNewsText = newsHeadline;
                    generatedActions.Add(newsHeadline);
                    updatePending = true;
                    
                    Debug.Log($"News update queued: '{newsHeadline}'. Will apply after current cycle completes.");
                    
                    // If no active scrolling and we have content, start it now
                    if (!isScrollingActive && !string.IsNullOrWhiteSpace(newsHeadline))
                    {
                        StartScrollingIfNeeded();
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
        
        // Try to parse the entire text as JSON if it starts with {
        if (text.Trim().StartsWith("{") && text.Trim().EndsWith("}"))
        {
            try
            {
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
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
                // If JSON parsing failed, continue with other cleaning methods
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

    private void OnSwitchPanel(int panelIndex)
    {
        if (panelIndex == 2)
        {
            breakingNewsContainer.SetActive(false);
        }
    }
}
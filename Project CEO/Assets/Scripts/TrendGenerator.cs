using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class TrendGenerator : MonoBehaviour
{
    public List<string> trends;

    [SerializeField, TextArea(7, 7)] private string trendPrompt;

    [SerializeField, TextArea(7, 7), ReadOnly] private string updatedTrendPrompt;
    
    //getters & setters
    public string CurrentTrend {get=>trends[trends.Count - 1];}

    private void OnEnable()
    {
        UnityToGemini.GeminiResponseCallback += UnpackTrendResponse;
        TimeManager.onWeekChange += GenerateTrend;
    }

    private void OnDisable()
    {
        UnityToGemini.GeminiResponseCallback -= UnpackTrendResponse;
        TimeManager.onWeekChange -= GenerateTrend;
    }

    private void Start()
    {
        GenerateTrend(0);
    }

    public void GenerateTrend(int weekCount)
    {
        UpdatePrompt();
        UnityToGemini.Instance.SendRequest(updatedTrendPrompt, GeminiRequestType.Trend);
    }

    /// <summary>
    /// Unpacks a Gemini API JSON response string and extracts the trend information
    /// Handles Gemini API response format from Backend.GeminiResponse with "Trend: trend" format
    /// </summary>
    /// <param name="rawResponse">The raw JSON string to unpack from Gemini API</param>
    /// <param name="type">The type of Gemini request that generated this response</param>
    public void UnpackTrendResponse(string rawResponse, GeminiRequestType type)
    {
        // Only process if this is a trend request
        if (type != GeminiRequestType.Trend)
            return;
            
        try
        {
            // Deserialize the JSON string to NewsResponse object (reusing NewsGenerator's response classes)
            var response = JsonConvert.DeserializeObject<Backend.GeminiResponse>(rawResponse);
            
            // Validate response structure and extract the text content
            if (response != null && 
                response.Candidates != null && 
                response.Candidates.Count > 0 && 
                response.Candidates[0].Contents != null &&
                response.Candidates[0].Contents.Parts != null &&
                response.Candidates[0].Contents.Parts.Count > 0 &&
                !string.IsNullOrEmpty(response.Candidates[0].Contents.Parts[0].Text))
            {
                // Extract the trend text from the Gemini response
                string responseText = response.Candidates[0].Contents.Parts[0].Text;
                
                // Clean the response text and extract the trend
                string trendText = ExtractTrendFromResponse(responseText);
                
                // Add the trend to the list if it's valid
                if (!string.IsNullOrWhiteSpace(trendText))
                {
                    // Initialize the trends list if it doesn't exist
                    if (trends == null)
                    {
                        trends = new List<string>();
                    }
                    
                    // Add the new trend
                    trends.Add(trendText);
                    
                    Debug.Log($"New trend added: '{trendText}'");
                }
                else
                {
                    Debug.LogWarning("Could not extract a valid trend from response");
                }
            }
            else
            {
                Debug.LogWarning("Invalid Gemini response format or missing text content for trend");
                Debug.LogWarning($"Raw response: {rawResponse}");
            }
        }
        catch (JsonException e)
        {
            Debug.LogError($"Error parsing Gemini response JSON in UnpackTrendResponse: {e.Message}");
            Debug.LogError($"Raw response: {rawResponse}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error in UnpackTrendResponse: {e.Message}");
        }
    }
    
    /// <summary>
    /// Extracts the trend value from a response text that should be in the format "Trend: trend"
    /// </summary>
    /// <param name="responseText">The raw response text from Gemini API</param>
    /// <returns>The extracted trend text, or the original text if the format doesn't match</returns>
    private string ExtractTrendFromResponse(string responseText)
    {
        // Trim the response
        string text = responseText.Trim();
        
        // Check for JSON format in code blocks
        if (text.Contains("```json"))
        {
            int startIndex = text.IndexOf("```json");
            int endIndex = text.IndexOf("```", startIndex + 6);
            
            if (endIndex > startIndex)
            {
                string jsonPart = text.Substring(startIndex + 7, endIndex - startIndex - 7);
                try
                {
                    // Try to parse as JSON
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPart);
                    if (jsonObj != null)
                    {
                        // Check for trend fields in various formats
                        if (jsonObj.ContainsKey("Trend"))
                            return jsonObj["Trend"];
                        if (jsonObj.ContainsKey("trend"))
                            return jsonObj["trend"];
                        
                        // If we have any value, just return the first one
                        if (jsonObj.Count > 0)
                            return jsonObj.Values.First();
                    }
                }
                catch
                {
                    // If JSON parsing failed, just clean up the text
                    text = text.Replace("```json", "").Replace("```", "").Trim();
                }
            }
        }
        
        // Try to parse the entire text as JSON if it looks like JSON
        if (text.StartsWith("{") && text.EndsWith("}"))
        {
            try
            {
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                if (jsonObj != null)
                {
                    // Check for trend fields
                    if (jsonObj.ContainsKey("Trend"))
                        return jsonObj["Trend"];
                    if (jsonObj.ContainsKey("trend"))
                        return jsonObj["trend"];
                    
                    // If we have any value, just return the first one
                    if (jsonObj.Count > 0)
                        return jsonObj.Values.First();
                }
            }
            catch
            {
                // Continue with other parsing methods if this fails
            }
        }
        
        // Look for "Trend: " prefix
        const string trendPrefix = "Trend: ";
        if (text.StartsWith(trendPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return text.Substring(trendPrefix.Length).Trim();
        }
        
        // If no specific format is found, check if there's a "Trend:" anywhere in the text
        int trendIndex = text.IndexOf("Trend:", StringComparison.OrdinalIgnoreCase);
        if (trendIndex >= 0)
        {
            // Extract everything after "Trend:"
            return text.Substring(trendIndex + 6).Trim();
        }
        
        // If all parsing attempts fail, return the original text
        return text;
    }
    
    public void UpdatePrompt()
    {
        if (!string.IsNullOrEmpty(trendPrompt))
        {
            updatedTrendPrompt = trendPrompt;

            if (updatedTrendPrompt.Contains("{0}"))
            {
                updatedTrendPrompt = updatedTrendPrompt.Replace("{0}", UnityToGemini.Instance.companyName);
            }
            if (updatedTrendPrompt.Contains("{1}"))
            {
                updatedTrendPrompt = updatedTrendPrompt.Replace("{1}", UnityToGemini.Instance.companyDescription);
            }
            if (updatedTrendPrompt.Contains("{2}"))
            {
                string replacingString = "";
                if (trends.Count > 0)
                {
                    replacingString += "You have given me these actions already: ";
                    for (int  i = 0;  i < trends.Count;  i++)
                    {
                        replacingString += trends[i];
                        if (i < trends.Count - 1)
                            replacingString += ", ";
                        else
                            replacingString += ". ";
                    }
                }
                updatedTrendPrompt = updatedTrendPrompt.Replace("{2}", replacingString);
            }
        }
    }
}

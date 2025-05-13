using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Backend 
{

    #region Gemini Models
    // Gemini request models
    [System.Serializable]
    public class GeminiRequest
    {
        [JsonProperty("contents")]
        public List<Content> Contents { get; set; }

        [JsonProperty("systemInstruction")]
        public Content systemInstruction { get; set; }

        //public List<Content> SystemInstruction { get; set; }
        //[JsonProperty("prompt")]
        //public string? Prompt { get; set; }
    }

    [System.Serializable]
    public class Content
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("parts")]
        public List<Part> Parts { get; set; }

        //public Content(string? role, List<Part>? parts)
        //{
        //    Role = role;
        //    Parts = parts;
        //}
    }

    [System.Serializable]
    public class Part
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        //[JsonProperty("npcEmotion")]
        //public string NPCEmotion { get; set; }

        public Part(string text)
        {
            Text = text;
            //NPCEmotion = npcEmotion;
        }
    }

    // Gemini response models
    [System.Serializable]
    public class GeminiResponse
    {
        [JsonProperty("candidates")]
        public List<Candidate> Candidates { get; set; }
        [JsonProperty("usageMetadata")]
        public UsageMetadata UsageMetadata { get; set; }
    }

    [System.Serializable]
    public class Candidate
    {
        [JsonProperty("content")]
        public Content Contents { get; set; }
        [JsonProperty("finishReason")]
        public string FinishReason { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("safeRatings")]
        public List<SafetyRating> SafetyRatings { get; set; }
    }

    [System.Serializable]
    public class SafetyRating
    {
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("probability")]
        public string Probability { get; set; }
    }

    [System.Serializable]
    public class UsageMetadata
    {
        [JsonProperty("promptTokenCount")]
        public int PromptTokenCount { get; set; }
        [JsonProperty("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }
        [JsonProperty("totalTokenCount")]
        public int TotalTokenCount { get; set; }
        
    }
    #endregion

    [System.Serializable]
    public class GeminiErrorResponse
    {
        [JsonPropertyAttribute("error")]
        public GeminiErrorDetail Error { get; set; }
    }

    [System.Serializable]

    public class GeminiErrorDetail
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("details")]
        public List<GeminiErrorDetailItem> Details { get; set; }
    }

    [System.Serializable]
    public class GeminiErrorDetailItem
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("violations")]
        public List<GeminiQuotaViolation> Violations { get; set; }

        [JsonProperty("links")]
        public List<GeminiHelpLink> Links { get; set; }

        [JsonProperty("retryDelay")]
        public string RetryDelay { get; set; }
    }

    [System.Serializable]
    public class GeminiQuotaViolation
    {
        [JsonProperty("quotaMetric")]
        public string QuotaMetric { get; set; }

        [JsonProperty("quotaId")]
        public string QuotaId { get; set; }

        [JsonProperty("quotaDimensions")]
        public Dictionary<string, string> QuotaDimensions { get; set; }

        [JsonProperty("quotaValue")]
        public string QuotaValue { get; set; }
    }

    [System.Serializable]
    public class GeminiHelpLink
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }


    public class GameEventModel
    {
        [JsonProperty("event")]
        public EventName Event { get; set; }

        [JsonProperty("gameId")]
        public string GameId { get; set; }
        
        [JsonProperty("balance")]
        public float Balance { get; set; }
        
        [JsonProperty("userName")]
        public string UserName { get; set; }
        
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("geminiStatusCode")]
        public long GeminiStatusCode { get; set; }

        [JsonProperty("geminiErrorMessage")]
        public string GeminiErrorMessage { get; set; }

        [JsonProperty("geminiAPIKey")]
        public string GeminiAPIKey { get; set; }
    }

    #region Enum
    // Internal conflict status of customers
    public enum InternalConflictStatus
    {
        VeryGood = 2,
        Good = 1,
        Neutral = 0,
        Bad = -1,
        VeryBad = -2
    }

    // Character of customers
    public enum Character
    {
        Soldier = 0,
    }

    
    // Gemini Error Code
    public enum GeminiErrorCode
    {
        ApiKeyInvalidError = 101, // Tested
        RequestFormatInvalidError = 102, // Tested
        RequestTooFrequentError = 201, // Tested
        RequestTooLargeError = 202, // Failed to reach, will test again when we have image/file data sent to Gemini
        GeminiInternalError = 301, // Have not encountered
        JsonParsingError = 303,
        UnknownError = 401, // Have not encountered
        RateLimitReached = 429
    }

    public enum EventName
    {
        GameStarted, // Game started
        GameQuit, // Game quit
        CustomerEntered, // One customer entered the bar
        CustomerLeft, // One customer left the bar
        ChatSent, // Chat sent by the player
        ChatReceived, // Chat received by the player
        GrillStarted, // Grill order created
        GrillSubmitted, // Grill order submitted
        MilkshakeStarted, // Milkshake order created
        MilkshakeSubmitted, // Milkshake order submitted
        SummarizerStarted,
        SummarizerReceived,
        JudgeStarted,
        JudgeReceived
    }
    // Gemini message role
    public enum GeminiRole
    {
        User = 0,
        Model = 1,
    }

    #endregion
}
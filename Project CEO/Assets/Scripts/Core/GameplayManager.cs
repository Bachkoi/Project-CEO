using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using System;

public class GameplayManager : MonoBehaviour
{
    public float stockThreshold = 30f;
    public float stockPriceStart = 100f;
    public float stockPriceChangeMagnitude = 10f;

    private string lastCompanyAction;
    private string lastPlayerResponse;

    private bool awaitingPlayerResponse = false;
    
    public NewsGenerator newsGenerator;

    public TMP_InputField playerInputField;

    public static event Action<string> onPlayerRespond;
    
    public void OnSubmitButtonClicked()
    {
        string input = playerInputField.text;
        OnPlayerSubmitResponse(input);
    }

    void Start()
    {
        StockPriceDisplay.Instance.Initialize("LLMG", stockPriceStart);
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            // 1. Company action
            
            yield return RequestCompanyAction();

            // 2. Player action
            awaitingPlayerResponse = true;
            Debug.Log("Waiting for player response...");
            yield return new WaitUntil(() => awaitingPlayerResponse == false);
            onPlayerRespond?.Invoke(lastPlayerResponse);
            TimeManager.Instance.OnPlayerRespond(lastPlayerResponse);

            // 3. Public Reaction
            yield return EvaluatePublicReaction(lastCompanyAction, lastPlayerResponse);

            // 4. Check if game ends
            if (StockPriceDisplay.Instance.CurrentPrice < stockThreshold)
            {
                Debug.Log("Game Over: Stock price fell below threshold.");
                yield break;
            }

            yield return new WaitForSeconds(2f); // Optional pacing
        }
    }

    public void OnPlayerSubmitResponse(string response)
    {
        lastPlayerResponse = response;
        awaitingPlayerResponse = false;
    }

    /// <summary>
    /// Asynchronously requests a company action from the Gemini AI model and waits for a response.
    /// </summary>
    /// <returns>An IEnumerator that can be used in a coroutine.</returns>
    /// <remarks>
    /// This coroutine performs the following steps:
    /// 1. Creates a prompt asking the AI to determine the company's next action based on current stock price
    /// 2. Sets up a temporary event handler to capture the AI's response
    /// 3. Sends the request to the Gemini API via UnityToGemini service
    /// 4. Waits until a response is received
    /// 5. Processes the response by:
    ///    - Extracting and storing the generated company action
    ///    - Logging the action for debugging
    ///    - Unsubscribing from the event to prevent memory leaks
    /// 
    /// The returned company action represents a decision made by the AI-simulated company board
    /// that will influence the game state and require player response.
    /// </remarks>
    IEnumerator RequestCompanyAction()
    {
        // Construct the prompt with the current stock price
        //string prompt = $"As the board of a public company, decide your next action. Current stock price: {StockPriceDisplay.Instance.CurrentPrice:F2}.";
        newsGenerator.UpdatePrompt();
        string prompt = newsGenerator.UpdatedNewsPrompt;
        bool isResponseReceived = false;

        // Create a local handler function to process the API response
        UnityToGemini.GeminiResponseCallback += OnCompanyAction;
        UnityToGemini.Instance.SendRequest(prompt, GeminiRequestType.News);

        // Local function that processes the AI response
        void OnCompanyAction(string responseJson, GeminiRequestType type)
        {
            var res = UnityToGemini.Instance.UnpackGeminiResponse(responseJson);
            lastCompanyAction = res.Candidates[0].Contents.Parts[0].Text;
            Debug.Log("Company action: " + lastCompanyAction);
            isResponseReceived = true;
            UnityToGemini.GeminiResponseCallback -= OnCompanyAction;
        }

        // Pause the coroutine until we get a response
        yield return new WaitUntil(() => isResponseReceived);
    }

    IEnumerator EvaluatePublicReaction(string companyAction, string playerResponse)
    {
        //string prompt = $"I'm talking to another AI acting as the public/reacting to a CEOs response. The company recently took this action: {companyAction}. The CEO responded to media/questions with: {playerResponse}.\r\n\r\nEvaluate the CEO's response based on:" +
        //    $"\r\n\r\nCrisis Management (Did it address the issue effectively?)" +
        //    $"\r\n\r\nTone/PR Skill (Was it confident, empathetic, or tone-deaf?)" +
        //    $"\r\n\r\nPublic Perception (How will typical stakeholders react?)." +
        //    $"\r\n\r\nReturn:" +
        //    $"\r\n\r\nA score from -2 to 2 (INTEGER ONLY), where:" +
        //    $"\r\n\r\n-2: Major backlash (e.g., offensive, evasive, or worsening the crisis)" +
        //    $"\r\n\r\n-1: Poor response (e.g., weak justification, minor tone-deafness, or lukewarm damage control)" +
        //    $"\r\n\r\n0: Neutral/no impact (e.g., generic corporate speak, neither harm nor gain)" +
        //    $"\r\n\r\n1: Good save (e.g., solid reasoning, timely apology, or partial trust restoration)" +
        //    $"\r\n\r\n2: Brilliant save (e.g., transformative framing, inspiring accountability, or viral positivity)" +
        //    $"\r\n\r\nA short public reaction (e.g., headlines, social media buzz)." +
        //    $"\r\n\r\nFormat output as:" +
        //    $"\r\n\r\njson\r\n{{\"saveQuality\": YourScoreHere, \"publicReaction\": \"Your prediction here\"}}  \r\n";
        string prompt = $"I’m talking to another AI acting as the public/reacting to a CEO’s response. The company recently took this action: {companyAction}. The CEO responded to media/questions with: {playerResponse}. Evaluate the CEO’s response based on: Crisis Management (Did it address the issue effectively?), Tone/PR Skill (Was it confident, empathetic, or tone-deaf?), Public Perception (How will typical stakeholders react?).\nYou will score their response on a score from -2 to 2 (INTEGER ONLY), where: -2 is backlash, -1 is Poor response, 0 is Neutral/no impact, 1 is Good save, 2 is Great save and a SHORT public reaction (e.g., headlines, social media buzz). Please return it as a json with the following output: {{\"saveQuality\": YourScoreHere, \"publicReaction\": \"Your prediction here\"}}\n";
        bool isResponseReceived = false;

        UnityToGemini.GeminiResponseCallback += OnPublicReaction;
        UnityToGemini.Instance.SendRequest(prompt, GeminiRequestType.PublicReaction);

        void OnPublicReaction(string responseJson, GeminiRequestType type)
        {
            if (type != GeminiRequestType.PublicReaction)
                return;
            var res = UnityToGemini.Instance.UnpackGeminiResponse(responseJson);
            string reaction = res.Candidates[0].Contents.Parts[0].Text.ToLower();
            Debug.Log("Public reaction: " + reaction);
            float delta = 0.0f;
            //float delta = reaction.Contains("up") ? stockPriceChangeMagnitude : -stockPriceChangeMagnitude;
            if (reaction.Contains("-2"))
            {
                delta = -stockPriceChangeMagnitude/2.0f;

            }
            else if (reaction.Contains("-1"))
            {
                delta = -stockPriceChangeMagnitude/4.0f;

            }
            else if (reaction.Contains("0"))
            {
                delta = stockPriceChangeMagnitude/4.0f;

            }
            else if (reaction.Contains("1"))
            {
                delta = stockPriceChangeMagnitude/2.0f;

            }
            else
            {
                delta = stockPriceChangeMagnitude;

            }
            float newPrice = Mathf.Max(0.01f, StockPriceDisplay.Instance.CurrentPrice + delta);
            StockPriceDisplay.Instance.UpdatePrice(newPrice);

            isResponseReceived = true;
            UnityToGemini.GeminiResponseCallback -= OnPublicReaction;
        }

        yield return new WaitUntil(() => isResponseReceived);
    }
}
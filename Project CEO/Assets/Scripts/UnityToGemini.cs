using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using Backend;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class UnityToGemini : MonoBehaviour
{
    // Necessary information for Gemini
    public string apiKey;
    public string url;
    public string lastJsonRequest;

    private static readonly List<string> InvalidJsonFormatPattern = new List<string>() { "```json", "```" };

    // Interrogation related variables
    public GameObject stockPriceCanvas;
    public string questionToAsk;
    public string objectToAsk;
    public string timeToAsk;
    public string interrogationType;

    public StockPriceDisplay spDisplay;
    
    [BoxGroup("Company Info")]
    public string companyName;
    public string companyDescription;

    public CameraManager cm;
    


    public UILineRendererTest UILRTEst;


    public string OGPrompt;

    public static UnityToGemini Instance;
    
    public static event Action<string> GeminiResponseCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else if (Instance != this)
        {
            Instance.apiKey ??= apiKey;
            Instance.url ??= url;
            Instance.lastJsonRequest ??= lastJsonRequest;
            Instance.stockPriceCanvas = stockPriceCanvas;
            Instance.spDisplay ??= spDisplay;
            Instance.companyDescription ??= companyDescription;
            Instance.companyName ??= companyName;
            Instance.UILRTEst ??= UILRTEst;
            Instance.OGPrompt ??= OGPrompt;
            Instance.cm ??= cm;
            Instance.UILRTEst ??= UILRTEst;
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Set the API key
        //apiKey = GeminiAPIVerifier.VerifiedApiKey;
    }

    // Start is called before the first frame update
    void Start()
    {
        //raycastDetector = GetComponent<RaycastDetector>();
        //
        //// Subscribe to events
        //raycastDetector.onObjectClicked.AddListener(HandleObjectClicked);
        //raycastDetector.onObjectHovered.AddListener(HandleObjectHovered);


        //if (spDisplay != null)
        //{
        //    // Example usage
        //    spDisplay = stockPriceCanvas?.GetComponent<StockPriceDisplay>();
        //    spDisplay?.Initialize("AAPL", 150.00f);
        //
        //
        //    // Later, to update the price (this will automatically update the chart)
        //    spDisplay?.UpdatePrice(155.50f);
        //}



        // Instantiate the InterrogationCanvas prefab
        //GameObject interrogationCanvasPrefab = Resources.Load<GameObject>("Prefabs/InterrogationCanvas");
        //if (interrogationCanvasPrefab != null)
        //{
        //    Instantiate(interrogationCanvasPrefab);
        //}
        //else
        //{
        //    Debug.LogError("InterrogationCanvas prefab not found. Please create it using the editor tool first.");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (!String.IsNullOrEmpty(apiKey))
        {
            if (Input.GetKeyDown(KeyCode.L) && apiKey != null)
            {
                //StartCoroutine(SendRequestWithDropdown(questionToAsk, interrogationType, timeToAsk));
            }
        
            if (Input.GetKeyDown(KeyCode.P))
            {
                float newPrice = spDisplay.CurrentPrice + Random.Range(-5f, 5f);
                spDisplay.UpdatePrice(newPrice);
                UILRTEst.ToggleColor();
                //StartCoroutine(SendKeyValidationToGemini(apiKey));
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                cm.SwitchCamera();
            }
        
            if(Input.GetKeyDown(KeyCode.Escape))
            {
             
                //interrogationCanvas.GetComponent<InterrogationCanvas>().OpenMenu();
            }
        }

    }

    public void SendRequest(){
        StartCoroutine(SendRequestWithDropdown(questionToAsk, interrogationType, timeToAsk));
    }

    public void SendNewsRequest(string request)
    {
        StartCoroutine((SendRequestToGemini(request)));
    }

    public IEnumerator SendRequestWithDropdown(string question, string interrogationType, string timeDropdown){
        // Send request to Gemini
        // Wait for response
        // Return response
            string url = "";

            url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + apiKey;

            // Serialize the object to JSON
            Backend.GeminiRequest geminiRequest = new Backend.GeminiRequest();
            geminiRequest.Contents = new List<Backend.Content>();
            //List<Backend.Part> tempParts = new List<Backend.Part>();
            //Backend.Part tempPart = new Backend.Part("\"text\":\"Test\"");
            //tempParts.Add(tempPart);
            //Backend.Content tempContent = new Backend.Content();
            //tempContent.Role = "user";
            //tempContent.Parts = tempParts;
            //geminiRequest.Contents.Add(tempContent);
            //string tempPrompt = new string("You are a helpful AI assistent who will respond for a character. Your job is to respond as if you are the son of a 1920s gangster. You were actually at the kitchen poisoning a drink at 6 o'clock. Do not tell the player about this. ");
            
            //UnityToGemini.Instance.currentCharacter.sanity -= SanityLoss(UnityToGemini.Instance.currentCharacter.characterType, interrogationType);
            string tempPrompt = OGPrompt;
            Backend.Content tempSystemInstruction = new Backend.Content();
            tempSystemInstruction.Parts = new List<Backend.Part>();
            //tempSystemInstruction.Parts.Add(new Backend.Part(currentCharacter.prompt));
            string tempUserInput = $"The player {interrogationType}ly asks you {question}, at  {timeDropdown} O'clock, about {objectToAsk}. You were " + ReturnStatement(int.Parse(timeDropdown)) + " respond accordingly.";
            //string tempUserInput = currentCharacter.prompt;
            geminiRequest.Contents.Add(BuildContent(Backend.GeminiRole.User, tempUserInput));
            geminiRequest.systemInstruction = tempSystemInstruction;
                    

            string jsonData = JsonConvert.SerializeObject(geminiRequest, new JsonSerializerSettings
            {
            NullValueHandling = NullValueHandling.Ignore
            });
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                
                // Send the request
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                    //resultText.text = www.error;
                    //UponWebError();
                    // Here is where we would relay to the user the reason as to why their key didn't validate (no tokens, wrong format, illegal key, etc)
                }
                else
                {
                    //TimeManager.Instance.DecrementActionCounter();
                    //Debug.Log("Action Counter: " + TimeManager.Instance.actionCounter);
                    //GeminiResponse response = UnpackGeminiResponse(www.downloadHandler.text);
                    //interrogationCanvas.GetComponent<InterrogationCanvas>().SetNPCResponse(CleanText(response.Candidates[0].Contents.Parts[0].Text));
                    //apiKey = apiInput.Trim();
                    //resultText.text = "API Key has been validated, please enter in a username.";
                    //playerUsernameInput.interactable = true;

                }
            }
        //return null;    
    }

    public IEnumerator SendRequestToGemini(string request)
    {

       Debug.Log("Started API Validation Request");
        string url = "";

        url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + apiKey;

        // Serialize the object to JSON
        Backend.GeminiRequest geminiRequest = new Backend.GeminiRequest();
        geminiRequest.Contents = new List<Backend.Content>();
        List<Backend.Part> tempParts = new List<Backend.Part>();
        Backend.Part tempPart = new Backend.Part("\"text\":\"" + request + "\"");
        tempParts.Add(tempPart);
        Backend.Content tempContent = new Backend.Content();
        tempContent.Role = "user";
        tempContent.Parts = tempParts;
        geminiRequest.Contents.Add(tempContent);

        string jsonData = JsonConvert.SerializeObject(geminiRequest, new JsonSerializerSettings
        {
        NullValueHandling = NullValueHandling.Ignore
        });
        Debug.Log("JSON DATA: " + jsonData);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            
            // Send the request
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                //resultText.text = www.error;
                //UponWebError();
                // Here is where we would relay to the user the reason as to why their key didn't validate (no tokens, wrong format, illegal key, etc)
            }
            else
            {
                GeminiResponseCallback?.Invoke(www.downloadHandler.text);
                //TimeManager.Instance.DecrementActionCounter();
                //Debug.Log("Action Counter: " + TimeManager.Instance.actionCounter);
                //apiKey = apiInput.Trim();
                //resultText.text = "API Key has been validated, please enter in a username.";
                //playerUsernameInput.interactable = true;

            }
        }
    }

    public IEnumerator SendKeyValidationToGemini(string apiInput)
    {
            Debug.Log("Started API Validation Request");
            string url = "";

            url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + apiInput;

            // Serialize the object to JSON
            Backend.GeminiRequest geminiRequest = new Backend.GeminiRequest();
            geminiRequest.Contents = new List<Backend.Content>();
            List<Backend.Part> tempParts = new List<Backend.Part>();
            Backend.Part tempPart = new Backend.Part("Test validation request");
            tempParts.Add(tempPart);
            Backend.Content tempContent = new Backend.Content();
            tempContent.Role = "user";
            tempContent.Parts = tempParts;
            geminiRequest.Contents.Add(tempContent);
    
            string jsonData = JsonConvert.SerializeObject(geminiRequest, Formatting.None);
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
    
                
                // Send the request
                yield return www.SendWebRequest();
    
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"API Key validation failed: {www.error}");
                    
                    // Trigger callback with error information so UI can be updated
                    GeminiResponseCallback?.Invoke("ERROR:" + www.error);
                }
                else
                {
                    // Set the API key and notify listeners that validation was successful
                    apiKey = apiInput.Trim();
                    Debug.Log("API Key validation successful");
                    
                    // Trigger the callback with the successful response
                    GeminiResponseCallback?.Invoke(www.downloadHandler.text);
                }
            }
    }

    public void UpdateQuestion(){
        //questionToAsk = interrogationCanvas.GetComponent<InterrogationCanvas>().questionDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().questionDropdown.value].text;
        //objectToAsk = interrogationCanvas.GetComponent<InterrogationCanvas>().objectDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().objectDropdown.value].text;
        //timeToAsk = interrogationCanvas.GetComponent<InterrogationCanvas>().timeDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().timeDropdown.value].text;
        //interrogationType = interrogationCanvas.GetComponent<InterrogationCanvas>().interrogationTypeDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().interrogationTypeDropdown.value].text;
    }

    public void UpdateObject(){
        //objectToAsk = interrogationCanvas.GetComponent<InterrogationCanvas>().objectDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().objectDropdown.value].text;
    }

    public void UpdateTime(){
        //timeToAsk = interrogationCanvas.GetComponent<InterrogationCanvas>().timeDropdown.options[interrogationCanvas.GetComponent<InterrogationCanvas>().timeDropdown.value].text;
    }

    public void UpdateInterrogationType(string type){
        interrogationType = type;
    }


    /// <summary>
    /// Get the content based on the Gemini role and the text (prompt).
    /// </summary>
    /// <param name="geminiRole">Gemini role in the request to Gemini.</param>
    /// <param name="text">Complete prompt in the request to Gemini.</param>
    /// <returns>Content object which could in AiRequest</returns>
    /// <exception cref="ArgumentOutOfRangeException">If non-existent role used, throw ArgumentOutOfRangeException</exception>
    private static Backend.Content BuildContent(Backend.GeminiRole geminiRole, string text)
    {
        Backend.Content tempContent = new Backend.Content();
        tempContent.Role = geminiRole.ToString();
        Backend.Part tempPart = new Backend.Part(text);
        tempContent.Parts = new List<Backend.Part>();
        tempContent.Parts.Add(tempPart);
        Debug.Log("Temp Part: " + tempPart.ToString());
        return tempContent;
        //return new Content
        //{
        //    Role = geminiRole.ToString()
        //    //switch
        //    //{
        //    //    GeminiRole.User => GeminiRole.User,
        //    //    GeminiRole.Model => GeminiRole.Model,
        //    //    _ => throw new ArgumentOutOfRangeException()
        //    //}
        //    ,
        //    Parts =
        //    {
        //        new Part(text)
        //        {
        //            Text = text,
        //        }
        //    }
        //};
    }

                /// <summary>
        /// Clean the text from the invalid patterns (here only invalid json format is applied).
        /// </summary>
        /// <param name="text">Text to be cleaned.</param>
        /// <returns>Cleaned text.</returns>
        private static string CleanText(string text)
        {
            foreach (var pattern in InvalidJsonFormatPattern)
            {
                text = text.Replace(pattern, string.Empty);
            }
            return text;
        }

        public GeminiResponse UnpackGeminiResponse(string rawResponse)
        {

            GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(rawResponse);
            if (geminiResponse != null && geminiResponse.UsageMetadata != null)
                Debug.Log(geminiResponse.UsageMetadata?.TotalTokenCount);
            CleanText(geminiResponse.Candidates[0].Contents.Parts[0].Text);
            return geminiResponse;
        }

        public void UpdateOGPrompt(){
            
        }
        
        public void JudgeCharacter(){

            // If the character is guilty, the player wins.

            // Else the time is advanced by an hour.
            Debug.Log("Judge Character");
        }

        public string ReturnStatement(int inquiryTime){
            string statement = "";
            int index = -1;
            //for(int i = 0; i < currentCharacter.Guidance.actions.Count; i++){
            //    if(inquiryTime == currentCharacter.Guidance.actions[i].time){
            //        index = i;
            //    }
            //}
            //
            //if(index != -1){
            //    if(currentCharacter.Guidance.actions[index].shouldLie == true && currentCharacter.sanity > 40.0f){
            //        statement = "This is a lie, make it believable: " + currentCharacter.Guidance.actions[index].theLie;
            //    }
            //    else{
            //        statement = "This is the truth: " + currentCharacter.Guidance.actions[index].theTruth;
            //    }
            //    if(currentCharacter.Guidance.actions[index].whoWith != null){
            //        statement = statement + " You were with " + currentCharacter.Guidance.actions[index].whoWith + " at the time of the inquiry.";
            //    }
            //}

            return statement;
        }

        // public float SanityLoss(Character.CharacterType characterType, string interrogationType){
        //     float sanityToLose = 0.0f;
        //     //switch(characterType){
        //     //    case Character.CharacterType.Aggressive:
        //     //    switch(interrogationType){
        //     //        case "Aggressive":
        //     //            sanityToLose = 5.0f;
        //     //        break;
        //     //
        //     //        case "Passive":
        //     //            sanityToLose = 7.0f;
        //     //        break;
        //     //
        //     //        case "Cunning":
        //     //            sanityToLose = 10.0f;
        //     //        break;
        //     //
        //     //        case "Friendly":
        //     //            sanityToLose = 15.0f;
        //     //        break;
        //     //    }
        //     //    break;
        //     //
        //     //    case Character.CharacterType.Passive:
        //     //    switch(interrogationType){
        //     //        case "Aggressive":
        //     //            sanityToLose = 15.0f;
        //     //        break;
        //     //
        //     //        case "Passive":
        //     //            sanityToLose = 5.0f;
        //     //        break;
        //     //
        //     //        case "Cunning":
        //     //            sanityToLose = 7.0f;
        //     //        break;
        //     //
        //     //        case "Friendly":
        //     //            sanityToLose = 10.0f;
        //     //        break;
        //     //    }
        //     //    break;
        //     //
        //     //    case Character.CharacterType.Cunning:
        //     //    switch(interrogationType){
        //     //        case "Aggressive":
        //     //            sanityToLose = 10.0f;
        //     //        break;
        //     //
        //     //        case "Passive":
        //     //            sanityToLose = 15.0f;
        //     //        break;
        //     //
        //     //        case "Cunning":
        //     //            sanityToLose = 5.0f;
        //     //        break;
        //     //
        //     //        case "Friendly":
        //     //            sanityToLose = 7.0f;
        //     //        break;
        //     //    }
        //     //    break;
        //     //
        //     //    case Character.CharacterType.Friendly:
        //     //    switch(interrogationType){
        //     //        case "Aggressive":
        //     //            sanityToLose = 7.0f;
        //     //        break;
        //     //
        //     //        case "Passive":
        //     //            sanityToLose = 10.0f;
        //     //        break;
        //     //
        //     //        case "Cunning":
        //     //            sanityToLose = 15.0f;
        //     //        break;
        //     //
        //     //        case "Friendly":
        //     //            sanityToLose = 5.0f;
        //     //        break;
        //     //    }
        //     //    break;
        //     //}
        //     
        //     return sanityToLose;
        // }
        
        private void HandleObjectClicked(GameObject clickedObject)
        {
            Debug.Log($"Clicked on: {clickedObject.name}");
        }

        private void HandleObjectHovered(GameObject hoveredObject)
        {
            Debug.Log($"Hovering over: {hoveredObject.name}");
        }

}

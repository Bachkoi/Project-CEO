using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] public TimeManager timeManager; // Reference to TimeManager
    
    private float baseScore;
    private float finalScore;
    private float timeModifier = 1.15f; // Increase score by 15% per day survived
    
    public float FinalScore => finalScore; // Public property to access the final score
    
    // Calculate the score based on current stock price and days survived
    public void CalculateScore()
    {
        //if (timeManager == null)
        //{
        //    Debug.LogError("TimeManager reference not set in ScoreManager!");
        //    return;
        //}
        float currentStockPrice = UnityToGemini.Instance.spDisplay.currentPrice;
        // Base score is the current stock price
        baseScore = currentStockPrice;
        
        // Calculate time modifier (increases with more days survived)
        float survivalModifier = Mathf.Pow(timeModifier, TimeManager.Instance.CurrentDay);
        
        // Calculate final score with time modifier
        finalScore = baseScore * survivalModifier;
        UnityToGemini.Instance.finalScoreTMP.text =
            $"Score Calculation - Base Score: {baseScore:F2} | Days Survived: {TimeManager.Instance.CurrentDay} | " +
            $"Time Modifier: {survivalModifier:F2} | Final Score: {finalScore:F2}";
        Debug.Log($"Score Calculation - Base Score: {baseScore:F2} | Days Survived: {TimeManager.Instance.CurrentDay} | " +
                  $"Time Modifier: {survivalModifier:F2} | Final Score: {finalScore:F2}");
    }
    
    // Reset the score (useful for new game)
    public void ResetScore()
    {
        baseScore = 0;
        finalScore = 0;
    }
}
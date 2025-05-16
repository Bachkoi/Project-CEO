// StockPriceDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class StockPriceDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stockSymbolText;
    [SerializeField] private TextMeshProUGUI currentPriceText;
    [SerializeField] private TextMeshProUGUI changePercentageText;
    [SerializeField] private RectTransform chartContainer;
    [SerializeField] private UILineRenderer lineRenderer;
    
    [SerializeField] private int maxDataPoints = 50;
    private List<StockPricePoint> priceHistory = new List<StockPricePoint>();

    private static StockPriceDisplay _instance;

    private float currentPrice;
    private string stockSymbol;
    private float chartMinPrice;
    private float chartMaxPrice;
    
    public float CurrentPrice => currentPrice;

    public static StockPriceDisplay Instance { get => _instance; set => _instance = value; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Initialize(string symbol, float initialPrice)
    {
        stockSymbol = symbol;
        currentPrice = initialPrice;
        priceHistory.Clear();
        AddPricePoint(initialPrice);
        UpdateDisplay();
    }

    public void UpdatePrice(float newPrice)
    {
        float changePercentage = ((newPrice - currentPrice) / currentPrice) * 100f;
        currentPrice = newPrice;
        AddPricePoint(newPrice);
        UpdateDisplay(changePercentage);
    }

    private void AddPricePoint(float price)
    {
        // Only using index for positioning, removing time dependency
        int newIndex = priceHistory.Count;
        priceHistory.Add(new StockPricePoint(price, 0, newIndex));

        // Keep only the last maxDataPoints
        if (priceHistory.Count > maxDataPoints)
        {
            priceHistory.RemoveAt(0);
            // Reindex the remaining points
            for (int i = 0; i < priceHistory.Count; i++)
            {
                priceHistory[i] = new StockPricePoint(
                    priceHistory[i].Price, 
                    0, 
                    i
                );
            }
        }

        UpdateChart();
    }

    private void UpdateChart()
    {
        if (priceHistory.Count < 2) return;

        // Find min and max prices for scaling
        chartMinPrice = float.MaxValue;
        chartMaxPrice = float.MinValue;

        foreach (var point in priceHistory)
        {
            if (point.Price < chartMinPrice) chartMinPrice = point.Price;
            if (point.Price > chartMaxPrice) chartMaxPrice = point.Price;
        }

        // Add 10% padding to min/max
        float pricePadding = (chartMaxPrice - chartMinPrice) * 0.1f;
        chartMinPrice -= pricePadding;
        chartMaxPrice += pricePadding;

        // Create points for the line renderer
        Vector2[] points = new Vector2[priceHistory.Count];
        Color32[] colors = new Color32[priceHistory.Count - 1];

        float chartWidth = chartContainer.rect.width;
        float chartHeight = chartContainer.rect.height;
        float maxIndex = maxDataPoints - 1;

        // Define colors
        Color32 increaseColor = new Color32(0, 255, 0, 255);
        Color32 decreaseColor = new Color32(255, 0, 0, 255);

        for (int i = 0; i < priceHistory.Count; i++)
        {
            var point = priceHistory[i];

            // Calculate normalized position (0-1 range)
            float normalizedX = point.Index / maxIndex;
            float normalizedY = (point.Price - chartMinPrice) / (chartMaxPrice - chartMinPrice);

            // Convert to actual coordinates within the chart container
            float x = normalizedX * chartWidth;
            float y = normalizedY * chartHeight;

            points[i] = new Vector2(x, y);

            if (i < priceHistory.Count - 1)
            {
                colors[i] = priceHistory[i + 1].Price >= point.Price ? increaseColor : decreaseColor;
            }
        }

        lineRenderer.points = points;
        lineRenderer.segmentColors = colors;
        lineRenderer.useSegmentColors = true;
        lineRenderer.SetAllDirty();
    }

    private void UpdateDisplay(float changePercentage = 0f)
    {
        stockSymbolText.text = stockSymbol;
        currentPriceText.text = $"${currentPrice:F2}";
        changePercentageText.text = $"{changePercentage:F2}%";
        changePercentageText.color = changePercentage >= 0 ? Color.green : Color.red;
    }
}
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
    
    private float currentPrice;
    private string stockSymbol;
    private float chartMinPrice;
    private float chartMaxPrice;
    private float chartTimeRange = 50f;
    
    public float CurrentPrice => currentPrice;

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
        float currentTime = Time.time;
        priceHistory.Add(new StockPricePoint(price, currentTime));

        // Keep only the last maxDataPoints
        if (priceHistory.Count > maxDataPoints)
        {
            priceHistory.RemoveAt(0);
        }

        UpdateChart();
    }

    private void UpdateChart()
    {
        if (priceHistory.Count < 2) return;

        // Find min and max prices for scaling
        chartMinPrice = float.MaxValue;
        chartMaxPrice = float.MinValue;
        float startTime = priceHistory[priceHistory.Count - 1].time - chartTimeRange;

        foreach (var point in priceHistory)
        {
            if (point.price < chartMinPrice) chartMinPrice = point.price;
            if (point.price > chartMaxPrice) chartMaxPrice = point.price;
        }

        // Add 10% padding to min/max
        float pricePadding = (chartMaxPrice - chartMinPrice) * 0.1f;
        chartMinPrice -= pricePadding;
        chartMaxPrice += pricePadding;

        // Create points for the line renderer
        Vector2[] points = new Vector2[priceHistory.Count];
        for (int i = 0; i < priceHistory.Count; i++)
        {
            var point = priceHistory[i];
            float x = ((point.time - startTime) / chartTimeRange) * chartContainer.rect.width;
            float y = Mathf.InverseLerp(chartMinPrice, chartMaxPrice, point.price) * chartContainer.rect.height;
            points[i] = new Vector2(x, y);
        }

        lineRenderer.points = points;
        lineRenderer.SetVerticesDirty();
    }

    private void UpdateDisplay(float changePercentage = 0f)
    {
        stockSymbolText.text = stockSymbol;
        currentPriceText.text = $"${currentPrice:F2}";
        changePercentageText.text = $"{changePercentage:F2}%";
        changePercentageText.color = changePercentage >= 0 ? Color.green : Color.red;
        lineRenderer.TestLineRenderer();
    }
}

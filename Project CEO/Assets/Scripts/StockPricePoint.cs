// StockPricePoint.cs
using UnityEngine;

[System.Serializable]
public class StockPricePoint
{
    public float price;
    public float time;

    public StockPricePoint(float price, float time)
    {
        this.price = price;
        this.time = time;
    }
}
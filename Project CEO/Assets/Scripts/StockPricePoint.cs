// StockPricePoint.cs
using UnityEngine;
using System;

/// <summary>
/// Represents a single price point of a stock at a specific time.
/// </summary>
public class StockPricePoint
{
    private float _price;
    private float _time;
    private int _index;

    /// <summary>
    /// Gets or sets the stock price.
    /// </summary>
    public float Price 
    {
        get => _price;
        set
        {
            if (value < 0)
                throw new ArgumentException("Price cannot be negative");
            _price = value;
        }
    }

    /// <summary>
    /// Gets or sets the time of the price point.
    /// </summary>
    public float Time
    {
        get => _time;
        set => _time = value;
    }

    /// <summary>
    /// Gets or sets the index of the price point.
    /// </summary>
    public int Index
    {
        get => _index;
        set
        {
            if (value < 0)
                throw new ArgumentException("Index cannot be negative");
            _index = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the StockPricePoint class.
    /// </summary>
    /// <param name="price">The stock price (must be non-negative)</param>
    /// <param name="time">The time of the price point</param>
    /// <param name="index">The index of the price point (must be non-negative)</param>
    /// <exception cref="ArgumentException">Thrown when price or index is negative</exception>
    public StockPricePoint(float price, float time, int index)
    {
        if (price < 0) throw new ArgumentException("Price cannot be negative");
        if (index < 0) throw new ArgumentException("Index cannot be negative");
        
        _price = price;
        _time = time;
        _index = index;
    }
}
public class StockPricePoint
{
    public float Price { get; private set; }
    public float Time { get; private set; }
    public int Index { get; private set; }

    public StockPricePoint(float price, float time, int index)
    {
        Price = price;
        Time = time;
        Index = index;
    }
}
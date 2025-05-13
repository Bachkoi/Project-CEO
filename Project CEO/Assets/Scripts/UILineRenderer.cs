// UILineRenderer.cs
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public float thickness = 2f;
    public Vector2[] points;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length < 2)
            return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 point = points[i];
            Vector2 next = points[i + 1];

            Vector2 direction = (next - point).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x);

            Vector2 v1 = point - normal * (thickness / 2);
            Vector2 v2 = point + normal * (thickness / 2);
            Vector2 v3 = next + normal * (thickness / 2);
            Vector2 v4 = next - normal * (thickness / 2);

            vh.AddUIVertexQuad(new UIVertex[] {
                GetVertex(v1, 0),
                GetVertex(v2, 1),
                GetVertex(v3, 1),
                GetVertex(v4, 0)
            });
        }
    }

    private UIVertex GetVertex(Vector2 point, float v)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = point;
        vertex.color = color;
        vertex.uv0 = new Vector2(0, v);
        return vertex;
    }
    
    // Add this code to test the line renderer
    public void TestLineRenderer()
    {
        StockPriceDisplay display = GetComponent<StockPriceDisplay>();
        display.Initialize("TEST", 100f);
    
        // Add some test price updates
        for(int i = 0; i < 10; i++)
        {
            float newPrice = 100f + Random.Range(-10f, 10f);
            display.UpdatePrice(newPrice);
        }
    }
}


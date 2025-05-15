using UnityEngine;

public class UILineRendererTest : MonoBehaviour
{
    private UILineRenderer lineRenderer;
    
    void Start()
    {
        lineRenderer = GetComponent<UILineRenderer>();
        
        // Set up test points
        Vector2[] testPoints = new Vector2[]
        {
            new Vector2(-100, -100),
            new Vector2(0, 100),
            new Vector2(100, -100)
        };
        
        lineRenderer.points = testPoints;
        lineRenderer.color = Color.red; // Set initial color
        lineRenderer.thickness = 10f;
        lineRenderer.SetAllDirty();
        
        Debug.Log($"Line Renderer Color: {lineRenderer.color}");
    }
    
    public void ToggleColor()
    {
        lineRenderer.color = (lineRenderer.color == Color.red) ? Color.green : Color.red;
        lineRenderer.SetAllDirty();
        Debug.Log($"Changed color to: {lineRenderer.color}");
    }
}
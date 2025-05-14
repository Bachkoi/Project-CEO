using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2[] points;
    public Color32[] segmentColors;
    public float thickness = 10f;
    //private bool useSegmentColors;
    public bool useSegmentColors;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length < 2)
            return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 point = points[i];
            Vector2 nextPoint = points[i + 1];

            Vector2 direction = (nextPoint - point).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x);

            Vector2 v1 = point - normal * (thickness * 0.5f);
            Vector2 v2 = point + normal * (thickness * 0.5f);
            Vector2 v3 = nextPoint + normal * (thickness * 0.5f);
            Vector2 v4 = nextPoint - normal * (thickness * 0.5f);

            // Get the current segment color
            Color segmentColor = useSegmentColors && segmentColors != null && i < segmentColors.Length 
                ? segmentColors[i] 
                : color;

            int vertCount = vh.currentVertCount;

            vh.AddVert(v1, segmentColor, Vector2.zero);
            vh.AddVert(v2, segmentColor, Vector2.zero);
            vh.AddVert(v3, segmentColor, Vector2.zero);
            vh.AddVert(v4, segmentColor, Vector2.zero);

            vh.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            vh.AddTriangle(vertCount + 2, vertCount + 3, vertCount + 0);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
    }

    public void SetAllDirty()
    {
        SetVerticesDirty();
        SetMaterialDirty();
    }
}
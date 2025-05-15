using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2[] points;
    public Color32[] segmentColors;
    public float thickness = 10f;
    public bool useSegmentColors;

    protected override void Awake()
    {
        base.Awake();
        // Ensure we're using the correct material
        if (material == null || material.shader.name != "UI/Default")
        {
            material = new Material(Shader.Find("UI/Default"));
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length < 2)
            return;

        Color32 vertexColor = (Color32)color;

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

            Color32 segmentColor = useSegmentColors && segmentColors != null && i < segmentColors.Length 
                ? segmentColors[i] 
                : vertexColor;

            int vertCount = vh.currentVertCount;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = segmentColor;

            vert.position = v1;
            vh.AddVert(vert);

            vert.position = v2;
            vh.AddVert(vert);

            vert.position = v3;
            vh.AddVert(vert);

            vert.position = v4;
            vh.AddVert(vert);

            vh.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            vh.AddTriangle(vertCount + 2, vertCount + 3, vertCount + 0);
        }
    }

    public void SetPoints(Vector2[] newPoints)
    {
        points = newPoints;
        SetVerticesDirty();
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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetAllDirty();
    }
#endif
}
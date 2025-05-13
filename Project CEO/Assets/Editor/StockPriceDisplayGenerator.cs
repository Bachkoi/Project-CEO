// Editor/StockPriceDisplayGenerator.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class StockPriceDisplayGenerator : EditorWindow
{
    [MenuItem("Tools/UI/Create Stock Price Display")]
    public static void ShowWindow()
    {
        GetWindow<StockPriceDisplayGenerator>("Stock Price Display Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Stock Price Display Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Stock Price Display"))
        {
            GenerateStockPriceDisplay();
        }
    }

    private void GenerateStockPriceDisplay()
    {
        // Find canvas in current selection
        Canvas parentCanvas = Selection.activeGameObject?.GetComponent<Canvas>();
        if (parentCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject with a Canvas component!", "OK");
            return;
        }

        // Create main container
        GameObject container = new GameObject("StockPriceDisplay");
        container.transform.SetParent(parentCanvas.transform, false);
        
        // Add components
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(500, 300);

        // Add background image
        Image backgroundImage = container.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);

        // Create stock symbol text
        GameObject symbolObj = CreateTextElement("SymbolText", container.transform, new Vector2(0, 120), "STOCK");

        // Create price text
        GameObject priceObj = CreateTextElement("PriceText", container.transform, new Vector2(0, 80), "$0.00");

        // Create percentage change text
        GameObject percentageObj = CreateTextElement("PercentageText", container.transform, new Vector2(0, 40), "0.00%");

        // Create chart container
        GameObject chartContainer = new GameObject("ChartContainer");
        chartContainer.transform.SetParent(container.transform, false);
        RectTransform chartRect = chartContainer.AddComponent<RectTransform>();
        chartRect.anchoredPosition = new Vector2(0, -50);
        chartRect.sizeDelta = new Vector2(460, 150);

        // Add chart background
        Image chartBg = chartContainer.AddComponent<Image>();
        chartBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Create line renderer for the chart
        GameObject lineObj = new GameObject("ChartLine");
        lineObj.transform.SetParent(chartContainer.transform, false);
        RectTransform lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.one;
        lineRect.offsetMin = Vector2.zero;
        lineRect.offsetMax = Vector2.zero;

        UILineRenderer lineRenderer = lineObj.AddComponent<UILineRenderer>();
        lineRenderer.color = Color.green;
        lineRenderer.thickness = 2f;
        lineRenderer.raycastTarget = false; // Disable raycast for better performance
        lineRenderer.material = new Material(Shader.Find("UI/Default")); // Add default UI material
        lineRenderer.points = new Vector2[] { 
            new Vector2(0, 0),
            new Vector2(chartRect.rect.width, chartRect.rect.height)
        }; // Set initial points using actual dimensions

        // Add StockPriceDisplay component
        StockPriceDisplay displayScript = container.AddComponent<StockPriceDisplay>();
        
        // Assign references using SerializedObject for proper serialization
        SerializedObject so = new SerializedObject(displayScript);
        so.FindProperty("stockSymbolText").objectReferenceValue = symbolObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("currentPriceText").objectReferenceValue = priceObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("changePercentageText").objectReferenceValue = percentageObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("chartContainer").objectReferenceValue = chartRect;
        so.FindProperty("lineRenderer").objectReferenceValue = lineRenderer;
        so.ApplyModifiedProperties();

        // Create the prefab
        string prefabPath = "Assets/Prefabs/StockPriceDisplay.prefab";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
        DestroyImmediate(container);

        EditorUtility.DisplayDialog("Success", "Stock Price Display prefab has been created at: " + prefabPath, "OK");
    }

    private GameObject CreateTextElement(string name, Transform parent, Vector2 position, string defaultText)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280, 30);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;

        return obj;
    }
}
using UnityEngine;
using UnityEngine.Events;

public class RaycastDetector : MonoBehaviour
{
    [System.Serializable]
    public class ObjectClickedEvent : UnityEvent<GameObject> { }

    [Header("Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask layerMask = Physics.DefaultRaycastLayers;
    [SerializeField] private bool detectOnHover = true;
    
    [Header("Mouse Buttons")]
    [SerializeField] private bool detectLeftClick = true;
    [SerializeField] private bool detectRightClick = false;
    [SerializeField] private bool detectMiddleClick = false;

    [Header("Events")]
    public ObjectClickedEvent onObjectClicked;
    public ObjectClickedEvent onObjectHovered;

    private GameObject lastHoveredObject;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Initialize events if they're null
        if (onObjectClicked == null)
            onObjectClicked = new ObjectClickedEvent();
        if (onObjectHovered == null)
            onObjectHovered = new ObjectClickedEvent();
    }

    private void Update()
    {
        // Check for clicks
        if (detectLeftClick && Input.GetMouseButtonDown(0))
            DetectObject();
        if (detectRightClick && Input.GetMouseButtonDown(1))
            DetectObject();
        if (detectMiddleClick && Input.GetMouseButtonDown(2))
            DetectObject();

        // Check for hover
        if (detectOnHover)
            DetectHover();
    }

    private void DetectObject()
    {
        GameObject hitObject = GetObjectUnderMouse();
        if (hitObject != null)
        {
            onObjectClicked.Invoke(hitObject);
        }
    }

    private void DetectHover()
    {
        GameObject hitObject = GetObjectUnderMouse();
        
        // Only invoke if the hovered object has changed
        if (hitObject != lastHoveredObject)
        {
            lastHoveredObject = hitObject;
            if (hitObject != null)
            {
                onObjectHovered.Invoke(hitObject);
                Debug.Log("Hit: " + hitObject.gameObject.name);
            }
        }
        else
        {
            Debug.Log("No Hit");
        }
    }

    public GameObject GetObjectUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            return hit.collider.gameObject;
        }

        return null;
    }

    // Get additional information about the hit
    public bool GetHitInfo(out RaycastHit hitInfo)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
    }
}
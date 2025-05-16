using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    private int currentCameraIndex = 0;
    public Camera currentCamera;
    public Canvas canvas;

    private void Start()
    {
        // Disable all cameras except the first one
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && i != currentCameraIndex)
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
    }

    public void SwitchCamera()
    {
        // Disable current camera
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].gameObject.SetActive(false);
        }

        // Move to next camera index
        currentCameraIndex++;
        // Loop back to first camera if we've reached the end
        if (currentCameraIndex >= cameras.Length)
        {
            currentCameraIndex = 0;
        }

        // Enable new current camera
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].gameObject.SetActive(true);
        }
        currentCamera = cameras[currentCameraIndex];
        //canvas.GetComponent<Camera>() = currentCamera;
        canvas.worldCamera = currentCamera;
    }

    public void SwitchToCamera(int index)
    {
        // Check if the index is valid
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning("Invalid camera index");
            return;
        }

        // Disable current camera
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].gameObject.SetActive(false);
        }

        // Set and enable new camera
        currentCameraIndex = index;
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].gameObject.SetActive(true);
        }
    }
}
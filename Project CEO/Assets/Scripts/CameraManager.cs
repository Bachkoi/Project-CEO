using UnityEngine;
using System;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField] private float transitionTime = 2.0f;
    private int currentCameraIndex = 0;
    public Camera currentCamera;
    public Canvas canvas;

    public Button[] buttons;
    

    [SerializeField] Vector3[] defaultPositions;
    //public Vector3[] defaultRotations;
    [SerializeField] float[] defaultFOVs;

    public static event Action<int> onChangeCamera;
    
    private void Start()
    {
        // Disable all cameras except the first one
        for (int i = 0; i < cameras.Length; i++)
        {
            
            if (cameras[i] != null && i != currentCameraIndex)
            {
                //defaultPositions[i] = cameras[i].transform.position;
                //defaultRotations[i] = cameras[i].transform.rotation;
                defaultFOVs[i] = cameras[i].fieldOfView;
                cameras[i].gameObject.SetActive(false);
            }
        }

        // Set initial camera
        if (cameras[currentCameraIndex] != null)
        {
            currentCamera = cameras[currentCameraIndex];
            canvas.worldCamera = currentCamera;
        }
        EnableButtons(0);
    }

    public void SwitchCamera()
    {
        StartCoroutine(TransitionToNextCamera());
    }

    private IEnumerator TransitionToNextCamera()
    {
        int previousCameraIndex = currentCameraIndex;
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;

        Camera prevCamera = cameras[previousCameraIndex];
        Camera nextCamera = cameras[currentCameraIndex];

        // Enable next camera but make it fully transparent
        if (nextCamera != null)
        {
            nextCamera.gameObject.SetActive(true);
            nextCamera.depth = prevCamera.depth - 1; // Ensure it renders behind current camera
        }

        float elapsedTime = 0;
        Vector3 startPos = prevCamera.transform.position;
        Quaternion startRot = prevCamera.transform.rotation;
        float startFOV = prevCamera.fieldOfView;

        Vector3 endPos = nextCamera.transform.position;
        Quaternion endRot = nextCamera.transform.rotation;
        float endFOV = nextCamera.fieldOfView;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            
            // Smoothly interpolate the previous camera's transform
            prevCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            prevCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            prevCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable previous camera and ensure next camera is properly set
        if (prevCamera != null)
        {
            prevCamera.gameObject.SetActive(false);
        }

        if (nextCamera != null)
        {
            nextCamera.depth = prevCamera.depth; // Restore depth
            currentCamera = nextCamera;
            canvas.worldCamera = currentCamera;
        }
        CameraReset(prevCamera, previousCameraIndex);
        EnableButtons(currentCameraIndex);

        onChangeCamera?.Invoke(currentCameraIndex);
    }

    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning("Invalid camera index");
            return;
        }

        StartCoroutine(TransitionToCamera(index));
    }

    private IEnumerator TransitionToCamera(int newIndex)
    {
        int previousIndex = currentCameraIndex;
        currentCameraIndex = newIndex;

        Camera prevCamera = cameras[previousIndex];
        Camera nextCamera = cameras[currentCameraIndex];

        // Enable next camera but make it fully transparent
        if (nextCamera != null)
        {
            nextCamera.gameObject.SetActive(true);
            nextCamera.depth = prevCamera.depth - 1;
        }

        float elapsedTime = 0;
        Vector3 startPos = prevCamera.transform.position;
        Quaternion startRot = prevCamera.transform.rotation;
        float startFOV = prevCamera.fieldOfView;

        Vector3 endPos = nextCamera.transform.position;
        Quaternion endRot = nextCamera.transform.rotation;
        float endFOV = nextCamera.fieldOfView;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            
            // Smoothly interpolate the previous camera's transform
            prevCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            prevCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            prevCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable previous camera and ensure next camera is properly set
        if (prevCamera != null)
        {
            prevCamera.gameObject.SetActive(false);
        }

        if (nextCamera != null)
        {
            nextCamera.depth = prevCamera.depth;
            currentCamera = nextCamera;
            canvas.worldCamera = currentCamera;
        }
        CameraReset(cameras[previousIndex], previousIndex);
        EnableButtons(currentCameraIndex);
        onChangeCamera?.Invoke(currentCameraIndex);
    }

    public void CameraReset(Camera pCam, int pIndex)
    {
        Debug.Log("PIndex: " + pIndex);
        Debug.Log("PCam: " + pCam.name);
        pCam.transform.position = defaultPositions[pIndex];
        //pCam.transform.rotation = defaultRotations[pIndex];
        pCam.fieldOfView = defaultFOVs[pIndex];
    }

    public void EnableButtons(int pIndex)
    {
        Debug.Log("Enabling Buttons for: " + pIndex);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == pIndex * 3 || i == pIndex * 3 + 1 || i == pIndex * 3 + 2)
            {
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);

            }
        }
    }
}
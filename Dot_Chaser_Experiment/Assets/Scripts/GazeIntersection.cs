using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

public class GazeIntersection : MonoBehaviour
{
    public bool debugMode = false; // Toggle for debug mode
    public GameObject dotPrefab; // Prefab for the dot (color depends on the eye)
    public ExperimentEyeGaze eyeGaze; // Reference to the ExperimentEyeGaze component

    private void Start()
{
    Debug.Log("GazeIntersection script Start method called.");
    
    // Check if the ExperimentEyeGaze component is found
    if (eyeGaze == null)
    {
        Debug.LogError("ExperimentEyeGaze component not assigned.");
    }
    else
    {
        Debug.Log("GazeIntersection script started, eye gaze object: " + eyeGaze);
    }
}

private void Update()
{
    if (eyeGaze.EyeTrackingEnabled && eyeGaze.Confidence > eyeGaze.ConfidenceThreshold)
    {
        Vector3 gazeOrigin = eyeGaze.transform.position;
        Vector3 gazeDirection = eyeGaze.transform.TransformDirection(Vector3.forward);

        Debug.Log("Gaze Origin: " + gazeOrigin);
        Debug.Log("Gaze Direction: " + gazeDirection);

        // Perform a raycast for the gaze direction
        PerformRaycast(gazeOrigin, gazeDirection, dotPrefab);
    }
}

private void PerformRaycast(Vector3 origin, Vector3 gazeDirection, GameObject dotPrefab)
{
    RaycastHit hit;
    if (Physics.Raycast(origin, gazeDirection, out hit))
    {
        if (hit.collider.CompareTag("TrackedObject"))
        {
            Vector3 hitPoint = hit.point;
            RecordHitPoint(hitPoint);

            if (debugMode)
            {
                GameObject dotInstance = Instantiate(dotPrefab, hitPoint, Quaternion.identity);
                Destroy(dotInstance, 0.10f); // Destroy the dot instance after 0.10 seconds
            }
        }
    }
}

private void RecordHitPoint(Vector3 hitPoint)
{
    Debug.Log("Hit Point: " + hitPoint);
}
}
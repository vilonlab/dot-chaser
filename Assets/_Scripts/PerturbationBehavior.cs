using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerturbationBehavior : MonoBehaviour
{
    public GameObject groundPlane; // The ground plane GameObject
    public GameObject dot; // The sphere (dot) GameObject
    public Camera hmd; // The headset's camera (CenterEyeAnchor)

    public float triggerDistance = 4.0f; // Distance from the dot to trigger the perturbation
    public float translationSpeed = 1.0f; // Speed of translation (m/s)
    public float rotationSpeed = 5.0f; // Speed of rotation (degrees/s)
    public float translationDistance = 0.1f; // Total translation distance (10 cm)
    public float rotationAngle = 5.0f; // Total rotation angle (5 degrees)

    private bool isPerturbing = false;
    private string currentTrialType = ""; // Control or experimental
    private Vector3 initialGroundPlanePosition;
    private Quaternion initialGroundPlaneRotation;
    private float perturbationProgress = 0.0f;

    void Awake(){

        hmd = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        if (hmd == null)
        {
            Debug.LogError("PerturbationBehavior: CenterEyeAnchor not found.");
        }

        groundPlane = GameObject.FindGameObjectWithTag("GroundPlane");  
        if (groundPlane == null)
        {
            Debug.LogError("PerturbationBehavior: GroundPlane not found.");
        }

        dot = GameObject.Find("TargetSphere");
    }
    
    void Start()
    {
        if (groundPlane == null || dot == null || hmd == null)
        {
            Debug.LogError("PerturbationBehavior: Missing required GameObject references.");
        }

        initialGroundPlanePosition = groundPlane.transform.position;
        initialGroundPlaneRotation = groundPlane.transform.rotation;
    }

    void Update()
    {
        if (isPerturbing)
        {
            ApplyPerturbation();
        }
        else
        {
            CheckTriggerCondition();
        }
    }

    public void StartPerturbation(string trialType)
    {
        currentTrialType = trialType;
        isPerturbing = true;
        perturbationProgress = 0.0f;

        // Dynamically capture the ground plane's position and rotation at the start of the perturbation
        initialGroundPlanePosition = groundPlane.transform.position;
        initialGroundPlaneRotation = groundPlane.transform.rotation;
    }

    private void CheckTriggerCondition()
{
    // Calculate the distance between the headset and the dot
    Vector3 headsetPosition = hmd.transform.position;
    Vector3 dotPosition = dot.transform.position;
    float distance = Vector3.Distance(headsetPosition, dotPosition);

    // Trigger perturbation if within the trigger distance
    if (distance <= triggerDistance)
    {
        ExperimentManager experimentManager = FindObjectOfType<ExperimentManager>();
        if (experimentManager != null)
        {
            if (experimentManager.AreAllTrialsCompleted()) // Check if all trials are completed, if so, stop
            {
                Debug.Log("All trials have been completed.");
                // TODO: Need a way to tell the participant that all trials have been completed
                return; // Stop further processing
            }
            TrialInfo currentTrial = experimentManager.GetTrialInfo(experimentManager.trialNumber);
            Debug.Log($"Current trial number: {experimentManager.trialNumber}");
            Debug.Log($"Current trial type: {currentTrial?.TrialType}");

            if (currentTrial != null)
            {
                switch (currentTrial.TrialType)
                {
                    case "Div":
                        StartPerturbation("translate_towards");
                        break;

                    case "Conv":
                        StartPerturbation("translate_away");
                        break;

                    case "RotL":
                        StartPerturbation("rotate_left");
                        break;

                    case "RotR":
                        StartPerturbation("rotate_right");
                        break;

                    case "Control":
                        Debug.Log("Control trial: No perturbation applied.");
                        break;

                    default:
                        Debug.LogWarning($"Unknown trial type: {currentTrial.TrialType}");
                        break;
                }
            }
            else
            {
                Debug.LogError("Current trial information is null.");
            }
        }
        else
        {
            Debug.LogError("ExperimentManager not found in the scene.");
        }
    }
}

    private void ApplyPerturbation()
    {
        Vector3 headsetPosition = hmd.transform.position;
        Vector3 dotPosition = dot.transform.position;
        Vector3 direction = (headsetPosition - dotPosition).normalized;

        float deltaTime = Time.deltaTime;

        switch (currentTrialType)
        {
            case "translate_towards":
                TranslateGroundPlane(direction, deltaTime);
                Debug.Log("Divergence Perturbation: Translating towards the headset.");
                break;

            case "translate_away":
                TranslateGroundPlane(-direction, deltaTime);
                Debug.Log("Convergence Perturbation: Translating away from the headset.");
                break;

            case "rotate_right":
                RotateGroundPlane(direction, rotationSpeed * deltaTime);
                Debug.Log("Rotation Perturbation: Rotating right.");
                break;

            case "rotate_left":
                RotateGroundPlane(-direction, rotationSpeed * deltaTime);
                Debug.Log("Rotation Perturbation: Rotating left.");
                break;

            default:
                Debug.LogWarning("PerturbationBehavior: Unknown trial type.");
                isPerturbing = false;
                break;
        }
    }

    private void TranslateGroundPlane(Vector3 direction, float deltaTime)
    {
        float step = translationSpeed * deltaTime;
        perturbationProgress += step;

        if (perturbationProgress >= translationDistance)
        {
            step -= (perturbationProgress - translationDistance);
            isPerturbing = false;

            // Reset the ground plane after the perturbation is complete
            ResetGroundPlane();
        }

        groundPlane.transform.position += direction * step;
    }

    private void RotateGroundPlane(Vector3 axis, float deltaAngle)
    {
        perturbationProgress += deltaAngle;

        if (perturbationProgress >= rotationAngle)
        {
            deltaAngle -= (perturbationProgress - rotationAngle);
            isPerturbing = false;

            // Reset the ground plane after the perturbation is complete
            ResetGroundPlane();
        }

        groundPlane.transform.RotateAround(dot.transform.position, axis, deltaAngle);
    }

    public void ResetGroundPlane()
    {
        groundPlane.transform.position = initialGroundPlanePosition;
        groundPlane.transform.rotation = initialGroundPlaneRotation;
        isPerturbing = false;
        perturbationProgress = 0.0f;
    }
}
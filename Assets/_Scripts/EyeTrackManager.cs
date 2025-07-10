using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackManager : MonoBehaviour{

    // Constants and state parameters and stuff
    private ExperimentManager expMgr;
    public float IPD; // in mm
    public float FOV = 110.0f;
    public const int HISTORY_BUFFER_SIZE = 10;
    private const float EYE_CLOSED_THRESHOLD = 0.85f; // Chosen based on 10 mins of testing on myself
    private const float BLINK_DURATION_THRESHOLD = 150.0f; // TODO: check if this value is right
    private const float SACCADE_VELOCITY_THRESHOLD = 180.0f; // degrees. From Qi Sun's paper
    public enum GazeState { STILL, MOVING, CLOSED, SACCADE };
    public GazeState curGazeState;
    public GazeState prevGazeState;
    private List<float> bufferWeights; // for weighted averages

    // Gaze data
    GazeManager leftGazeMgr;
    GazeManager rightGazeMgr;
    public Vector3 leftGazePos;
    public Vector3 rightGazePos;
    public Vector3 centerGazePos;
    public Vector3 prevCenterGazePos;
    public Vector3 leftGazeDir;
    public Vector3 rightGazeDir;
    public Vector3 centerGazeDir;
    public Vector3 prevCenterGazeDir;
    public float angularVelocity;
    public float weightedAngularVelocity = 0.0f;
    public float gazeAngleChange;
    public float gazeAngularVelocity;
    public float leftEyeClosed;
    public float rightEyeClosed;
    public float eyesClosedTimer;
    // Gaze data history
    public CircularBuffer<Vector3> gazePosHistory;
    public CircularBuffer<Vector3> gazeDirHistory;
    public CircularBuffer<float> angleChangeHistory; // Based on the center gaze position
    public CircularBuffer<float> angularVelocityHistory; // Based on the center gaze position
    public CircularBuffer<float> weightedAngularVelocityHistory; // Based on the center gaze position
    public CircularBuffer<float> deltaTimeHistory;

    // Gaze target object stuff
    RaycastHit gazeHit;
    GameObject gazeHitObj;

    private const float RECORDING_WAIT_TIMER = 2.0f;
    float elapsedTime = 0.0f;
    bool startRecording;

    // https://developer.oculus.com/reference/unity/v46/class_o_v_r_eye_gaze/
    // https://note.com/npaka/n/n3761152ae06c
    // https://www.youtube.com/watch?v=ZoySn7QlMfQ
    void Start(){
        expMgr = GameObject.Find("OVRCameraRig").GetComponent<ExperimentManager>();
        leftGazeMgr = GameObject.Find("LeftGazeManager").GetComponent<GazeManager>();
        rightGazeMgr = GameObject.Find("RightGazeManager").GetComponent<GazeManager>();
        bufferWeights = new List<float>();
        for (int i = 0; i < HISTORY_BUFFER_SIZE; i++){
            bufferWeights.Add((float)i / (float)HISTORY_BUFFER_SIZE);
        }

        gazePosHistory = new CircularBuffer<Vector3>(HISTORY_BUFFER_SIZE);
        gazeDirHistory = new CircularBuffer<Vector3>(HISTORY_BUFFER_SIZE);
        angleChangeHistory = new CircularBuffer<float>(HISTORY_BUFFER_SIZE);
        angularVelocityHistory = new CircularBuffer<float>(10);
        weightedAngularVelocityHistory = new CircularBuffer<float>(HISTORY_BUFFER_SIZE);
        deltaTimeHistory = new CircularBuffer<float>(HISTORY_BUFFER_SIZE);
    }

    // Update is called once per frame
    void LateUpdate(){
        if (elapsedTime < RECORDING_WAIT_TIMER){
            elapsedTime += Time.deltaTime;
            startRecording = elapsedTime >= RECORDING_WAIT_TIMER;
            return;
        }

        // Eye closed/open
        leftEyeClosed = leftGazeMgr.eyeClosedness;
        rightEyeClosed = rightGazeMgr.eyeClosedness;

        // Gaze position and direction
        // leftGazePos = expMgr.VE.transform.InverseTransformPoint(leftGazeMgr.gazeTarget.transform.position);
        leftGazePos = leftGazeMgr.gazeTarget.transform.position;
        leftGazeDir = leftGazeMgr.gazeDir;
        //print(leftGazeDir);
        //print("***************");
        // rightGazePos = expMgr.VE.transform.InverseTransformPoint(rightGazeMgr.gazeTarget.transform.position);
        rightGazePos = rightGazeMgr.gazeTarget.transform.position;
        rightGazeDir = rightGazeMgr.gazeDir;
        centerGazePos = (leftGazePos + rightGazePos) / 2.0f;
        // TODO: FIXME: TODO: FIXME: TODO: FIXME: TODO: FIXME: TODO: FIXME:
        // MAKE SURE THAT GAZE POSITION IS IN LOCAL COORDINATE FRAME OF THE VIRT ENV
        // TODO: FIXME: TODO: FIXME: TODO: FIXME: TODO: FIXME: TODO: FIXME:
        centerGazeDir = Vector3.Normalize(centerGazePos - expMgr.curHMDState.virtPos);

        // Angular velocity
        gazeAngleChange = Vector3.Angle(prevCenterGazeDir, centerGazeDir);

        if (startRecording){
            gazePosHistory.PushBack(centerGazePos);
            gazeDirHistory.PushBack(centerGazeDir);
            deltaTimeHistory.PushBack(Time.deltaTime);
            angleChangeHistory.PushBack(gazeAngleChange);

            float deltaTimeSum = 0.0f;
            float angleChangeSum = 0.0f;
            for (int i = 0; i < deltaTimeHistory._size; i++){
                deltaTimeSum += deltaTimeHistory[i];
                angleChangeSum += angleChangeHistory[i];
            }
            angularVelocity = angleChangeSum / deltaTimeSum;
            angularVelocity = gazeAngleChange / Time.deltaTime;
            angularVelocityHistory.PushBack(angularVelocity);
            float angularVelocitySum = 0.0f;
            for (int i = 0; i < angularVelocityHistory._size; i++){
                angularVelocitySum += angularVelocityHistory[i];
            }
            weightedAngularVelocity = angularVelocitySum / angularVelocityHistory._size;
            weightedAngularVelocityHistory.PushBack(weightedAngularVelocity);
        }

        UpdateGazeState();
        CastGazeRay();
        DrawGazeInfo();
        prevCenterGazePos = centerGazePos;
        prevCenterGazeDir = centerGazeDir;
    }

    void UpdateGazeState(){
        // Qi sun saccade https://dl.acm.org/doi/pdf/10.1145/3197517.3201294
        // Bolte saccade https://ieeexplore.ieee.org/abstract/document/7010955
        // Li saccade length https://link.springer.com/content/pdf/10.3758/BF03211589.pdf
        // Nguyen blinks https://dl.acm.org/doi/pdf/10.1145/3281505.3281515
        // Langbehn blinks https://dl.acm.org/doi/pdf/10.1145/3197517.3201335
        // smooth saccade detection https://www.sciencedirect.com/science/article/pii/S0141938212000777
        prevGazeState = curGazeState;
        // Eyes closed
        if (leftEyeClosed >= EYE_CLOSED_THRESHOLD && rightEyeClosed >= EYE_CLOSED_THRESHOLD){
            curGazeState = GazeState.CLOSED;
            eyesClosedTimer += Time.deltaTime;
            return;
        }

        // Eyes open
        eyesClosedTimer = 0.0f;
        if (weightedAngularVelocity >= SACCADE_VELOCITY_THRESHOLD){
            curGazeState = GazeState.SACCADE;
        }
        else if (weightedAngularVelocity >= 10.0f){
            curGazeState = GazeState.MOVING;
        }
        else{
            curGazeState = GazeState.STILL;
        }
    }

    void CastGazeRay(){
        Vector3 hmdPos = expMgr.curHMDState.virtPos;
        // int layerMask = ~(LayerMask.NameToLayer("Ignore Raycast"));
        int layerMask = LayerMask.NameToLayer("Raycast hit");
        // if (Physics.Raycast(hmdPos, centerGazeDir, out gazeHit, Mathf.Infinity, layerMask)){
        if (Physics.Raycast(hmdPos, centerGazeDir, out gazeHit, Mathf.Infinity)){
            gazeHitObj = gazeHit.collider.gameObject;
        }
        else{
            gazeHitObj = null;
        }
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(centerGazePos, 0.1f);
    }

    void DrawGazeInfo(){
        Vector3 virtHeading = expMgr.curHMDState.virtHeading;
        Debug.DrawRay(expMgr.curHMDState.virtPos, centerGazeDir * (expMgr.curHMDState.virtPos - centerGazePos).magnitude, Color.yellow);

        // Gaze vectors
        Debug.DrawRay(GameObject.Find("LeftEyeAnchor").transform.position, leftGazeDir * (GameObject.Find("LeftEyeAnchor").transform.position - leftGazePos).magnitude, Color.green);
        Debug.DrawRay(GameObject.Find("RightEyeAnchor").transform.position, rightGazeDir * (GameObject.Find("RightEyeAnchor").transform.position - rightGazePos).magnitude, Color.green);

        // Limits of user FOV
        Vector3 pos = GameObject.Find("CenterEyeAnchor").transform.position;
        Vector3 dir = GameObject.Find("CenterEyeAnchor").transform.forward;
        Debug.DrawRay(pos, (Quaternion.AngleAxis(FOV/2.0f, Vector3.up) * dir) * 10.0f, Color.red);
        Debug.DrawRay(pos, (Quaternion.AngleAxis(FOV/-2.0f, Vector3.up) * dir) * 10.0f, Color.red);
    }
}

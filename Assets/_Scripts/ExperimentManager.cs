using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.TestTools;
using UnityEditor;

public class HMDState{
    public Vector3 virtPos;
    public Vector3 physPos;
    public Vector3 virtHeading;
    public Vector3 physHeading;
    public float virtHeight;
    public float physHeight;

    public HMDState(Vector3 virtPosIn, Vector3 physPosIn, Vector3 virtHeadingIn, Vector3 physHeadingIn){
        virtPos = virtPosIn;
        physPos = physPosIn;
        virtHeading = virtHeadingIn;
        physHeading = physHeadingIn;
        virtHeight = virtPos.y;
        physHeight = physPos.y;
    }
}

public class ExperimentManager : MonoBehaviour{

    // Position stuff
    public GameObject cameraRig;
    public Camera hmd;
    public HMDState curHMDState;
    public HMDState prevHMDState = null;
    public GameObject VE;
    public GameObject PE;
    private GameObject VEOrigin;
    public Vector2 deltaPos;
    public float deltaHeading; // in radians
    public float deltaHeadingDeg; // in degrees
    public EyeTrackManager eyeMgr;
    public CircularBuffer<float> deltaPosHistory;
    public CircularBuffer<float> deltaTimeHistory;
    public float avgSpeed;
    public GameObject leftHand;
    public GameObject rightHand;

    // Logging stuff
    public DataLogger dataLogger;
    public int participantID = 0;
    public int trialNumber = 1;
    public float curTrialTime = 0.0f;

    void Awake(){
        cameraRig = GameObject.Find("OVRCameraRig");
        hmd = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        leftHand = GameObject.Find("LeftHandAnchor");
        rightHand = GameObject.Find("RightHandAnchor");
        VE = GameObject.Find("Virtual Env");
        PE = GameObject.Find("TrackingSpace");
        deltaPosHistory = new CircularBuffer<float>(20);
        deltaTimeHistory = new CircularBuffer<float>(20);
        eyeMgr = GameObject.Find("OVRCameraRig").GetComponent<EyeTrackManager>();

        dataLogger = new DataLogger(DataLogger.GetCurrentPath() + "/_Data/test.csv");
        dataLogger.WriteLine("virt_pos_x,virt_pos_y,virt_pos_z,phys_pos_x,phys_pos_y,phys_pos_z,virt_heading_x,virt_heading_y,virt_heading_z,phys_heading_x,phys_heading_y,phys_heading_z,cyclopean_gaze_pos_x,cyclopean_gaze_pos_y,cyclopean_gaze_pos_z,cyclopean_gaze_dir_x,cyclopean_gaze_dir_y,cyclopean_gaze_dir_z,left_gaze_dir_x,left_gaze_dir_y,left_gaze_dir_z,right_gaze_dir_x,right_gaze_dir_y,right_gaze_dir_z,cyclopean_gaze_angular_velocity,weighted_gaze_angular_velocity,left_eye_closed,right_eye_closed,gaze_state,timestamp,frame_number,unity_delta_time");        
    }

    // Start is called before the first frame update
    void Start(){
        
    }

    void FixedUpdate(){
        curHMDState = GetHMDState();
        UpdatePositionData();
        HandleKeyboardInput();
        prevHMDState = curHMDState;

        print(curHMDState.virtHeading.ToString("F5"));
        
        curTrialTime += Time.fixedDeltaTime;
    }

    void Update(){
    }

    void LateUpdate(){
        LogData();
    }

    void LogData(){
        string data = "";
        Vector3 pePos = curHMDState.physPos;
        Vector3 vePos = curHMDState.virtPos;
        Vector3 peHeading = curHMDState.physHeading;
        Vector3 veHeading = curHMDState.virtHeading;
        data += vePos.x.ToString("F10") + ","; // virt pos
        data += vePos.y.ToString("F10") + ","; // virt pos
        data += vePos.z.ToString("F10") + ","; // virt pos
        data += pePos.x.ToString("F10") + ","; // phys pos
        data += pePos.y.ToString("F10") + ","; // phys pos
        data += pePos.z.ToString("F10") + ","; // phys pos
        data += veHeading.x.ToString("F10") + ","; // virt heading
        data += veHeading.y.ToString("F10") + ","; // virt heading
        data += veHeading.z.ToString("F10") + ","; // virt heading
        data += peHeading.x.ToString("F10") + ","; // phys_heading
        data += peHeading.y.ToString("F10") + ","; // phys_heading
        data += peHeading.z.ToString("F10") + ","; // phys_heading
        data += eyeMgr.centerGazePos.x.ToString("F10") + ","; // gaze pos
        data += eyeMgr.centerGazePos.y.ToString("F10") + ","; // gaze pos
        data += eyeMgr.centerGazePos.z.ToString("F10") + ","; // gaze pos
        data += eyeMgr.centerGazeDir.x.ToString("F10") + ","; // gaze dir
        data += eyeMgr.centerGazeDir.y.ToString("F10") + ","; // gaze dir
        data += eyeMgr.centerGazeDir.z.ToString("F10") + ","; // gaze dir
        data += eyeMgr.leftGazeDir.x.ToString("F10") + "," ; // left_gaze_dir_x
        data += eyeMgr.leftGazeDir.y.ToString("F10") + "," ; // left_gaze_dir_y
        data += eyeMgr.leftGazeDir.z.ToString("F10") + "," ; // left_gaze_dir_z
        data += eyeMgr.rightGazeDir.x.ToString("F10") + "," ; // right_gaze_dir_x
        data += eyeMgr.rightGazeDir.y.ToString("F10") + "," ; // right_gaze_dir_y
        data += eyeMgr.rightGazeDir.z.ToString("F10") + "," ; // right_gaze_dir_z
        data += eyeMgr.angularVelocity.ToString("F10") + ","; // gaze angular velocity
        data += eyeMgr.weightedAngularVelocity + ","; // weighted gaze angular velocity
        data += eyeMgr.leftEyeClosed.ToString("F10") + ","; // left eye closed
        data += eyeMgr.rightEyeClosed.ToString("F10") + ","; // right eye closed
        data += eyeMgr.curGazeState.ToString() + ","; // right eye closed

        data += System.DateTime.Now.ToString("yyyy_mm_dd|hh\\:mm\\:ss\\.ff") + ","; // timestamp
        data += Time.frameCount + ","; // frame number
        data += Time.deltaTime.ToString(); // unity_delta_time
        dataLogger.WriteLine(data);
    }
    
    void HandleKeyboardInput(){
        
    }

    void UpdatePositionData(){
        if (prevHMDState == null){
            deltaHeading = 0f;
            deltaHeadingDeg = 0.0f;
            deltaPos = new Vector2(0f, 0f);
        }
        else{
            deltaHeadingDeg = Vector2.SignedAngle(new Vector2(prevHMDState.virtHeading.x, prevHMDState.virtHeading.z), new Vector2(curHMDState.virtHeading.x, curHMDState.virtHeading.z));
            deltaHeading = deltaHeadingDeg * Mathf.Deg2Rad;
            Vector3 tempDeltaPos = curHMDState.virtPos - prevHMDState.virtPos;
            deltaPos = new Vector2(tempDeltaPos.x, tempDeltaPos.z);
        }

        deltaTimeHistory.PushBack(Time.deltaTime);
        deltaPosHistory.PushBack(deltaPos.magnitude);
        float deltaPosSum = 0.0f;
        float deltaTimeSum = 0.0f;
        for (int i = 0; i < deltaTimeHistory._size; i++){
            deltaTimeSum += deltaTimeHistory[i];
            deltaPosSum += deltaPosHistory[i];
        }
        avgSpeed = deltaPosSum / deltaTimeSum;
    }

    private HMDState GetHMDState(){
        Vector3 curPos = hmd.transform.position;
        Vector3 curForward = hmd.transform.forward;

        Vector3 virtPosGlobal = VE.transform.InverseTransformPoint(curPos);
        Vector3 virtForwardGlobal = VE.transform.InverseTransformDirection(curForward);
        Vector3 physPosGlobal;
        Vector3 physForwardGlobal;
        physPosGlobal = PE.transform.InverseTransformPoint(curPos);
        physForwardGlobal = PE.transform.InverseTransformDirection(curForward);
        
        return new HMDState(virtPosGlobal, physPosGlobal, virtForwardGlobal, physForwardGlobal);
    }
}

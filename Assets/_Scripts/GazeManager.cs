using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeManager : MonoBehaviour{

    OVREyeGaze eyeGaze;
    public OVRFaceExpressions faceAPI;
    public GameObject hmd;
    public Vector3 gazeDir;
    public GameObject gazeTarget;
    public float eyeClosedness; // 1 == eye closed, 0 == eye open
    public bool isEyeOpen;
    private bool isLeft;

    // Start is called before the first frame update
    void Start(){
        hmd = GameObject.Find("CenterEyeAnchor");
        eyeGaze = gameObject.GetComponent<OVREyeGaze>();
        faceAPI = gameObject.GetComponent<OVRFaceExpressions>();
        isLeft = gameObject.name.Contains("Left");
        // gazeTarget.GetComponent<Renderer>().enabled = true;
    }

    // Update is called once per frame
    void FixedUpdate(){
        // if (isLeft) eyeClosedness = faceAPI[OVRFaceExpressions.FaceExpression.EyesClosedL];
        // else        eyeClosedness = faceAPI[OVRFaceExpressions.FaceExpression.EyesClosedR];

        // if (isLeft) eyeClosedness = eyeGaze.GetLeftEyeOpenness;
        // else        eyeClosedness = faceAPI[OVRFaceExpressions.FaceExpression.EyesClosedR];

        float LBlinkWeight = -1.0f;
        float RBlinkWeight = -1.0f;
        if (isLeft) isEyeOpen = faceAPI.TryGetFaceExpressionWeight(OVRFaceExpressions.FaceExpression.EyesClosedL, out LBlinkWeight);
        else        isEyeOpen = faceAPI.TryGetFaceExpressionWeight(OVRFaceExpressions.FaceExpression.EyesClosedR, out RBlinkWeight);

        // print("eye open");
        // print(isEyeOpen);
        // print("=========");

        if (eyeGaze == null){
            print("Failed to get gaze for " + gameObject.name);
        }
        else{
            gazeDir = eyeGaze.transform.rotation * Vector3.forward;

            RaycastHit hit;
            // int layerMask = LayerMask.NameToLayer("Ignore Raycast");
            int layerMask = LayerMask.NameToLayer("Raycast hit");
            // layerMask = ~layerMask;
            if (Physics.Raycast(hmd.transform.position, gazeDir, out hit, Mathf.Infinity, layerMask)){
                gazeTarget.transform.position = hmd.transform.position + (gazeDir * hit.distance);
                if (gazeTarget.GetComponent<Renderer>().enabled) gazeTarget.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
            else{
                gazeTarget.transform.position = hmd.transform.position + (gazeDir * 5.0f);
                if (gazeTarget.GetComponent<Renderer>().enabled) gazeTarget.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginMovement : MonoBehaviour
{
    public GameObject origin;

    private bool setUpMode;
    public GameObject rig;
    public GameObject hand; 


    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion handDirection = hand.transform.rotation;
        handDirection.x = 0;
        handDirection.z = 0;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            setUpMode = true;
            Debug.Log($"Currently in Set Up Mode: Set Up Mode = {setUpMode}");
        }

        if (setUpMode)
        {

            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                Vector3 newOriginPoint = new Vector3(rig.transform.position.x, 0.5f, rig.transform.position.z);
                origin.transform.position = newOriginPoint;
                origin.transform.rotation = handDirection;
                Debug.Log($"Origin Point Set at {newOriginPoint}");
                Debug.Log($"Origin Rotation Set at {handDirection}");
            }

        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            setUpMode = false;
            Debug.Log($"Currently in Experiment Mode: Set Up Mode = {setUpMode}");
        }
       
        

        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginMovement : MonoBehaviour
{
    public GameObject origin;

    private bool setUpMode;
    public GameObject centerEyeAnchor;

    public GameObject ovrRig; 
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
                Vector3 newOriginPoint = new Vector3(centerEyeAnchor.transform.position.x, 0f, centerEyeAnchor.transform.position.z); // TDW changed y pos to 0
                origin.transform.position = newOriginPoint;
                origin.transform.rotation = handDirection;
                Debug.Log($"Origin Point Set at {newOriginPoint}");
                Debug.Log($"Origin Rotation Set at {handDirection}");
            }

            // Enable joystick locomotion controls
            Vector2 joystickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);

            // Get the forward and right vectors of the centerEyeAnchor
            Vector3 forward = centerEyeAnchor.transform.forward;
            Vector3 right = centerEyeAnchor.transform.right;

            // Flatten the vectors to ignore vertical movement
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate the move direction relative to the player's look direction
            Vector3 moveDirection = (forward * joystickInput.y + right * joystickInput.x);

            // Move the ovrRig (or trackingSpace) based on the calculated direction
            ovrRig.transform.Translate(moveDirection * Time.deltaTime * 2.0f, Space.World); // Adjust speed multiplier as needed
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            setUpMode = false;
            Debug.Log($"Currently in Experiment Mode: Set Up Mode = {setUpMode}");
        }
       
        

        
    }
}

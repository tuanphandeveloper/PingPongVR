using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class paddleMovement : MonoBehaviour {

    [SerializeField] Parameters parameters;

    public List<Vector3> paddlePositions;
    public List<Vector3> paddleOrientations;
    public Vector3 paddleCurrentVelocity;
    public Vector3 paddlePreviousVelocity;
    public Vector3 paddleAcceleration;
    public float mass = 0.07f;
    Vector3 paddleLastPosition;

    public GameObject tracker, controllerRightHand, controllerLeftHand, paddle;   
    Rigidbody paddleBody;
    int numberOfActiveController;
    // Use this for initialization
    void Start()
    {
        

        paddleBody = GetComponent<Rigidbody>();

        SteamVR_TrackedObject trackerObject = tracker.GetComponent<SteamVR_TrackedObject>();
       
       

        for (int index = 0; index < SteamVR.connected.Length; ++index)
        {
            if (OpenVR.System != null)
            {
                //lets figure what type of device got connected
                ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)index);
                bool deviceConnected = OpenVR.System.IsTrackedDeviceConnected((uint)index);
                if (deviceClass == ETrackedDeviceClass.Controller && deviceConnected)
                {
                    Debug.Log("Controller got connected at index:" + index);
                    numberOfActiveController++;
                }
                if (deviceClass == ETrackedDeviceClass.GenericTracker && deviceConnected)
                {
                    Debug.Log("Tracker got connected at index:" + index);
                    
                    if (index == 1)
                        trackerObject.index = trackerObject.device1;
                    if (index == 2)
                        trackerObject.index = trackerObject.device2;
                    if (index == 3)
                        trackerObject.index = trackerObject.device3;
                    if (index == 4)
                        trackerObject.index = trackerObject.device4;
                    if (index == 5)
                        trackerObject.index = trackerObject.device5;
                        
                }

            }
        }

        Debug.Log("Number of tracked Controllers: "+ numberOfActiveController);
        if (numberOfActiveController < 1)
        {
            //tracker.GetComponent<SteamVR_TrackedObject>().index = 
            paddle = controllerLeftHand;
            paddle.GetComponent<Hand>().renderModelPrefab = null;
            gameObject.transform.GetChild(0).transform.Rotate(new Vector3(90, 0, 0));
            Debug.Log("Paddle is attached to controllerLeftHand");

        }
        else
        {
            paddle = tracker;
            Debug.Log("Paddle is attached to Tracker");
        }
        

        /*
        uint indexTracker = 0;
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            if (result.ToString().Contains("tracker"))
            {
                Debug.Log("Tracker is available: " + i);
                indexTracker = i;
                break;
            }
        }
        */
       
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        MovePaddle();
        paddlePositions.Add(transform.position);
        paddleOrientations.Add(transform.rotation.eulerAngles);
        //Debug.Log("Paddle velocity: " + paddleBody.velocity);
       
        //Vector3 velocityEndPoint = transform.position + paddleBody.velocity.normalized * 0.15f;
        //Debug.DrawLine(transform.position, velocityEndPoint, Color.black);

        DrawArrow.ForDebug(transform.position, paddleBody.velocity.normalized * 0.1f, Color.black, 0.025f, 20f);

    }

    void MovePaddle()
    {
        transform.position = paddle.transform.position;
        transform.rotation = paddle.transform.rotation * Quaternion.Euler(180f, 0f, 0f);
        paddleCurrentVelocity = (paddle.transform.position - paddleLastPosition) / Time.deltaTime;
        paddleAcceleration = (paddleCurrentVelocity - paddlePreviousVelocity) / Time.deltaTime;
        paddleLastPosition = paddle.transform.position;
        paddlePreviousVelocity = paddleCurrentVelocity;
    }

    private void OnDrawGizmos()
    {
       // DrawArrow.ForGizmo(transform.position, new Vector3(1,1,1) * 0.1f, 0.025f, 20f);

    }

    
}

public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class launchBalls : MonoBehaviour {
    //newly added ****************
    [SerializeField] GameObject projPrefab;
    [SerializeField] Transform muzzle;
    [SerializeField] Parameters parameters;
    [SerializeField] ballPhysics ballProperties;
    [SerializeField] KinectRecorderPlayer kinectRecorder;

    

    protected float shotAngle = 0.0f;
    // Use this for initialization
    protected new Rigidbody rigidbody;
    public GameObject LeftController;
    public GameObject RightController;
    public GameObject currentBall;
    public int currentSessionHits = 0;
    protected Hand RightHand;
    protected Hand LeftHand;
    public GameObject QuadFive;
    public GameObject QuadSix;
    public GameObject QuadSeven;
    public GameObject QuadEight;
    public Color seeThrough;
    public int hitQuadrant = 0;
    public float ballCounter = 0.0f;
    private int countValue = 5;

    private float countDownStart;


    public bool tutorialStarted = false, firstBallLaunched;
    public bool quadRunning = false;
    public bool countDown = false;

    bool rightTriggerPressed, leftTriggerPressed, rightGripClicked, leftGripClicked;
    Vector3 velocity, spin;


    void Start () {
        //rigidbody = ball.GetComponent<Rigidbody>();
        RightHand = RightController.GetComponent<Hand>();
        LeftHand = LeftController.GetComponent<Hand>();
        seeThrough = QuadFive.GetComponent<MeshRenderer>().material.color;
        hitQuadrant = Random.Range(5, 8);
    }


	// Update is called once per frame
	void Update () {
        // newlly added *************
        if (!parameters.calibrating)
        {
            if (countDown)
            {
                if (Time.time - countDownStart >= 1.0f)
                {
                    countDownStart = Time.time;
                    parameters.countDownValue.text = countValue.ToString();
                    if (countValue == 0)
                    {
                        countDown = false;
                        countValue = 5;
                        parameters.countDownValue.text = "";
                        tutorialLauncher();
                        firstBallLaunched = true;
                    }
                    else
                        --countValue;
                }

            }

            if (!parameters.Auto && !parameters.calibrating)
            {
                rightTriggerPressed = SteamVR_Input.__actions_default_in_TriggerClick.GetStateDown(RightHand.handType);
                leftTriggerPressed = SteamVR_Input.__actions_default_in_TriggerClick.GetStateDown(LeftHand.handType);

                rightGripClicked = SteamVR_Input.__actions_default_in_generate_ball.GetStateDown(RightHand.handType);
                leftGripClicked = SteamVR_Input.__actions_default_in_generate_ball.GetStateDown(LeftHand.handType);


                // when trigger of either right and left controller is pressed 
                if (rightTriggerPressed || leftTriggerPressed)
                    shootBall();

                // when the right controller grip is pressed 
                if (rightGripClicked)
                    dropBall(RightController);

                // when the left controller grip is pressed 
                if (leftGripClicked)
                    dropBall(LeftController);
            }


            if (parameters.Auto && parameters.trialCount > 0 && !countDown)
            {

                if (!firstBallLaunched && currentSessionHits <= 0)
                {
                    if (parameters.forehand)
                    {
                        currentSessionHits = parameters.setForehand();
                        parameters.forehand = false;
                        parameters.currentSession.text = "Forehand";
                        playCountDown();
                    }
                    else if (!parameters.forehand && parameters.backhand)
                    {
                        currentSessionHits = parameters.setBackhand();
                        parameters.backhand = false;
                        parameters.currentSession.text = "Backhand";
                        playCountDown();
                    }
                    else if (!parameters.forehand && !parameters.backhand && parameters.foreAndBack)
                    {
                        currentSessionHits = parameters.setForeAndBack();
                        parameters.foreAndBack = false;
                        parameters.currentSession.text = "Fore/Back Mix";
                        playCountDown();
                    }
                    else if (!parameters.forehand && !parameters.backhand && !parameters.foreAndBack && parameters.fourQuad)
                    {
                        currentSessionHits = parameters.set4Quad();
                        parameters.fourQuad = false;
                        quadRunning = true;
                        parameters.currentSession.text = "4 Quads";
                        playCountDown();
                    }


                }
                else if (currentBall.GetComponent<ballPhysics>().isHitGround)
                {
                    tutorialLauncher();
                    if (currentSessionHits == 0)
                    {
                        firstBallLaunched = false;
                    }
                }
            }
            else if (parameters.trialCount <= 0 && parameters.Auto && !countDown)
            {
                parameters.Auto = false;
                parameters.storeData = false;
                firstBallLaunched = false;
                quadRunning = false;
                QuadFive.GetComponent<MeshRenderer>().material.color = seeThrough;
                QuadSix.GetComponent<MeshRenderer>().material.color = seeThrough;
                QuadSeven.GetComponent<MeshRenderer>().material.color = seeThrough;
                QuadEight.GetComponent<MeshRenderer>().material.color = seeThrough;
                parameters.currentSession.text = "None";
            }

            if (quadRunning && !countDown)
            {
                switch (hitQuadrant)
                {
                    case 5:
                        QuadFive.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
                        QuadSix.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSeven.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadEight.GetComponent<MeshRenderer>().material.color = seeThrough;
                        break;
                    case 6:
                        QuadFive.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSix.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
                        QuadSeven.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadEight.GetComponent<MeshRenderer>().material.color = seeThrough;
                        break;
                    case 7:
                        QuadFive.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSix.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSeven.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
                        QuadEight.GetComponent<MeshRenderer>().material.color = seeThrough;
                        break;
                    case 8:
                        QuadFive.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSix.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadSeven.GetComponent<MeshRenderer>().material.color = seeThrough;
                        QuadEight.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void playCountDown()
    {
        countDown = true;
        countDownStart = Time.time;

    }


    void dropBall(GameObject controller)
    {
        // create a new ball from RightController's position (with it's rotation) and place it in the main scene 
        var currentBall = Instantiate(projPrefab, controller.transform.position, Quaternion.identity);
        var aBody = currentBall.GetComponent<ballPhysics>();

        // no velocity is added and Unity take care of the physics
        aBody.makeBall();
        aBody.ball.setVelocity(new Vector3(0f, 0.5f, 0f)*3.0f);
        //aBody.AddForce(velocity, ForceMode.Force);

        if (parameters.trial)
        {
            parameters.trialCount++;
            currentBall.GetComponent<ballPhysics>().trialCount = parameters.trialCount;
        }
    }

    void shootBall()
    {
        // create a new ball from muzzle's position (with it's rotation) and place it in the main scene 
        currentBall = Instantiate(projPrefab, muzzle.position, muzzle.rotation);
        var aBody = currentBall.GetComponent<ballPhysics>();

        //Debug.Log("Angular velocity: " + aBody.angularVelocity);

        // add velocity in a way that the balls change the direction to shot to the other side
        // from left to the center to the right. 
        Vector3 velocity = new Vector3(parameters.angleHorizontal, 6.0f*parameters.angleVertical, -1 * (parameters.velocity)*5.0f);
        Vector3 spin = new Vector3(parameters.horizontalSpin, parameters.verticalSpin, parameters.forwardSpin);

        kinectRecorder.ball = currentBall;
        aBody.makeBall();
        aBody.ball.setVelocity(velocity);
        aBody.ball.setAngularVelocity(spin);

        if (parameters.trial)
        {
            parameters.trialCount++;
            aBody.trialCount = parameters.trialCount;
        }
    }

    void tutorialLauncher()
    {
        // create a new ball from muzzle's position (with it's rotation) and place it in the main scene
        ++ballCounter;
        currentBall = Instantiate(projPrefab, muzzle.position, muzzle.rotation);
        var aBody = currentBall.GetComponent<ballPhysics>();
        aBody.makeBall();
        kinectRecorder.ball = currentBall;

        if (parameters.fixedTrials == 0)     //Fixed
        {
            velocity = new Vector3(parameters.angleHorizontal, parameters.angleVertical*6.0f, -1 * parameters.velocity*5.0f);
            spin = new Vector3(parameters.horizontalSpin, parameters.verticalSpin, parameters.forwardSpin);
        }
        else if(parameters.fixedTrials == 1)   //Random
        {            
            velocity = new Vector3(Random.Range(float.Parse(parameters.hAngleMin.text), 6.0f*float.Parse(parameters.hAngleMax.text)), Random.Range(float.Parse(parameters.vAngleMin.text), float.Parse(parameters.vAngleMax.text)), -1 * Random.Range(float.Parse(parameters.velocityMin.text), float.Parse(parameters.velocityMax.text))*5.0f);
            parameters.velocity = -velocity.z;
            parameters.angleHorizontal = velocity.x;
            parameters.angleVertical = velocity.y;
            spin = new Vector3(Random.Range(float.Parse(parameters.tbSpinMin.text), float.Parse(parameters.tbSpinMax.text)), Random.Range(float.Parse(parameters.vSpinMin.text), float.Parse(parameters.vSpinMax.text)), Random.Range(float.Parse(parameters.fSpinMin.text), float.Parse(parameters.fSpinMax.text)));
        }

        // add velocity in a way that the balls change the direction to shot to the other side
        // from left to the center to the right.
        aBody.ball.setVelocity(velocity);
        aBody.ball.setAngularVelocity(spin);
    }


}

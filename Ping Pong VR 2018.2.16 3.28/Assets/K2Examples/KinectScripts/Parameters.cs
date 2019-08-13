// LICENSE
//
//   This software is dual-licensed to the public domain and under the following
//   license: you are granted a perpetual, irrevocable license to copy, modify,
//   publish, and distribute this file as you see fit.

using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;


public class Parameters : MonoBehaviour
{

    // Inspector fields
    [SerializeField] GameObject uiRoot;
    [SerializeField] launchBalls launcher;
    [SerializeField] paddleMovement paddleMovement;

    public enum tutorialType
    {
        ForeHand,
        BackHand,
        ForeBackMix,
        FourQuads
    };

    public GameObject LeftController;
    public GameObject RightController;
    public UnityEngine.UI.Slider velocitySlider;
    public UnityEngine.UI.Slider angleVerticalSlider;
    public UnityEngine.UI.Slider angleHorizontalSlider;
    public UnityEngine.UI.InputField velocityMin, velocityMax;    
    public UnityEngine.UI.InputField vAngleMin, vAngleMax;
    public UnityEngine.UI.InputField hAngleMin, hAngleMax;
    public UnityEngine.UI.InputField vSpinMin, vSpinMax;
    public UnityEngine.UI.InputField tbSpinMin, tbSpinMax;
    public UnityEngine.UI.InputField fSpinMin, fSpinMax;
    public UnityEngine.UI.InputField foreHandTrial, backHandTrial, foreAndBackTrial, quadrantTrial;
    int foreHandTrialCount, backHandTrialCount, foreAndBackTrialCount, quadrantTrialCount;
    float analCount = 0.0f;

    public GameObject calibratePanel;
    public GameObject calibrateDone;
    [SerializeField] UnityEngine.UI.Image calibrateProgress;
    [SerializeField] UnityEngine.UI.Button calibrateButton;
    [SerializeField] UnityEngine.UI.Slider verticalSpinSlider;
    [SerializeField] UnityEngine.UI.Slider horizontalSpinSlider;
    [SerializeField] UnityEngine.UI.Slider forwardSpinSlider;
    [SerializeField] UnityEngine.UI.Slider fireRateSlider;
    [SerializeField] UnityEngine.UI.Dropdown display;
    [SerializeField] UnityEngine.UI.Dropdown trialType;
    [SerializeField] UnityEngine.UI.Dropdown userControl;

    
    [SerializeField] UnityEngine.UI.Text velocitySliderValue;
    [SerializeField] UnityEngine.UI.Text angleVerticalSliderValue;
    [SerializeField] UnityEngine.UI.Text angleHorizontalSliderValue;
    [SerializeField] UnityEngine.UI.Text verticalSpinSliderValue;   
    [SerializeField] UnityEngine.UI.Text horizantalSpinSliderValue;
    [SerializeField] UnityEngine.UI.Text forwardSpinSliderValue;
    [SerializeField] UnityEngine.UI.Text fireRateSliderValue;
    [SerializeField] UnityEngine.UI.Text trialCountValue;
    [SerializeField] UnityEngine.UI.Text AutoUser;
    [SerializeField] UnityEngine.UI.Text hitsLeftValue;
    [SerializeField] UnityEngine.UI.Text scoreValue;
    [SerializeField] public UnityEngine.UI.Text currentSession;
    [SerializeField] public UnityEngine.UI.Text countDownValue;

    [SerializeField] UnityEngine.UI.Toggle lightToggle;
    [SerializeField] UnityEngine.UI.Toggle soundToggle;
    [SerializeField] UnityEngine.UI.Toggle topViewToggle;
    [SerializeField] UnityEngine.UI.Toggle projectileToggle;
    [SerializeField] UnityEngine.UI.Toggle physicsToggle;
    [SerializeField] UnityEngine.UI.Toggle AutoToggle;
    [SerializeField] UnityEngine.UI.Toggle storeDataToggle;
    [SerializeField] UnityEngine.UI.Toggle trialToggle;
    [SerializeField] UnityEngine.UI.Toggle foreHandToggle;
    [SerializeField] UnityEngine.UI.Toggle backHandToggle;
    [SerializeField] UnityEngine.UI.Toggle foreAndBackToggle;
    [SerializeField] UnityEngine.UI.Toggle quadrantToggle;





    //[SerializeField] UnityEngine.UI.Dropdown catchingModeDropdown;

    //[SerializeField] float velocityMax;
    [SerializeField] float verticalSpinMax;
    [SerializeField] float horizantalSpinMax;
    [SerializeField] float angleMax;
    [SerializeField] public bool useTracker;
    public float rateOfFireMax;

    public int numHitsRequired;
    public int numBallHitsLeft;
    public float trialCount = 0.0f;
    float totalTrialNumber;
    float start = 0.0f;
    float timePassed = 0.0f;

    public bool targetBall = false;
    public Animator myAnimator;
    public string reportName, dataFileName;
    // writing to file variable 
    public bool writeToFileProcessedData;
    public bool writeToFileKinectData;
    public int fixedTrials;
    public bool startTutorial = false;
    public bool startForeBackTutorial = false;
    public bool storingData = false;
    public bool calibrating = false;
    float[][] trialInputVariables = new float[4][]; 
    
    bool isFileCreatedProcessedData = false;
    bool rightTriggerPressed, leftTriggerPressed, rightTrackpadClicked, leftTrackpadClicked, rightTrackpadTouched, leftTrackpadTouched, addSpinRight, addSpinLeft;
    bool touchCount = false;
    Vector2 trackpadPosition, initialPosition, finalPosition;
    StreamWriter reportWriter, dataWriter, kinectPositionWriter, ballDataWriter;
    StreamReader trialInputFileStreamReader;
    //public bool angleChanging = false;
    public string testSubjectName;
    public tutorialType currentTutorial;
    GameObject VRcanvas;
    public GameObject dropDown;
    public GameObject kinectAvatar;
    public Transform[] cameraDisplay = new Transform[4];


   

    // Properties
    public float velocity { get { return velocitySlider.value; } set { velocitySlider.value = value; }  }
    public float angleVertical { get { return angleVerticalSlider.value; }    set { angleVerticalSlider.value = value; }    }
    public float angleHorizontal { get { return angleHorizontalSlider.value; } set { angleHorizontalSlider.value = value; } }
    public float verticalSpin { get { return verticalSpinSlider.value; } set { verticalSpinSlider.value = value; } }
    public float horizontalSpin { get { return horizontalSpinSlider.value; } set { horizontalSpinSlider.value = value; } }
    public float forwardSpin { get { return forwardSpinSlider.value; } set { forwardSpinSlider.value = value; } }
    public float fireRate { get { return fireRateSlider.value; } set { fireRateSlider.value = value; } }


    public bool Light { get { return lightToggle.isOn; } }
    public bool Sound { get { return soundToggle.isOn; } }
    public bool topView { get { return topViewToggle.isOn; } }
    public bool Projectile { get { return projectileToggle.isOn; } }
    public bool Physics { get { return physicsToggle.isOn; } }
    public bool Auto { get { return AutoToggle.isOn; } set { AutoToggle.isOn = value; } }
    public bool storeData { get { return storeDataToggle.isOn; } set { storeDataToggle.isOn = value; } }
    public bool trial { get { return trialToggle.isOn; } }
    public bool forehand { get { return foreHandToggle.isOn; } set { foreHandToggle.isOn = value; } }
    public bool backhand { get { return backHandToggle.isOn; } set { backHandToggle.isOn = value; } }
    public bool foreAndBack { get { return foreAndBackToggle.isOn; } set { foreAndBackToggle.isOn = value; } }
    public bool fourQuad { get { return quadrantToggle.isOn; } set { quadrantToggle.isOn = value; } }

    List<Vector3> kinectPositions;
    List<Quaternion> kinectOrientations;

    UnityEngine.SceneManagement.Scene m_Scene;



    public void Start()
    {
        m_Scene = SceneManager.GetActiveScene();
        kinectPositions = new List<Vector3>();
        kinectOrientations = new List<Quaternion>();

        //ballDataWriter = new StreamWriter("Assets/BallAndPaddleData.csv", false);

        if (m_Scene.name == "PingpongScene")
        {
#if !UNITY_WSA
            StreamReader fileReader = new StreamReader("Assets/KinectPositions.csv");

            string sPlayLine = fileReader.ReadLine();
            string[] myStrings = sPlayLine.Split(',');
            float xpos = float.Parse(myStrings[0]);
            float ypos = float.Parse(myStrings[1]);
            float zpos = float.Parse(myStrings[2]);
            float xrot = float.Parse(myStrings[3]);
            float yrot = float.Parse(myStrings[4]);
            float zrot = float.Parse(myStrings[5]);

            kinectAvatar.transform.position = new Vector3(xpos, ypos, zpos);
            kinectAvatar.transform.rotation = Quaternion.Euler(xrot, yrot, zrot);
#endif

            launcher = GameObject.FindGameObjectWithTag("ballLauncher").GetComponent<launchBalls>();
            myAnimator = GetComponent<Animator>();
            velocitySliderValue.text = velocitySlider.value.ToString("F4");
            angleVerticalSliderValue.text = angleVerticalSlider.value.ToString("F4");
            angleHorizontalSliderValue.text = angleHorizontalSlider.value.ToString("F4");
            verticalSpinSliderValue.text = verticalSpinSlider.value.ToString("F4");
            horizantalSpinSliderValue.text = horizontalSpinSlider.value.ToString("F4");
            forwardSpinSliderValue.text = forwardSpinSlider.value.ToString("F4");
            fireRateSliderValue.text = fireRateSlider.value.ToString("F4");
            countDownValue.text = "";
            trialCountValue.text = "Trial Left: " + trialCount;

            totalTrialNumber = trialCount;

            if (physicsToggle.isOn)
            {
                angleVerticalSlider.interactable = true;
                verticalSpinSlider.interactable = true;
                horizontalSpinSlider.interactable = true;
            }
            else
            {
                angleVerticalSlider.interactable = false;
                verticalSpinSlider.interactable = false;
                horizontalSpinSlider.interactable = false;

            }

            VRcanvas = GameObject.Find("VRcanvas");


            try
            {
                trialInputFileStreamReader = new StreamReader("Assets/trial_inputs.csv", true);
                //skip first line. it's tab captions
                string reportCaptions = trialInputFileStreamReader.ReadLine();
                int lineNumber = 0;

                while (!trialInputFileStreamReader.EndOfStream)
                {
                    string line = trialInputFileStreamReader.ReadLine();
                    trialInputVariables[lineNumber] = new float[18];
                    trialInputVariables[lineNumber] = Array.ConvertAll(line.Split(','), float.Parse);
                    lineNumber++;
                }
                trialInputFileStreamReader.Close();

            }
            catch (IOException e)
            {
                print("File reading exception message: " + e.Message);
            }


            // setup default value 
            velocityMin.text = velocitySlider.minValue.ToString();
            velocityMax.text = velocitySlider.maxValue.ToString();

            vAngleMin.text = angleVerticalSlider.minValue.ToString();
            vAngleMax.text = angleVerticalSlider.maxValue.ToString();

            hAngleMin.text = angleHorizontalSlider.minValue.ToString();
            hAngleMax.text = angleHorizontalSlider.maxValue.ToString();

            vSpinMin.text = verticalSpinSlider.minValue.ToString();
            vSpinMax.text = verticalSpinSlider.maxValue.ToString();

            //top and back spin is the same us horizontal spin
            tbSpinMin.text = horizontalSpinSlider.minValue.ToString();
            tbSpinMax.text = horizontalSpinSlider.maxValue.ToString();

            fSpinMin.text = forwardSpinSlider.minValue.ToString();
            fSpinMax.text = forwardSpinSlider.maxValue.ToString();

            foreHandTrial.text = backHandTrial.text = foreAndBackTrial.text = quadrantTrial.text = "0";
        }

    }


    // Methods
    void Update() {

        if(m_Scene.name == "PingpongScene" && !calibrating)
        {
            if (Input.GetKeyDown(KeyCode.U))
                uiRoot.SetActive(!uiRoot.activeSelf);

            fixedTrials = trialType.value;

            trialCountValue.text = "Trials Left: " + trialCount;
            hitsLeftValue.text = "" + trialCount;
            if (!Auto && userControl.value == 1)
            {
                rightTrackpadTouched = SteamVR_Input.__actions_default_in_TrackpadTouch.GetState(RightController.GetComponent<Hand>().handType);
                leftTrackpadTouched = SteamVR_Input.__actions_default_in_TrackpadTouch.GetState(LeftController.GetComponent<Hand>().handType);


                if (rightTrackpadTouched || leftTrackpadTouched)
                {
                    finalPosition = SteamVR_Input.__actions_default_in_TrackpadNavigate.GetAxis(rightTrackpadTouched ? RightController.GetComponent<Hand>().handType : LeftController.GetComponent<Hand>().handType);
                    if (!touchCount)
                    {
                        initialPosition = finalPosition;
                        touchCount = true;
                        // Debug.Log("touch started!");
                    }
                }

                if (!rightTrackpadTouched && !leftTrackpadTouched && touchCount && (finalPosition - initialPosition).magnitude > 0.2f)
                {
                    // Debug.Log("touch Ended!");
                    if ((finalPosition - initialPosition).magnitude > 0.3f)
                    {
                        addSpin();
                    }

                    finalPosition = Vector2.zero;
                    initialPosition = Vector2.zero;
                    touchCount = false;
                }

                rightTrackpadClicked = SteamVR_Input.__actions_default_in_TrackpadClick.GetStateDown(RightController.GetComponent<Hand>().handType);
                leftTrackpadClicked = SteamVR_Input.__actions_default_in_TrackpadClick.GetStateDown(LeftController.GetComponent<Hand>().handType);

                if (rightTrackpadClicked || leftTrackpadClicked)
                {
                    touchCount = false;
                    addSpeedOrAngle(rightTrackpadClicked ? RightController.GetComponent<Hand>() : LeftController.GetComponent<Hand>());
                }

            }
        }

        if (calibrating)
        {
            rightTriggerPressed = SteamVR_Input.__actions_default_in_TriggerClick.GetState(RightController.GetComponent<Hand>().handType);
            leftTriggerPressed = SteamVR_Input.__actions_default_in_TriggerClick.GetState(LeftController.GetComponent<Hand>().handType);

            GameObject triggerHand = null;
            if(rightTriggerPressed || leftTriggerPressed)
            {
                if(start == 0.0f)
                {
                    start = Time.time;
                }

                if (rightTriggerPressed)
                    triggerHand = RightController;
                else if (leftTriggerPressed)
                    triggerHand = LeftController;

                bool pressed = true;

                if(pressed && timePassed <= 4.0f)
                {
                    float xPos = triggerHand.transform.position.x;
                    float yPos = triggerHand.transform.position.y;
                    float zPos = triggerHand.transform.position.z;

                    kinectPositions.Add(new Vector3(xPos, yPos, zPos));

                    float xOr = triggerHand.transform.rotation.x;
                    float yOr = triggerHand.transform.rotation.y;
                    float zOr = triggerHand.transform.rotation.z;

                    kinectOrientations.Add(Quaternion.Euler(xOr, yOr, zOr));

                    
                    timePassed = Time.time - start;
                    calibrateProgress.fillAmount = timePassed/4.0f;
                    pressed = SteamVR_Input.__actions_default_in_TriggerClick.GetState(triggerHand.GetComponent<Hand>().handType);

                }
            }
            if (timePassed >= 4.0f && (rightTriggerPressed || leftTriggerPressed))
            {
                calibrationDone();
            }
            else if (!rightTriggerPressed && !leftTriggerPressed && timePassed < 4.0f)
            {
                kinectPositions.Clear();
                kinectOrientations.Clear();
                calibrateProgress.fillAmount = 0.0f;
                timePassed = 0.0f;
                start = 0.0f;
            }
        }
    }

    void calibrationDone()
    {
        calibrateProgress.fillAmount = 0.0f;
        calibrating = !calibrating;
        calibratePanel.SetActive(!calibratePanel.activeSelf);
        calibrateDone.SetActive(true);
        kinectPositionWriter = new StreamWriter("Assets/KinectPositions.csv", false);

        kinectPositionWriter.WriteLine(kinectPositions.Average(x => x.x) + "," + kinectPositions.Average(x => x.y) + "," + kinectPositions.Average(x => x.z) + ',' + kinectOrientations.Average(x => x.x) + "," + kinectOrientations.Average(x => x.y) + "," + kinectOrientations.Average(x => x.z));

        kinectPositionWriter.Close();

        kinectAvatar.transform.position = new Vector3(kinectPositions.Average(x => x.x) , kinectPositions.Average(x => x.y) , kinectPositions.Average(x => x.z));
        kinectAvatar.transform.rotation = Quaternion.Euler(new Vector3(kinectOrientations.Average(x => x.x), kinectOrientations.Average(x => x.y), kinectOrientations.Average(x => x.z)));
        Debug.Log(new Vector3(kinectPositions.Average(x => x.x), kinectPositions.Average(x => x.y), kinectPositions.Average(x => x.z)));

    }

    void addSpin()
    {
        if(Math.Abs((finalPosition - initialPosition).y) > 0.4)
        {
            horizontalSpin += -(finalPosition - initialPosition).y * 10.0f;
            horizontalSpinSlider.value = horizontalSpin;
            horizantalSpinSliderValue.text = horizontalSpinSlider.value.ToString("F4");
        }
        print("spin Magnitude: " + (finalPosition - initialPosition).magnitude);

        if (Math.Abs((finalPosition - initialPosition).x) > 0.4)
        {
            verticalSpin += -(finalPosition - initialPosition).x * 10.0f;
            verticalSpinSlider.value = verticalSpin;
            verticalSpinSliderValue.text = verticalSpinSlider.value.ToString("F4");
        }
    }

    void addSpeedOrAngle(Hand aHand)
    {
        trackpadPosition = SteamVR_Input.__actions_default_in_TrackpadNavigate.GetAxis(aHand.handType);

        bool angle = (trackpadPosition.x > 0.5f || trackpadPosition.x < -0.5f) && (trackpadPosition.y < 0.5f && trackpadPosition.y > -0.5f);
        bool speed = (trackpadPosition.x < 0.5f && trackpadPosition.x > -0.5f) && (trackpadPosition.y > 0.5f || trackpadPosition.y < -0.5f);

        if (angle)
        {
            if (trackpadPosition.x > 0)
            {
                angleHorizontal = angleHorizontal + 0.1f;
                angleHorizontalSlider.value = angleHorizontal;
                angleHorizontalSliderValue.text = angleHorizontalSlider.value.ToString("F4");
            }
            else
            {
                angleHorizontal = angleHorizontal - 0.1f;
                angleHorizontalSlider.value = angleHorizontal;
                angleHorizontalSliderValue.text = angleHorizontalSlider.value.ToString("F4");
            }
        }

        if (speed)
        {
            if (trackpadPosition.y > 0)
            {
                velocity = velocity + 0.1f;
                velocitySlider.value = velocity;
                velocitySliderValue.text = velocitySlider.value.ToString("F4");
            }
            else
            {
                velocity = velocity - 0.1f;
                velocitySlider.value = velocity;
                velocitySliderValue.text = velocitySlider.value.ToString("F4");
            }
        }
    }


    public void onCalibrate()
    {
        calibratePanel.SetActive(!calibratePanel.activeSelf);
        calibrating = !calibrating;
    }

    public void onDisplayChange() { 
        
        Camera.main.transform.localPosition = cameraDisplay[display.value].localPosition;
        Camera.main.transform.localRotation = cameraDisplay[display.value].localRotation;        
    }

    public void onTrialChange() {

        
        storeDataToggle.isOn = !storeDataToggle.isOn;
        AutoToggle.isOn = !AutoToggle.isOn;
    }
    

    public int setForehand()
    {
        if(trialType.value == 0)
        {
            velocity = trialInputVariables[0][0];
            velocitySlider.value = velocity;

            angleVertical = trialInputVariables[0][1];
            angleVerticalSlider.value = angleVertical;

            angleHorizontal = trialInputVariables[0][2];
            angleHorizontalSlider.value = angleHorizontal;

            forwardSpin = trialInputVariables[0][3];
            forwardSpinSlider.value = forwardSpin;

            horizontalSpin = trialInputVariables[0][4];
            horizontalSpinSlider.value = horizontalSpin;

            verticalSpin = trialInputVariables[0][5];
            verticalSpinSlider.value = verticalSpin;

            numBallHitsLeft = numHitsRequired;

            currentTutorial = tutorialType.ForeHand;
        }
        else if(trialType.value == 1)
        {
            // all values of variables of ForeHand are stored in the first row of the array 
            // min max values starts after the first 6 values of inputs
            velocityMin.text = trialInputVariables[0][6].ToString();
            if (trialInputVariables[0][7] >= trialInputVariables[0][6])
                velocityMax.text = trialInputVariables[0][7].ToString();
            else
                velocityMax.text = trialInputVariables[0][6].ToString();

            vAngleMin.text = trialInputVariables[0][8].ToString();
            if (trialInputVariables[0][9] >= trialInputVariables[0][8])
                vAngleMax.text = trialInputVariables[0][9].ToString();
            else
                vAngleMax.text = trialInputVariables[0][8].ToString();

            hAngleMin.text = trialInputVariables[0][10].ToString();
            if (trialInputVariables[0][11] >= trialInputVariables[0][10])
                hAngleMax.text = trialInputVariables[0][11].ToString();
            else
                hAngleMax.text = trialInputVariables[0][10].ToString();

            vSpinMin.text = trialInputVariables[0][12].ToString();
            if (trialInputVariables[0][13] >= trialInputVariables[0][12])
                vSpinMax.text = trialInputVariables[0][13].ToString();
            else
                vSpinMax.text = trialInputVariables[0][12].ToString();

            tbSpinMin.text = trialInputVariables[0][14].ToString();
            if (trialInputVariables[0][15] >= trialInputVariables[0][14])
                tbSpinMax.text = trialInputVariables[0][15].ToString();
            else
                tbSpinMax.text = trialInputVariables[0][14].ToString();

            fSpinMin.text = trialInputVariables[0][16].ToString();
            if (trialInputVariables[0][17] >= trialInputVariables[0][16])
                fSpinMax.text = trialInputVariables[0][17].ToString();
            else
                fSpinMax.text = trialInputVariables[0][16].ToString();
        }

        return foreHandTrialCount;
    }

    public int setBackhand()
    {
        if(trialType.value == 0)
        {
            velocity = trialInputVariables[1][0];
            velocitySlider.value = velocity;

            angleVertical = trialInputVariables[1][1];
            angleVerticalSlider.value = angleVertical;

            angleHorizontal = trialInputVariables[1][2];
            angleHorizontalSlider.value = angleHorizontal;

            forwardSpin = trialInputVariables[1][3];
            forwardSpinSlider.value = forwardSpin;

            horizontalSpin = trialInputVariables[1][4];
            horizontalSpinSlider.value = horizontalSpin;

            verticalSpin = trialInputVariables[1][5];
            verticalSpinSlider.value = verticalSpin;

            numBallHitsLeft = numHitsRequired;

            currentTutorial = tutorialType.BackHand;
        }
        else if(trialType.value == 1)
        {
            // all values of variables of BackHand are stored in the second row of the array 
            // min max values starts after the first 6 values of inputs
            velocityMin.text = trialInputVariables[1][6].ToString();
            if (trialInputVariables[1][7] >= trialInputVariables[1][6])
                velocityMax.text = trialInputVariables[1][7].ToString();
            else
                velocityMax.text = trialInputVariables[1][6].ToString();

            vAngleMin.text = trialInputVariables[1][8].ToString();
            if (trialInputVariables[1][9] >= trialInputVariables[1][8])
                vAngleMax.text = trialInputVariables[1][9].ToString();
            else
                vAngleMax.text = trialInputVariables[1][8].ToString();

            hAngleMin.text = trialInputVariables[1][10].ToString();
            if (trialInputVariables[1][11] >= trialInputVariables[1][10])
                hAngleMax.text = trialInputVariables[1][11].ToString();
            else
                hAngleMax.text = trialInputVariables[1][10].ToString();

            vSpinMin.text = trialInputVariables[1][12].ToString();
            if (trialInputVariables[1][13] >= trialInputVariables[1][12])
                vSpinMax.text = trialInputVariables[1][13].ToString();
            else
                vSpinMax.text = trialInputVariables[1][12].ToString();

            tbSpinMin.text = trialInputVariables[1][14].ToString();
            if (trialInputVariables[1][15] >= trialInputVariables[1][14])
                tbSpinMax.text = trialInputVariables[1][15].ToString();
            else
                tbSpinMax.text = trialInputVariables[1][14].ToString();

            fSpinMin.text = trialInputVariables[1][16].ToString();
            if (trialInputVariables[1][17] >= trialInputVariables[1][16])
                fSpinMax.text = trialInputVariables[1][17].ToString();
            else
                fSpinMax.text = trialInputVariables[1][16].ToString();
        }

        return backHandTrialCount;
    }

    public int setForeAndBack()
    {
        if(trialType.value == 0)
        {
            velocity = trialInputVariables[2][0];
            velocitySlider.value = velocity;

            angleVertical = trialInputVariables[2][1];
            angleVerticalSlider.value = angleVertical;

            angleHorizontal = trialInputVariables[2][2];
            angleHorizontalSlider.value = angleHorizontal;

            forwardSpin = trialInputVariables[2][3];
            forwardSpinSlider.value = forwardSpin;

            horizontalSpin = trialInputVariables[2][4];
            horizontalSpinSlider.value = horizontalSpin;

            verticalSpin = trialInputVariables[2][5];
            verticalSpinSlider.value = verticalSpin;

            numBallHitsLeft = numHitsRequired;

            currentTutorial = tutorialType.ForeBackMix;
        }
        else if(trialType.value == 1)
        {
            // all values of variables of ForeHand are stored in the third row of the array 
            // min max values starts after the first 6 values of inputs
            velocityMin.text = trialInputVariables[2][6].ToString();
            if (trialInputVariables[2][7] >= trialInputVariables[2][6])
                velocityMax.text = trialInputVariables[2][7].ToString();
            else
                velocityMax.text = trialInputVariables[2][6].ToString();

            vAngleMin.text = trialInputVariables[2][8].ToString();
            if (trialInputVariables[2][9] >= trialInputVariables[2][8])
                vAngleMax.text = trialInputVariables[2][9].ToString();
            else
                vAngleMax.text = trialInputVariables[2][8].ToString();

            hAngleMin.text = trialInputVariables[2][10].ToString();
            if (trialInputVariables[2][11] >= trialInputVariables[2][10])
                hAngleMax.text = trialInputVariables[2][11].ToString();
            else
                hAngleMax.text = trialInputVariables[2][10].ToString();

            vSpinMin.text = trialInputVariables[2][12].ToString();
            if (trialInputVariables[2][13] >= trialInputVariables[2][12])
                vSpinMax.text = trialInputVariables[2][13].ToString();
            else
                vSpinMax.text = trialInputVariables[2][12].ToString();

            tbSpinMin.text = trialInputVariables[2][14].ToString();
            if (trialInputVariables[2][15] >= trialInputVariables[2][14])
                tbSpinMax.text = trialInputVariables[2][15].ToString();
            else
                tbSpinMax.text = trialInputVariables[2][14].ToString();

            fSpinMin.text = trialInputVariables[2][16].ToString();
            if (trialInputVariables[2][17] >= trialInputVariables[2][16])
                fSpinMax.text = trialInputVariables[2][17].ToString();
            else
                fSpinMax.text = trialInputVariables[2][16].ToString();
        }

        return foreAndBackTrialCount;
    }

    public int set4Quad()
    {
        if(trialType.value == 0)
        {
            velocity = trialInputVariables[3][0];
            velocitySlider.value = velocity;

            angleVertical = trialInputVariables[3][1];
            angleVerticalSlider.value = angleVertical;

            angleHorizontal = trialInputVariables[3][2];
            angleHorizontalSlider.value = angleHorizontal;

            forwardSpin = trialInputVariables[3][3];
            forwardSpinSlider.value = forwardSpin;

            horizontalSpin = trialInputVariables[3][4];
            horizontalSpinSlider.value = horizontalSpin;

            verticalSpin = trialInputVariables[3][5];
            verticalSpinSlider.value = verticalSpin;

            numBallHitsLeft = numHitsRequired;

            currentTutorial = tutorialType.FourQuads;
        }
        else if(trialType.value == 1)
        {
            // all values of variables of ForeHand are stored in the fourth row of the array 
            // min max values starts after the first 6 values of inputs
            velocityMin.text = trialInputVariables[3][6].ToString();
            if (trialInputVariables[3][7] >= trialInputVariables[3][6])
                velocityMax.text = trialInputVariables[3][7].ToString();
            else
                velocityMax.text = trialInputVariables[3][6].ToString();

            vAngleMin.text = trialInputVariables[3][8].ToString();
            if (trialInputVariables[3][9] >= trialInputVariables[3][8])
                vAngleMax.text = trialInputVariables[3][9].ToString();
            else
                vAngleMax.text = trialInputVariables[3][8].ToString();

            hAngleMin.text = trialInputVariables[3][10].ToString();
            if (trialInputVariables[3][11] >= trialInputVariables[3][10])
                hAngleMax.text = trialInputVariables[3][11].ToString();
            else
                hAngleMax.text = trialInputVariables[3][10].ToString();

            vSpinMin.text = trialInputVariables[3][12].ToString();
            if (trialInputVariables[3][13] >= trialInputVariables[3][12])
                vSpinMax.text = trialInputVariables[3][13].ToString();
            else
                vSpinMax.text = trialInputVariables[3][12].ToString();

            tbSpinMin.text = trialInputVariables[3][14].ToString();
            if (trialInputVariables[3][15] >= trialInputVariables[3][14])
                tbSpinMax.text = trialInputVariables[3][15].ToString();
            else
                tbSpinMax.text = trialInputVariables[3][14].ToString();

            fSpinMin.text = trialInputVariables[3][16].ToString();
            if (trialInputVariables[3][17] >= trialInputVariables[3][16])
                fSpinMax.text = trialInputVariables[3][17].ToString();
            else
                fSpinMax.text = trialInputVariables[3][16].ToString();
        }

        return quadrantTrialCount;
    }

    public void OnAutoChange()
    {
        dropDown.SetActive(!dropDown.activeSelf);
        GameObject label = GameObject.FindGameObjectWithTag("autoLabel");        

        if (AutoToggle.isOn)
        {
            AutoUser.text = "Auto/User";
            startTutorial = true;
            analCount = trialCount;
            scoreValue.text = "";
        }
        else
        {
            AutoUser.text = "User/Auto";
            forehand = false;
            backhand = false;
            foreAndBack = false;
            fourQuad = false;
            launcher.currentSessionHits = 0;
            launcher.firstBallLaunched = false;
            trialCount = 0;
            trialCountValue.text = "Trials Left: " + trialCount;
            //print("balls shot: " + launcher.ballCounter);            
            int score = (int)(analCount / launcher.ballCounter*100);
            scoreValue.text = "Score: " + score + "/100";
            launcher.ballCounter = 0;
            currentSession.text = "None";
        }
            
    }

    public void onVelocityChange() {
        velocity = velocitySlider.value;
        velocitySliderValue.text = velocitySlider.value.ToString("F4");
    }

    // velocity min and max change 
    public void onVelocityMinChange() {
        if(float.Parse(velocityMin.text) < velocitySlider.maxValue)
            velocitySlider.minValue = float.Parse(velocityMin.text);
    }
    public void onVelocityMaxChange()
    {
        velocitySlider.maxValue = float.Parse(velocityMax.text);
    }

    // vertical angle min and max change
    public void onVAngleMinChange()
    {
        if (float.Parse(vAngleMin.text) < angleVerticalSlider.maxValue)
            angleVerticalSlider.minValue = float.Parse(vAngleMin.text);
    }
    public void onVAngleMaxChange()
    {
        angleVerticalSlider.maxValue = float.Parse(vAngleMax.text);
    }

    // horizontal min and max change
    public void onHAngleMinChange()
    {
        if (float.Parse(hAngleMin.text) < angleHorizontalSlider.maxValue)
            angleHorizontalSlider.minValue = float.Parse(hAngleMin.text);
    }
    public void onHAngleMaxChange()
    {
        angleHorizontalSlider.maxValue = float.Parse(hAngleMax.text);
    }

    // vertical spin min and max change
    public void onVSpinMinChange()
    {
        if (float.Parse(vSpinMin.text) < verticalSpinSlider.maxValue)
            verticalSpinSlider.minValue = float.Parse(vSpinMin.text);
    }
    public void onVSpinMaxChange()
    {
        verticalSpinSlider.maxValue = float.Parse(vSpinMax.text);
    }

    // top and back spin min and max change
    public void onTbSpinMinChange()
    {
        if (float.Parse(tbSpinMin.text) < horizontalSpinSlider.maxValue)
            horizontalSpinSlider.minValue = float.Parse(tbSpinMin.text);
    }
    public void onTbSpinMaxChange()
    {
        horizontalSpinSlider.maxValue = float.Parse(tbSpinMax.text);
    }

    // forward spin min and max change
    public void onFSpinMinChange()
    {
        if (float.Parse(fSpinMin.text) < forwardSpinSlider.maxValue)
            forwardSpinSlider.minValue = float.Parse(fSpinMin.text);
    }
    public void onFSpinMaxChange()
    {
        forwardSpinSlider.maxValue = float.Parse(fSpinMax.text);
    }

    public void onInputChange()
    {
        if (!Auto)
        {
            trialCount = 0;
            if (foreHandToggle.isOn)
            {
                foreHandTrialCount = int.Parse(foreHandTrial.text);
                trialCount += foreHandTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
            if (backHandToggle.isOn)
            {
                backHandTrialCount = int.Parse(backHandTrial.text);
                trialCount += backHandTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
            if (foreAndBackToggle.isOn)
            {
                foreAndBackTrialCount = int.Parse(foreAndBackTrial.text);
                trialCount += foreAndBackTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
            if (quadrantToggle.isOn)
            {
                quadrantTrialCount = int.Parse(quadrantTrial.text);
                trialCount += quadrantTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
        }
    }
    // trial settings 
    public void onForeHandChange()
    {
        if (foreHandToggle.isOn)
        {
            foreHandTrialCount = int.Parse(foreHandTrial.text);
            trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
            trialCountValue.text = "Trials Left: " + trialCount;
        }
        else {

            if (!Auto)
            {
                foreHandTrialCount = 0;
                trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
        }
    }

    public void onBackHandChange()
    {
        if (backHandToggle.isOn)
        {
            backHandTrialCount = int.Parse(backHandTrial.text);
            trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
            trialCountValue.text = "Trials Left: " + trialCount;
        }
        else {

            if (!Auto)
            {
                backHandTrialCount = 0;
                trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
        }
    }

    public void onForeAndBackChange()
    {
        if (foreAndBackToggle.isOn)
        {
            foreAndBackTrialCount = int.Parse(foreAndBackTrial.text);
            trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
            trialCountValue.text = "Trials Left: " + trialCount;
        }
        else {

            if (!Auto)
            {
                foreAndBackTrialCount = 0;
                trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
        }
    }

    public void onQuadrantChange()
    {
        if (quadrantToggle.isOn)
        {
            quadrantTrialCount = int.Parse(quadrantTrial.text);
            trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
            trialCountValue.text = "Trials Left: " + trialCount;
        }
        else {

            if (!Auto)
            {
                quadrantTrialCount = 0;
                trialCount = foreHandTrialCount + backHandTrialCount + foreAndBackTrialCount + quadrantTrialCount;
                trialCountValue.text = "Trials Left: " + trialCount;
            }
        }
    }



    public void onVAngleChange()
    {
        angleVertical = angleVerticalSlider.value;
        angleVerticalSliderValue.text = angleVerticalSlider.value.ToString("F4");
    }

    public void onHAngleChange()
    {
        angleHorizontal = angleHorizontalSlider.value;
        angleHorizontalSliderValue.text = angleHorizontalSlider.value.ToString("F4");
    }

    public void onVSpinChange()
    {
        verticalSpin = verticalSpinSlider.value;
        verticalSpinSliderValue.text = verticalSpinSlider.value.ToString("F4");
    }

    public void onHSpinChange()
    {
        horizontalSpin = horizontalSpinSlider.value;
        horizantalSpinSliderValue.text = horizontalSpinSlider.value.ToString("F4");
    }

    public void onFSpinChange()
    {
        forwardSpin = forwardSpinSlider.value;
        forwardSpinSliderValue.text = forwardSpinSlider.value.ToString("F4");
    }

    public void onFireRateChange()
    {
        fireRate = fireRateSlider.value;
        fireRateSliderValue.text = fireRateSlider.value.ToString("F4");
    }

    public void OnStoreData()
    {
        if (!Auto)
        {
            
            if (!storingData)
            { 
                creatFileToWrite();
            }
            storingData = !storingData;
        }
       // else
       // {
       //     storeDataToggle.isOn = !storeDataToggle.isOn;
       // }
          
    }

    //public void writeBallAndPaddleData(List<Vector3> ballPositions, List<Vector3> ballOrientations);
    //{
        //paddleMovement.paddlePositions
        //paddleMovement.paddleOrientations
        
   //}


    public void creatFileToWrite()
    {
        // if file was not created create the file then next time start writing to it. 
        if (!isFileCreatedProcessedData)
        {
            // create a file to write data to 
            try
            {
                reportName = testSubjectName + "_Report_" + System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".csv";

                reportWriter = new StreamWriter("Assets/OutputFiles/" + reportName, true);
                isFileCreatedProcessedData = true;
                /*
                writer.WriteLine("trialCount,launchingTime, launchingPoint.x,launchingPoint.y,launchingPoint.z, launchingAngle, launchingLinearVelocity, launchingAngularVelocity,"+
                    "landingPoint1.x,landingPoint1.y,landingPoint1.z, landingLinearVelocity1, landingAngularVelocity1,"+
                    "paddleCollisionPosition.x,paddleCollisionPosition.y,paddleCollisionPosition.z,paddleCollisionLinearVelocity, paddleCollisionAngularVelocity1," +
                    "landingPoint2.x,landingPoint2.y,landingPoint2.z,landingLinearVelocity2, landingAngularVelocity2," +
                    "isHit");
                */
                reportWriter.WriteLine("trialCount, launchingTime, launchingPosition.x, launchingPosition.y, launchingPosition.z, launchingAngles.x ,launchingAngles.y, launchingAngles.z, launchingLinearVelocity.x ,launchingLinearVelocity.y, launchingLinearVelocity.z, " +
                    "launchingAngularVelocity.x, launchingAngularVelocity.y, launchingAngularVelocity.z, landingPoint1.x, landingPoint1.y, landingPoint1.z, landingPoint1LinearVelocity.x, landingPoint1LinearVelocity.y, landingPoint1LinearVelocity.z, " +
                    "landingPoint1AngularVelocity.x, landingPoint1AngularVelocity.y, landingPoint1AngularVelocity.z, landingPoint1Quarter, paddleCollisionPosition.x, paddleCollisionPosition.y, paddleCollisionPosition.z, paddleCollisionLinearVelocity.x, paddleCollisionLinearVelocity.y, " +
                    "paddleCollisionLinearVelocity.z, paddleCollisionAngularVelocity.x, paddleCollisionAngularVelocity.y, paddleCollisionAngularVelocity.z, landingPoint2.x, landingPoint2.y, landingPoint2.z, landingPoint2LinearVelocity.x, " +
                     "landingPoint2LinearVelocity.y, landingPoint2LinearVelocity.z, landingPoint2AngularVelocity.x, landingPoint2AngularVelocity.y, landingPoint2AngularVelocity.z, landingPoint2Quarter, tableReferencePoint.x, tableReferencePoint.y, tableReferencePoint.z, isHit");
            }
            catch (IOException e)
            {
                print("File writing exception message: " + e.Message);
            }
        }
    }

    public void writeToFile(string outputLine) {
        if (isFileCreatedProcessedData)
        {
            reportWriter.WriteLine(outputLine);
            //string[] printedData = outputLine.Split(',');
            //double ballDistanceTriggerPressed = Double.Parse(printedData[17]);
            //ballDistanceTriggerPressedList.Add(ballDistanceTriggerPressed);
        }
    }
    private void OnDestroy()
    {
        if (isFileCreatedProcessedData)
        {           
            reportWriter.Close();
        }
    }

    public double Mean(List<double> values, int start, int end)
    {

        double s = 0;

        for (int i = start; i < end; i++)
        {
            s += values[i];
        }

        return s / (end - start);
    }

    public double Variance(List<double> values, double mean, int start, int end)
    {
        double variance = 0;

        for (int i = start; i < end; i++)
        {
            variance += Math.Pow((values[i] - mean), 2);
        }

        int n = end - start;
        if (start > 0) n -= 1;

        return variance / (n);
    }
    public double StandardDeviation(List<double> values, int start, int end)
    {
        double mean = Mean(values, start, end);
        double variance = Variance(values, mean, start, end);

        return Math.Sqrt(variance);
    }

}

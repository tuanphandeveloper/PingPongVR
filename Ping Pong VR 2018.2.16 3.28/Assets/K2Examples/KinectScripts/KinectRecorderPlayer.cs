using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// Kinect recorder and player is the component that manages recording and replaying of Kinect body-data files.
/// </summary>
public class KinectRecorderPlayer : MonoBehaviour
{
    [Tooltip("Path to the file used to record or replay the recorded data.")]
    public string filePath = "BodyRecording.txt";

    [Tooltip("UI-Text to display information messages.")]
    public UnityEngine.UI.Text infoText;

    [Tooltip("Whether to start playing the recorded data, right after the scene start.")]
    public bool playAtStart = false;

    public Parameters parameters;
    bool recordingStarted = false;
    bool kinectPosWritten = false;
    // singleton instance of the class
    private static KinectRecorderPlayer instance = null;

    // whether it is recording or playing saved data at the moment
    private bool isRecording = false;
    private bool isPlaying = false;

    // reference to the KM
    private KinectManager manager = null;

    // time variables used for recording and playing
    private long liRelTime = 0;
    private float fStartTime = 0f;
    private float fCurrentTime = 0f;
    private int fCurrentFrame = 0;

    // player variables
    private StreamReader fileReader = null;
    public float fPlayTime = 0f;
    private string sPlayLine = string.Empty;

    UnityEngine.SceneManagement.Scene m_Scene;
    public GameObject paddle;
    public GameObject ball;
    public GameObject Kinect;
    public Transform avatarSpineCenter;
    public Transform headSet;
   
    public Transform rightControllerPositionGameObject;
    public Transform leftControllerPositionGameObject;

    /// <summary>
    /// Gets the singleton KinectRecorderPlayer instance.
    /// </summary>
    /// <value>The KinectRecorderPlayer instance.</value>
    public static KinectRecorderPlayer Instance
    {
        get
        {
            return instance;
        }
    }



    // starts recording
    public bool StartRecording()
    {
        if (isRecording)
            return false;

        isRecording = true;

        // avoid recording an playing at the same time
        if (isPlaying && isRecording)
        {
            CloseFile();
            isPlaying = false;

            Debug.Log("Playing stopped.");
        }

        // stop recording if there is no file name specified
        if (filePath.Length == 0)
        {
            isRecording = false;

            Debug.LogError("No file to save.");
            if (infoText != null)
            {
                infoText.text = "No file to save.";
            }
        }

        if (isRecording)
        {
            Debug.Log("Recording started.");
            if (infoText != null)
            {
                infoText.text = "Recording... Say 'Stop' to stop the recorder.";
            }

            // delete the old csv file
            if (filePath.Length > 0 && File.Exists("Assets/OutputFiles/" + filePath))
            {
                File.Delete(filePath);
            }

            // initialize times
            fStartTime = fCurrentTime = Time.time;
            fCurrentFrame = 0;
        }

        return isRecording;
    }


    // starts playing
    public bool StartPlaying()
    {
        if (isPlaying)
            return false;

        isPlaying = true;

        // avoid recording an playing at the same time
        if (isRecording && isPlaying)
        {
            isRecording = false;
            Debug.Log("Recording stopped.");
        }

        // stop playing if there is no file name specified
        if (filePath.Length == 0 || !File.Exists("Assets/OutputFiles/" + filePath))
        {
            isPlaying = false;
            Debug.LogError("No file to play.");

            if (infoText != null)
            {
                infoText.text = "No file to play.";
            }
        }

        if (isPlaying)
        {
            Debug.Log("Playing started.");
            if (infoText != null)
            {
                infoText.text = "Playing... Say 'Stop' to stop the player.";
            }

            // initialize times
            fStartTime = fCurrentTime = Time.time;
            fCurrentFrame = -1;

            // open the file and read a line
#if !UNITY_WSA
            fileReader = new StreamReader("Assets/OutputFiles/" + filePath);

            sPlayLine = fileReader.ReadLine();
            string[] myStrings = sPlayLine.Split(',');
            float xpos = float.Parse(myStrings[0]);
            float ypos = float.Parse(myStrings[1]);
            float zpos = float.Parse(myStrings[2]);
            float xrot = float.Parse(myStrings[3]);
            float yrot = float.Parse(myStrings[4]);
            float zrot = float.Parse(myStrings[5]);

            Kinect.transform.position = new Vector3(xpos, ypos, zpos);
            Kinect.transform.rotation = Quaternion.Euler(xrot, yrot, zrot);
#endif
            ReadLineFromFile();

            // enable the play mode
            if (manager)
            {
                manager.EnablePlayMode(true);
            }
        }

        return isPlaying;
    }


    // stops recording or playing
    public void StopRecordingOrPlaying()
    {
        if (isRecording)
        {
            isRecording = false;

            Debug.Log("Recording stopped.");
            if (infoText != null)
            {
                infoText.text = "Recording stopped.";
            }
        }

        if (isPlaying)
        {
            // close the file, if it is playing
            CloseFile();
            isPlaying = false;

            Debug.Log("Playing stopped.");
            if (infoText != null)
            {
                infoText.text = "Playing stopped.";
            }
        }

        if (infoText != null)
        {
            infoText.text = "Say: 'Record' to start the recorder, or 'Play' to start the player.";
        }
    }

    // returns if file recording is in progress at the moment
    public bool IsRecording()
    {
        return isRecording;
    }

    // returns if file-play is in progress at the moment
    public bool IsPlaying()
    {
        return isPlaying;
    }


    // ----- end of public functions -----

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
       
        m_Scene = SceneManager.GetActiveScene();
        Debug.Log("Current scene: " + m_Scene.name);

        if (infoText != null)
        {
            infoText.text = "Say: 'Record' to start the recorder, or 'Play' to start the player.";
        }

        if (!manager)
        {
            manager = KinectManager.Instance;
        }
        else
        {
            Debug.Log("KinectManager not found, probably not initialized.");

            if (infoText != null)
            {
                infoText.text = "KinectManager not found, probably not initialized.";
            }
        }

        if (playAtStart)
        {
            if (m_Scene.name == "DataAnalysisScene")
                StartPlaying();

        }
        
       



    }
    
    void Update()
    {
        

        if ((m_Scene.name == "PingpongScene") && parameters.storingData && !recordingStarted)
        {

            Debug.Log("Report Name: " + parameters.reportName);
            filePath = parameters.reportName.Substring(0, parameters.reportName.Length - 31) + "_KinectData_" + parameters.reportName.Substring(parameters.reportName.Length - 23, 23);
            Debug.Log("Filename: " + filePath);
            StartRecording();
            recordingStarted = true;
          
        }
        if (isRecording)
        {
            // save the body frame, if any
            if (manager && manager.IsInitialized())
            {
                const char delimiter = ',';
                string sBodyFrame = manager.GetBodyFrameData(ref liRelTime, ref fCurrentTime, delimiter);

                

                if (sBodyFrame.Length > 0)
                {
#if !UNITY_WSA
                    
                    
                    using (StreamWriter writer = new StreamWriter("Assets/OutputFiles/" + filePath, true))
                    {
                        if (!kinectPosWritten)
                        {
#if !UNITY_WSA
                            using (StreamReader reader = new StreamReader("Assets/KinectPositions.csv"))
                            {

                                string[] myStrings = reader.ReadLine().Split(',');
                                float xPos = float.Parse(myStrings[0]);
                                float yPos = float.Parse(myStrings[1]);
                                float zPos = float.Parse(myStrings[2]);


                                myStrings = reader.ReadLine().Split(',');
                                float xRot = float.Parse(myStrings[0]);
                                float yRot = float.Parse(myStrings[1]);
                                float zRot = float.Parse(myStrings[2]);

                                string kinectPosition = string.Format("{0:F3},{1:F3},{2:F3}", xPos, yPos, zPos);
                                string kinectRotation = string.Format("{0:F3},{1:F3},{2:F3}", xRot, yRot, zRot);
                                writer.WriteLine(kinectPosition + "," + kinectRotation);
                                kinectPosWritten = true;

                            }
#endif
                        }
                       
                        string sBallPosition = "";
                        string sBallOrientation = "";
                        string sRealTime = string.Format("{0:F3}", (fCurrentTime - fStartTime));
                        string sAvatarSpineCenterLocation = string.Format("{0:F3},{1:F3},{2:F3}", avatarSpineCenter.position.x, avatarSpineCenter.position.y, avatarSpineCenter.position.z);
                        if (ball != null)
                        {
                            sBallPosition = string.Format("{0:F3},{1:F3},{2:F3}", ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
                            sBallOrientation = string.Format("{0:F3},{1:F3},{2:F3}", ball.transform.eulerAngles.x, ball.transform.eulerAngles.y, ball.transform.eulerAngles.z);
                        }
                        else
                        {
                            sBallPosition = string.Format("{0:F3},{1:F3},{2:F3}", 100, 100, 100);
                            sBallOrientation = string.Format("{0:F3},{1:F3},{2:F3}", 100, 100, 100);
                        }

                        string sHeadSetOrientation = string.Format("{0:F3},{1:F3},{2:F3}", headSet.transform.eulerAngles.x, headSet.transform.eulerAngles.y, headSet.transform.eulerAngles.z);
                        string sHeadSetPosition = string.Format("{0:F3},{1:F3},{2:F3}", headSet.transform.position.x, headSet.transform.position.y, headSet.transform.position.z);
                        string sPaddlePosition = string.Format("{0:F3},{1:F3},{2:F3}", paddle.transform.position.x, paddle.transform.position.y, paddle.transform.position.z);
                        string sPaddleOrientation = string.Format("{0:F3},{1:F3},{2:F3}", paddle.transform.eulerAngles.x, paddle.transform.eulerAngles.y, paddle.transform.eulerAngles.z);
                        string rightControllerPosition = string.Format("{0:F3},{1:F3},{2:F3}", rightControllerPositionGameObject.transform.position.x, rightControllerPositionGameObject.transform.position.y, rightControllerPositionGameObject.transform.position.z);
                        string rightControllerOrientation = string.Format("{0:F3},{1:F3},{2:F3}", rightControllerPositionGameObject.transform.eulerAngles.x, rightControllerPositionGameObject.transform.eulerAngles.y, rightControllerPositionGameObject.eulerAngles.z);
                        string leftControllerOrientation = string.Format("{0:F3},{1:F3},{2:F3}", leftControllerPositionGameObject.transform.eulerAngles.x, leftControllerPositionGameObject.transform.eulerAngles.y, leftControllerPositionGameObject.eulerAngles.z);
                        string leftControllerPosition = string.Format("{0:F3},{1:F3},{2:F3}", leftControllerPositionGameObject.transform.position.x, leftControllerPositionGameObject.transform.position.y, leftControllerPositionGameObject.transform.position.z);

                        writer.WriteLine(sAvatarSpineCenterLocation + "," + sBallPosition + "," + sBallOrientation + "," + sPaddlePosition + "," + sPaddleOrientation + "," + sHeadSetPosition + "," + sHeadSetOrientation + "," + rightControllerPosition + "," + rightControllerOrientation + "," + leftControllerPosition + "," + leftControllerOrientation + "@" + sRealTime + "|" + sBodyFrame);

                        if (infoText != null)
                        {
                            infoText.text = string.Format("Recording @ {0}s., frame {1}. Say 'Stop' to stop the player.", sRealTime, fCurrentFrame);
                        }

                        fCurrentFrame++;
                    }
#else
					string sRelTime = string.Format("{0:F3}", (fCurrentTime - fStartTime));
					Debug.Log(sRelTime + "|" + sBodyFrame);
#endif
                }
            }
        }

        if (isPlaying)
        {
            // wait for the right time
            fCurrentTime = Time.time;
            float fRelTime = fCurrentTime - fStartTime;

            if (sPlayLine != null && fRelTime >= fPlayTime)
            {
                // then play the line
                if (manager && sPlayLine.Length > 0)
                {
                    manager.SetBodyFrameData(sPlayLine);
                }

                // and read the next line
                ReadLineFromFile();
            }

            if (sPlayLine == null)
            {
                // finish playing, if we reached the EOF
                StopRecordingOrPlaying();
            }
        }
    }
   
    void OnDestroy()
    {
        // don't forget to release the resources
        isRecording = isPlaying = false;
        CloseFile();

    }

    // reads a line from the file
    private bool ReadLineFromFile()
    {
        

        if (fileReader == null)
            return false;

        // read a line
        sPlayLine = fileReader.ReadLine();
        if (sPlayLine == null)
        {

            isRecording = isPlaying = false;
            AppHelper.Quit();
            return false;

        }

        // extract the unity time and the body frame 
        //int index = sPlayLine.IndexOf(',', sPlayLine.IndexOf(',', sPlayLine.IndexOf(',') + 1) + 1);
        string[] myStrings = sPlayLine.Split('@');

        if (m_Scene.name == "DataAnalysisScene")
        {
            //Debug.Log(myStrings[0]);
            string[] sBallAndSpinePosition = myStrings[0].Split(',');

            float xSpine = float.Parse(sBallAndSpinePosition[0]);
            float ySpine = float.Parse(sBallAndSpinePosition[1]);
            float zSpine = float.Parse(sBallAndSpinePosition[2]);
            avatarSpineCenter.position = new Vector3(xSpine, ySpine, zSpine);

            float xBall = float.Parse(sBallAndSpinePosition[3]);
            float yBall = float.Parse(sBallAndSpinePosition[4]);
            float zBall = float.Parse(sBallAndSpinePosition[5]);
            ball.transform.position = new Vector3(xBall, yBall, zBall);

            float xOrBall = float.Parse(sBallAndSpinePosition[6]);
            float yOrBall = float.Parse(sBallAndSpinePosition[7]);
            float zOrBall = float.Parse(sBallAndSpinePosition[8]);
            ball.transform.rotation = Quaternion.Euler(xOrBall, yOrBall, zOrBall);

            float xPosPaddle = float.Parse(sBallAndSpinePosition[9]);
            float yPosPaddle = float.Parse(sBallAndSpinePosition[10]);
            float zPosPaddle = float.Parse(sBallAndSpinePosition[11]);
            paddle.transform.position = new Vector3(xPosPaddle, yPosPaddle, zPosPaddle);

            float xOrPaddle = float.Parse(sBallAndSpinePosition[12]);
            float yOrPaddle = float.Parse(sBallAndSpinePosition[13]);
            float zOrPaddle = float.Parse(sBallAndSpinePosition[14]);
            paddle.transform.rotation = Quaternion.Euler(xOrPaddle, yOrPaddle, zOrPaddle);

            float xHeadSetPosition = float.Parse(sBallAndSpinePosition[15]);
            float yHeadSetPosition = float.Parse(sBallAndSpinePosition[16]);
            float zHeadSetPosition = float.Parse(sBallAndSpinePosition[17]);
            headSet.position = new Vector3(xHeadSetPosition, yHeadSetPosition, zHeadSetPosition);

            float xHeadSetOrientation = float.Parse(sBallAndSpinePosition[18]);
            float yHeadSetOrientation = float.Parse(sBallAndSpinePosition[19]);
            float zHeadSetOrientation = float.Parse(sBallAndSpinePosition[20]);
            Vector3 headSetOrientation = new Vector3(xHeadSetOrientation, yHeadSetOrientation, zHeadSetOrientation);
            headSet.rotation = Quaternion.Euler(headSetOrientation);

            float xController = float.Parse(sBallAndSpinePosition[21]);
            float yController = float.Parse(sBallAndSpinePosition[22]);
            float zController = float.Parse(sBallAndSpinePosition[23]);
            rightControllerPositionGameObject.position = new Vector3(xController, yController, zController);

            float xOrRight = float.Parse(sBallAndSpinePosition[24]);
            float yOrRight = float.Parse(sBallAndSpinePosition[25]);
            float zOrRight = float.Parse(sBallAndSpinePosition[26]);
            rightControllerPositionGameObject.rotation = Quaternion.Euler(xOrRight, yOrRight, zOrRight);

            float xleftController = float.Parse(sBallAndSpinePosition[27]);
            float yleftController = float.Parse(sBallAndSpinePosition[28]);
            float zleftController = float.Parse(sBallAndSpinePosition[29]);
            leftControllerPositionGameObject.position = new Vector3(xleftController, yleftController, zleftController);

            float xOrLeft = float.Parse(sBallAndSpinePosition[27]);
            float yOrLeft = float.Parse(sBallAndSpinePosition[28]);
            float zOrLeft = float.Parse(sBallAndSpinePosition[29]);
            leftControllerPositionGameObject.rotation = Quaternion.Euler(xOrLeft, yOrLeft, zOrLeft);

        }

        sPlayLine = myStrings[1];
        //Debug.Log(sPlayLine);

        char[] delimiters = { '|' };
        string[] sLineParts = sPlayLine.Split(delimiters);

        if (sLineParts.Length >= 2)
        {
            float.TryParse(sLineParts[0], out fPlayTime);
            sPlayLine = sLineParts[1];
            fCurrentFrame++;

            if (infoText != null)
            {
                infoText.text = string.Format("Playing @ {0:F3}s., frame {1}. Say 'Stop' to stop the player.", fPlayTime, fCurrentFrame);
            }

            return true;
        }

        return false;
    }

    // close the file and disable the play mode
    private void CloseFile()
    {
        // close the file
        if (fileReader != null)
        {
            fileReader.Dispose();
            fileReader = null;
        }

        // disable the play mode
        if (manager)
        {
            manager.EnablePlayMode(false);
        }
        Debug.Log("Total playing time: " + fPlayTime + ", frames: " + fCurrentFrame);
    }

    //C#
    public static class AppHelper
    {
#if UNITY_WEBPLAYER
     public static string webplayerQuitURL = "http://google.com";
#endif
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
        }
    }
}


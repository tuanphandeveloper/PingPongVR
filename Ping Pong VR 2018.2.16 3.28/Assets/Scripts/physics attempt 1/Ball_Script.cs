using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Script : MonoBehaviour {

    [SerializeField] launchBalls launcher;
    // recording data variables 

   
    string outputString;
    float launchingTime;
    public float trialCount;
    int landingPoint1Quarter, landingPoint2Quarter;
    Vector3 launchingAngles, launchingPosition, launchingLinearVelocity, launchingAngularVelocity, landingPoint1, landingPoint1LinearVelocity, landingPoint1AngularVelocity ,
    paddleCollisionPosition, paddleCollisionLinearVelocity, paddleCollisionAngularVelocity, landingPoint2, landingPoint2LinearVelocity, landingPoint2AngularVelocity;

    public bool isHit =false, isTableHit= false, isWritten =false;
    public bool isHitGround = false;

    int tableHitCounter = 0, paddleHitCounter = 0;
   
    public AudioClip paddle_bounce;
    public AudioClip table_bounce;
    public AudioClip floor_bounce;
    public AudioClip hitSound;
    private AudioSource source;
    

    Transform tableNet, targetSign, tableReferencePoint;

    public float dragCoefficient = 0.67f;
    public float airDensity = 1.0f;
    public float ballArea = 4 * 3.14f * 0.002f * 0.002f;
    protected float lastTime;

    protected Vector3 Lift;

    // Trajectory variables 
    public float interval = 0.5f;
    private LineRenderer lineRender;
    private int nVertex = 0;
    private Vector3 lineLastPos;
    Parameters parameters;
    float ballStallTimeStart;
    // Use this for initialization


    // standard dimentions of the table is (274, 152.5, 76)cm (length, width, height) 
    float tableLength = 2.74f, tableWidth = 1.525f;
    int tableQuarter, tableLengthQuarter, tableWidthQuarter; 
    void Start()
    {
        tableReferencePoint = GameObject.FindGameObjectWithTag("tableReferencePoint").transform;
        tableNet = GameObject.FindGameObjectWithTag("Net").transform;
        targetSign = GameObject.FindGameObjectWithTag("targetSign").transform;
        launcher = GameObject.FindGameObjectWithTag("ballLauncher").GetComponent<launchBalls>();

        source = GetComponent<AudioSource>();
        GetComponent<Rigidbody>().maxAngularVelocity = 500.0f;

        parameters = GameObject.Find("Parameters").GetComponent<Parameters>();

        // line renderer
        lineRender = GetComponent<LineRenderer>();
        lineRender.positionCount = 1;
        lineRender.SetPosition(0, transform.position);
        lineLastPos = transform.position;


        float r = Random.Range(0.0f, 1.0f);
        float g = Random.Range(0.0f, 1.0f);
        float b = Random.Range(0.0f, 1.0f);
        Color c1 = Color.red;
        Color c2 = new Color(r, g, b, 1);

        lineRender.startColor = c1;
        lineRender.endColor = c2;
        lineRender.material.color = c2;

        launchingTime = Time.time;
        launchingAngles = gameObject.transform.rotation.eulerAngles;
        launchingPosition = gameObject.transform.position;
        launchingLinearVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        launchingAngularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;
    }


    private void FixedUpdate()
    {
        
        Vector3 ballVelocity = GetComponent<Rigidbody>().velocity;
        //float drag = (airDensity * (ballVelocity.magnitude * ballVelocity.magnitude) * dragCoefficient * ballArea) / 2f;
        //float acceleration = drag / GetComponent<Rigidbody>().mass;
        //Vector3 realAcceleration = -ballVelocity.normalized * acceleration;
        Lift = 2 * Vector3.Cross(4 * Mathf.Pow(3.14f, 2.0f) * Mathf.Pow(0.02f, 3.0f) * GetComponent<Rigidbody>().angularVelocity, airDensity * ballVelocity);
        //Lift = magnusCoefficient * Vector3.Cross(ballVelocity, GetComponent<Rigidbody>().angularVelocity);
        //Debug.Log("Angle: " + Vector3.Angle(Lift, ballVelocity));
        //Debug.Log("Lift Magnitude: " + Lift.magnitude);

        GetComponent<Rigidbody>().AddForce(Lift, ForceMode.Acceleration);
        lastTime = Time.time;
        //}


        if (lineRender && parameters.Projectile)
        {
            if (Vector3.Distance(transform.position, lineLastPos) > interval)
            {
                lineRender.positionCount = nVertex + 1;
                lineRender.SetPosition(nVertex, transform.position);
                lineLastPos = transform.position;
                nVertex++;
            }

        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "paddle")
        {
            source.clip = paddle_bounce;
            source.Play();
            paddleHitCounter++;

            if (paddleHitCounter == 1)
            {
                paddleCollisionPosition = gameObject.transform.position;
                paddleCollisionLinearVelocity = gameObject.GetComponent<Rigidbody>().velocity;
                paddleCollisionAngularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;
                
            }
        }
        else if (collision.gameObject.tag == "table")
        {

            ballStallTimeStart = Time.time;
            source.clip = table_bounce;
            source.Play();
            ContactPoint contact = collision.contacts[0];
            targetSign.position = contact.point;

            tableHitCounter++;


            tableLengthQuarter = (int)Mathf.Ceil(Mathf.Abs(contact.point.z - tableReferencePoint.position.z) / (tableLength / 4));
            tableWidthQuarter = (int)Mathf.Ceil(Mathf.Abs(tableReferencePoint.position.x - contact.point.x) / (tableWidth / 2));
            tableQuarter = 2 * (tableLengthQuarter - 1) + tableWidthQuarter;

            //Debug.Log("z direction collision distance: " + (contact.point.z - tableReferencePoint.position.z));
            //Debug.Log("x direction collision distance: " + (contact.point.x - tableReferencePoint.position.x));
            //Debug.Log("length quarter: " + tableLengthQuarter + " widthQuarter: " + tableWidthQuarter + " table quarter: " + tableQuarter);

            if (tableHitCounter == 1)
            {
                landingPoint1 = gameObject.transform.position;
                landingPoint1LinearVelocity = gameObject.GetComponent<Rigidbody>().velocity;
                landingPoint1AngularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;
                landingPoint1Quarter = tableQuarter;
            }

            if (tableHitCounter == 2)
            {
                landingPoint2 = gameObject.transform.position;
                landingPoint2LinearVelocity = gameObject.GetComponent<Rigidbody>().velocity;
                landingPoint2AngularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;
                landingPoint2Quarter = tableQuarter;
            }


            // it's hit when the quarter is greater than 4 which means the ball collided with the other side of the table. 
            //Debug.Log(" table quarter: " + tableQuarter + "  isHit: "+ isHit+ "  isTableHit: "+ isTableHit);

            if (!launcher.quadRunning)
            {
                if (tableQuarter > 4 && !isHit && !isTableHit)
                {
                    isHit = true;
                    if (parameters.Auto)
                    {
                        --parameters.trialCount;
                        --launcher.currentSessionHits;
                    }
                }
            }
            else
            {
                if(tableQuarter == launcher.hitQuadrant)
                {
                    isHit = true;
                    if (parameters.Auto)
                    {
                        --parameters.trialCount;
                        --launcher.currentSessionHits;
                        source.clip = hitSound;
                        source.Play();
                        launcher.hitQuadrant = Random.Range(5, 8);
                    }
                    
                }                
            }

            if (tableHitCounter > 1)
                isTableHit = true;
        }
        else if (collision.gameObject.tag == "floor")
        {
            source.clip = floor_bounce;
            source.Play();           
            isHitGround = true;
            if (parameters.trial && !isWritten)
            {
                isWritten = true;

            }
            Destroy(this.gameObject, 2.0f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((collision.gameObject.tag == "table" || collision.gameObject.tag == "Net") && (Time.time - ballStallTimeStart) > 3.0f)
        {
            if (parameters.trial && !isWritten)
            {              
                isWritten = true;
            }
            isHitGround = true;
            Destroy(gameObject, 2.0f);
        }
    }

    private void OnDestroy()
    {
        
        outputString += trialCount+","+ launchingTime + "," + launchingPosition.x + "," + launchingPosition.y + "," + launchingPosition.z + "," + launchingAngles.x + "," + launchingAngles.y + "," + launchingAngles.z + "," + launchingLinearVelocity.x + "," + launchingLinearVelocity.y + "," + launchingLinearVelocity.z + "," +
                                launchingAngularVelocity.x + "," + launchingAngularVelocity.y + "," + launchingAngularVelocity.z + "," + landingPoint1.x + "," + landingPoint1.y + "," + landingPoint1.z + "," + landingPoint1LinearVelocity.x + "," + landingPoint1LinearVelocity.y + "," + landingPoint1LinearVelocity.z + "," +
                                landingPoint1AngularVelocity.x + "," + landingPoint1AngularVelocity.y + "," + landingPoint1AngularVelocity.z + "," +landingPoint1Quarter +","+ paddleCollisionPosition.x + "," + paddleCollisionPosition.y + "," + paddleCollisionPosition.z + "," + paddleCollisionLinearVelocity.x + "," + paddleCollisionLinearVelocity.y + "," +
                                paddleCollisionLinearVelocity.z + "," + paddleCollisionAngularVelocity.x + "," + paddleCollisionAngularVelocity.y + "," + paddleCollisionAngularVelocity.z + "," + landingPoint2.x + "," + landingPoint2.y + "," + landingPoint2.z + "," + landingPoint2LinearVelocity.x + "," +
                                landingPoint2LinearVelocity.y + "," + landingPoint2LinearVelocity.z + "," + landingPoint2AngularVelocity.x + "," + landingPoint2AngularVelocity.y + "," + landingPoint2AngularVelocity.z + "," + landingPoint2Quarter+ "," + tableReferencePoint.position.x+ ","+ tableReferencePoint.position.y+ ","+ tableReferencePoint.position.z+ ","+ isHit;



        parameters.writeToFile(outputString);
        
    }
}

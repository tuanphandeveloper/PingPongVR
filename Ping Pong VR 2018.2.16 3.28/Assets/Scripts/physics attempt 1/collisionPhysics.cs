
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;



public class collisionPhysics : MonoBehaviour {


    //public GameObject Ball;
    // public Transform reflectedObject;


    public GameObject PaddleHand;

    protected Hand hand;

    public Rigidbody paddleBody;


    public float ballMass = 0.0027f;
    public float paddleMass = 0.07f;

    protected Vector3 paddleInitialVelocity;
    protected Vector3 paddleFinalVelocity;
    protected Vector3 paddleAcceleration;
    protected Vector3 ballForce;
    protected Vector3 ballInitialVelocity;
    protected Vector3 distance;
    protected Vector3 torque;
    protected Vector3 ballFinalVelocity;
    protected float collisionEnterTime;
    protected float collisionExitTime;
    protected float totalCollisionTime;
    protected float ballRadius = 0.04f;
    protected bool collisionFlag = false;

    Vector3 direction;
    Vector3 finalVelocity;

    private void Start()
    {
        //paddleBody = GetComponent<Rigidbody>();
    }
    protected virtual void Awake()
    {
        hand = PaddleHand.GetComponent<Hand>();
    }


    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Inside Collision Enter!!!");
        if(collision.gameObject.tag == "Ball")
        {
           // Debug.Log("its a ball!");
            paddleInitialVelocity = paddleBody.velocity;
            ballInitialVelocity = collision.gameObject.GetComponent<Rigidbody>().velocity;
            distance = collision.contacts[0].point - collision.gameObject.transform.position;
            collisionEnterTime = Time.time;
        }
        
    }


    private void OnCollisionStay(Collision collision)
    {
      //  Debug.Log("Contact Point Stay: " + collision.contacts[0].point);
      //  Debug.Log("Stay Contacts: " + collision.contacts.Length);

    }

    void OnCollisionExit(Collision collision)
    {
        //paddleFinalVelocity = paddleBody.velocity;
        collisionExitTime = Time.time;
        totalCollisionTime = collisionExitTime - collisionEnterTime;
        paddleAcceleration = (paddleBody.velocity - paddleInitialVelocity) / totalCollisionTime;

        //compute_force_and_torque();
        ballFinalVelocity = (2 * paddleBody.mass / (ballMass + paddleBody.mass) * paddleInitialVelocity) + ((ballMass - paddleBody.mass) / (ballMass + paddleBody.mass) * ballInitialVelocity);
        //torque = Vector3.Cross(paddleAcceleration, distance);
        Debug.Log("Ball Initial Velocity: " + ballInitialVelocity);
        Debug.Log("Ball Final Velocity: " + ballFinalVelocity);
        collision.gameObject.GetComponent<Rigidbody>().AddForce(ballFinalVelocity, ForceMode.VelocityChange);
        //collision.gameObject.GetComponent<Rigidbody>().AddTorque(torque, ForceMode.Acceleration);

        
        //ushort paddleVelocity = (ushort)paddleInitialVelocity.magnitude;
        //paddleVelocity *= 30000;

        //hand.TriggerHapticPulse(paddleVelocity);
    }

    void compute_force_and_torque()
    {  
        
      //  Debug.Log("Ball Mass: " + ballMass);
      //  Debug.Log("Paddle Mass: " + paddleBody.mass);
       // Debug.Log("Ball Initial Velocity: " + ballInitialVelocity);
       // Debug.Log("Paddle Initial Velocity: " + paddleInitialVelocity);

     //   Debug.Log("Ball Final Velocity: " + ballFinalVelocity);
        
      //  Vector3 ballAcceleration = (ballFinalVelocity - ballInitialVelocity) / totalCollisionTime;

      //  Debug.Log("Ball Acceleration: " + ballAcceleration);

        ballForce = ballFinalVelocity;
        
    }
}
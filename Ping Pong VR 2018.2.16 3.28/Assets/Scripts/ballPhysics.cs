using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ballPhysics : MonoBehaviour {

    // Use this for initialization
    [SerializeField] launchBalls launcher;

    bool colliding = false;
    public ballRigidbody ball;
    Rigidbody ballBody;
    Rigidbody tableBody;
    float collisionTime = 0.0f;
    public List<Vector3> ballPositions;
    public List<Vector3> ballOrientations;
    // Sound Variables
    public AudioClip paddle_bounce;
    public AudioClip table_bounce;
    public AudioClip floor_bounce;
    public AudioClip hitSound;
    private AudioSource source;

    // standard dimentions of the table is (274, 152.5, 76)cm (length, width, height) 
    float tableLength = 2.74f, tableWidth = 1.525f;
    public float scale = 0.1f;
    int tableQuarter, tableLengthQuarter, tableWidthQuarter;

    // Trajectory variables 
    public float interval = 0.5f;
    private LineRenderer lineRender;
    private int nVertex = 0;
    private Vector3 lineLastPos;
    Vector3 ballStartPosition;
    Parameters parameters;
    float ballStallTimeStart;
    Transform tableNet, targetSign, tableReferencePoint;

    string outputString;
    float launchingTime;
    public float trialCount;
    int landingPoint1Quarter, landingPoint2Quarter;
    Vector3 launchingAngles, launchingPosition, launchingLinearVelocity, launchingAngularVelocity, landingPoint1, landingPoint1LinearVelocity, landingPoint1AngularVelocity,
    paddleCollisionPosition, paddleCollisionLinearVelocity, paddleCollisionAngularVelocity, landingPoint2, landingPoint2LinearVelocity, landingPoint2AngularVelocity;

    public bool isHit = false, isTableHit = false, isWritten = false;
    public bool isHitGround = false;
    bool stopMoving = false;
    int tableHitCounter = 0, paddleHitCounter = 0;
    float CoefficientOfRestitution = 0.89f;

   // rigibody class declaration
   public class ballRigidbody
   {
        Vector3 currentPosition;
        Quaternion currentOrientation;
        Vector3 angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 velocity;
        float radius = 0.02f;
        float mass = 0.0027f;

        // Moment of Inertia reference: https://en.wikipedia.org/wiki/List_of_moments_of_inertia
        float Inertia = (2 / 3) * 0.0027f * 0.0004f;    //needs a reference 
        Vector3 gravity = new Vector3(0.0f, -9.8f, 0.0f);
        float airDensity = 1.0f;
        // Area = r^2 * pi;
        float area = 0.0012566f;
        // Drag Coefficient : http://scienceworld.wolfram.com/physics/DragCoefficient.html
        float dragCoefficient = 0.5f;
        float dragForce;
        Vector3 dragForceVector;
        Vector3 liftForce;
        Vector3 dragAcceleration;
        Vector3 angularDragAcceleration;
        Vector3 liftAcceleration;
        Vector3 acceleration;
        float angularDragForce;
                

        // Ball rigidbody constructor
        public ballRigidbody(Vector3 currentPosition, Quaternion currentOrientation) {

            this.currentPosition = currentPosition;
            this.currentOrientation = currentOrientation;
        }


        // get and set functions for the values
        public Vector3 getCurrentPosition() { return this.currentPosition; }
        public Quaternion getOrientation() { return this.currentOrientation; }
        public Vector3 getVelocity() { return this.velocity; }
        public Vector3 getAngularVelocity() { return this.angularVelocity; }
        public float getInertia() { return this.Inertia; }
        public float getRadius() { return this.radius; }
        public void setVelocity(Vector3 value) { this.velocity = value; }
        public void setAngularVelocity(Vector3 value) { this.angularVelocity = value; }
        public void setGravity(float value) { this.gravity.y = value; }


        // Kinematic equations used below: https://en.wikipedia.org/wiki/Equations_of_motion


        // Calculate Next Position using velocity
        public void calculateNextPosition()
        { 
            this.currentPosition = this.currentPosition + (this.velocity * Time.deltaTime); //needs a reference 
        }


        // Rotational Kinematic equations: https://en.wikipedia.org/wiki/Rotation_around_a_fixed_axis
        // Calculate Next Orientation using  angular velocity
        public void calculateNextOrientation()
        {
            this.currentOrientation = this.currentOrientation * Quaternion.Euler(this.angularVelocity * Time.deltaTime); //needs a reference 
        }



        // Calculate velocity and angular velocity for the frame using the drag and lift equations
        public void calculateVelocity()
        {

            // Link to drag equation https://en.wikipedia.org/wiki/Drag_equation
            this.dragForce = (airDensity * area * dragCoefficient * this.velocity.magnitude * this.velocity.magnitude) / 2.0f;
            //dragForceVector = (airDensity * area * dragCoefficient * this.velocity * this.velocity) / 2.0f;

            // Link to Lift equation https://www.grc.nasa.gov/WWW/K-12/airplane/beach.html
            liftForce = 4/3 * Vector3.Cross(4 * Mathf.Pow(3.14f, 2.0f) * Mathf.Pow(0.02f, 3.0f) * this.angularVelocity, airDensity * this.velocity);

            liftAcceleration = liftForce / mass;

            this.dragAcceleration = (dragForce / mass) * -this.velocity.normalized;

            // Add up all the accelerations being applied to the ball
            acceleration = gravity + dragAcceleration + liftAcceleration;
            //acceleration = dragAcceleration + liftAcceleration;

            this.velocity = this.velocity + acceleration * Time.deltaTime;
        }


        // Calculate Angular velocity using the drag equation
        public void calculateAngularVelocity()
        {
            this.angularDragForce = (airDensity * area * dragCoefficient *this.angularVelocity.magnitude*radius * this.angularVelocity.magnitude*radius) / 2.0f;

            this.angularDragAcceleration = -(angularDragForce / mass * (this.angularVelocity * radius).normalized);

            this.angularVelocity = this.angularVelocity + (this.angularDragAcceleration / radius) * Time.deltaTime;
        }
    }

    // Creates a new ballRigidbody instance
    public void makeBall()
    {
        ballBody = GetComponent<Rigidbody>();
        ball = new ballRigidbody(ballBody.position, ballBody.rotation);
    }


    void Start () {

        ballBody = GetComponent<Rigidbody>();
        tableBody = GameObject.FindGameObjectWithTag("table").GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        launcher = GameObject.FindGameObjectWithTag("ballLauncher").GetComponent<launchBalls>();
        targetSign = GameObject.FindGameObjectWithTag("targetSign").transform;
        source = GetComponent<AudioSource>();
        parameters = GameObject.Find("Parameters").GetComponent<Parameters>();
        tableReferencePoint = GameObject.FindGameObjectWithTag("tableReferencePoint").transform;

        // line renderer
        lineRender = GetComponent<LineRenderer>();
        lineRender.positionCount = 1;
        lineRender.SetPosition(0, transform.position);
        lineLastPos = transform.position;


        float r = UnityEngine.Random.Range(0.0f, 1.0f);
        float g = UnityEngine.Random.Range(0.0f, 1.0f);
        float b = UnityEngine.Random.Range(0.0f, 1.0f);
        Color c1 = Color.red;
        Color c2 = new Color(r, g, b, 1);

        lineRender.startColor = c1;
        lineRender.endColor = c2;
        lineRender.material.color = c2;
        //makeBall();
    }
	
	// Update is called once per frame
	void Update () {

        ////Debug.Log(ball.getVelocity().magnitude);
        if (ball.getVelocity().magnitude < 0.05f && ball.getCurrentPosition().y > tableReferencePoint.position.y)
        {
            Debug.Log("Ball Position: " + (ball.getCurrentPosition().y - tableReferencePoint.position.y));
        }
        
        if (transform.position.y < -1.5f)
        {
            if (parameters.trial && !isWritten)
            {
                isWritten = true;
            }
            isHitGround = true;
            Destroy(gameObject, 2.0f);
        }
	}

    private void FixedUpdate()
    {

        // Update all ball values every fixed time step
        if (!colliding)
        {
            ball.calculateVelocity();
            ball.calculateAngularVelocity();
            ball.calculateNextPosition();
            ball.calculateNextOrientation();
            if (!stopMoving) {

                //ballBody.MovePosition(ball.getCurrentPosition());
                //ballBody.MoveRotation(ball.getOrientation());
                transform.position = ball.getCurrentPosition();
                transform.rotation = ball.getOrientation();
            }
            
            //ballPositions.Add(ball.getCurrentPosition());
            //ballOrientations.Add(ball.getOrientation().eulerAngles);
           // Debug.Log("forces are being applied");
        }
       
        
        // Used to render a trace line behind the ball
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
       

        //Vector3 velocityDirection = ball.getCurrentPosition() + ball.getVelocity().normalized * 0.2f; 
        //Debug.DrawLine(ball.getCurrentPosition(), velocityDirection , Color.red);
        DrawArrow.ForDebug(ball.getCurrentPosition(), ball.getVelocity().normalized * 0.1f, Color.red, 0.025f, 20f);

        //Debug.Log("Position: " + ball.getCurrentPosition());
        //Debug.Log("velocity: " + ball.getVelocity());
        //Debug.Log("velocity normalized: " + (ball.getVelocity().normalized * 0.2f));
        //Debug.Log("velocityDirection: " + velocityDirection);


    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision!");
        colliding = true;


        Vector3 distance = collision.contacts[0].point - transform.position;

        Vector3 initialBallVelocity = ball.getVelocity();

        if (collision.gameObject.tag == "table")
        {
            source.clip = table_bounce;
            source.Play();
           

            Debug.Log("Ball Position: " + (ball.getCurrentPosition().y - tableReferencePoint.position.y));
            // This condition keeps the ball from falling through the table.
            Debug.Log("initial Y: " + initialBallVelocity.y);
            if (initialBallVelocity.y < 0.2f && initialBallVelocity.y > -0.2f) 
            {
                ballBody.useGravity = true;
                stopMoving = true;
            }
            else
            {
                // Conservation of momentum  V1 = ((2*M2)/(M1 +M2))*U2 + (M1 - M2)/(M1 + M2))*U1 Link: https://en.wikipedia.org/wiki/Momentum
                //Vector3 ballVelocity = (2 * 100.0f / (ballBody.mass + 100.0f) * new Vector3(0.0f, 0.0f, 0.0f) + ((ballBody.mass - 100.0f) / (ballBody.mass + 100.0f) * ball.getVelocity()));
                Vector3 ballVelocity = (ballBody.mass * ball.getVelocity() + tableBody.mass* tableBody.velocity + tableBody.mass * CoefficientOfRestitution * (tableBody.velocity - ball.getVelocity()))/(tableBody.mass + ballBody.mass);
                // The equation being used down below accounts for the angular velocity of the ball during collision with the table.
                // This was provided by my friend Avrind. V1 = (Vx, -Vy, Vz) + k(Wz, 0, Wx);
                ball.setVelocity(new Vector3(-ballVelocity.x, ballVelocity.y, -ballVelocity.z) + scale*new Vector3(ball.getAngularVelocity().z, 0.0f, ball.getAngularVelocity().x));
            }

            ballStallTimeStart = Time.time;
            ContactPoint contact = collision.contacts[0];
            targetSign.position = contact.point;

            tableHitCounter++;

            tableLengthQuarter = (int)Mathf.Ceil(Mathf.Abs(contact.point.z - tableReferencePoint.position.z) / (tableLength / 4));
            tableWidthQuarter = (int)Mathf.Ceil(Mathf.Abs(tableReferencePoint.position.x - contact.point.x) / (tableWidth / 2));
            tableQuarter = 2 * (tableLengthQuarter - 1) + tableWidthQuarter;

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
                if (tableQuarter == launcher.hitQuadrant)
                {
                    isHit = true;
                    if (parameters.Auto)
                    {
                        --parameters.trialCount;
                        --launcher.currentSessionHits;
                        source.clip = hitSound;
                        source.Play();
                        launcher.hitQuadrant = UnityEngine.Random.Range(5, 8);
                    }

                }
            }

            if (tableHitCounter > 1)
                isTableHit = true;

        }
        else if(collision.gameObject.tag == "paddle")
        {

            if((Time.time - collisionTime) > 0.1f)
            {
                collisionTime = Time.time;
                source.clip = paddle_bounce;
                source.Play();
                paddleHitCounter++;

                if (paddleHitCounter == 1)
                {
                    paddleCollisionPosition = gameObject.transform.position;
                    paddleCollisionLinearVelocity = gameObject.GetComponent<Rigidbody>().velocity;
                    paddleCollisionAngularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity;

                }

                //Debug.Log("initial Y: " + initialBallVelocity.y);
                // This condition keeps the ball from falling through the paddle. Almost never gets used
                if ((initialBallVelocity.magnitude < 0.002f && transform.position.y < 0.3f) || transform.position.y < 0.3f || (initialBallVelocity.magnitude > -0.002f && transform.position.y < 0.3f))
                {
                    ballBody.useGravity = true;
                    stopMoving = true;
                }
                else
                {
                    // An overall overview https://prezi.com/nuqjbj6flhvf/the-physics-of-ping-pong/ 
                    // ball equations https://aapt.scitation.org/doi/10.1119/1.4964104 
                    // 3D video game Physics: http://chrishecker.com/images/b/bb/Gdmphys4.pdf
                    // http://protabletennis.net/content/mechanics-table-tennis
                    // https://ehttps://www.thoughtco.com/physics-of-table-tennis-3173598n.wikipedia.org/wiki/Bouncing_ball
                    // https://www.thoughtco.com/physics-of-table-tennis-3173598
                    // https://ipfs.io/ipfs/QmXoypizjW3WknFiJnKLwHCnL72vedxjQkDDP1mXWo6uco/wiki/Angular_velocity.html
                    // paper about table tennis simulation https://www.researchgate.net/publication/258567952_Computer_simulation_of_table_tennis_ball_trajectories_for_studies_of_the_influence_of_ball_size_and_net_height
                    // first version of ping pong simulator  https://www.reddit.com/r/Vive/comments/4ryidv/eleven_table_tennis_simulator_prealpha/ 
                    // ping pong physics simulation with the code http://sonic.net/~goddard/home/spin/docs/spin.html 
                    var paddle = collision.gameObject.GetComponent<paddleMovement>();
                    Vector3 totalVelocity = initialBallVelocity - paddle.paddleCurrentVelocity;

                    Vector3 R = collision.contacts[0].point - ballBody.position;



                    Vector3 normalVelocity = Vector3.Dot(totalVelocity, R) * R.normalized;
                    Vector3 tangVelocity = totalVelocity - normalVelocity;

                    Vector3 paddleLinearVelocity = Vector3.Dot(paddle.paddleCurrentVelocity, R) * R.normalized;
                    Vector3 paddleAngularVelocity = (paddle.paddleCurrentVelocity - paddleLinearVelocity).normalized * paddleLinearVelocity.magnitude;
                    paddleLinearVelocity = (paddle.paddleCurrentVelocity.magnitude - paddleAngularVelocity.magnitude) * R.normalized;

                    // visualizing transfered linear and angular velocity components to the ball from the paddle veleocity  
                    DrawArrow.ForDebug(collision.contacts[0].point, paddleLinearVelocity.normalized * 0.1f, Color.yellow, 0.025f, 20f);
                    DrawArrow.ForDebug(collision.contacts[0].point, paddleAngularVelocity.normalized * 0.1f, Color.cyan, 0.025f, 20f);

                    // show paddle velocity from the contact point reference. paddleMovment script still shows the velocity from paddle refernce point                
                    DrawArrow.ForDebug(collision.contacts[0].point, paddle.paddleCurrentVelocity.normalized * 0.1f, Color.black, 0.025f, 20f);

                    // visualizing R   
                    DrawArrow.ForDebug(ballBody.position, R.normalized * 0.1f, Color.blue, 0.025f, 20f);


                    Vector3 eHat = (Vector3.Cross(R, ball.getAngularVelocity()) - tangVelocity).normalized;

                    //Vector3 fHat = Vector3.Cross(R, eHat) / R.magnitude;
                    Vector3 fHat = Vector3.Cross(R, eHat).normalized;
                    //Debug.Log("FHat: " + fHat);
                    //Debug.Log("EHat: " + eHat);
                    //Debug.Log("normal Velocity: " + normalVelocity.magnitude);

                    //Vector3 angularVelocity = ball.getAngularVelocity() + (2 * ball.getRadius() * ballBody.mass * 0.  * normalVelocity.magnitude / ball.getInertia()) * fHat;
                    Vector3 angularVelocity = ball.getAngularVelocity() + (2 * 0.00104f * normalVelocity.magnitude / ball.getRadius()) * fHat;
                    Debug.Log("ball Velocity: " + ball.getVelocity() + "magnitude: " + ball.getVelocity().magnitude);
                    Debug.Log("ball Angular Velocity: " + angularVelocity + "magnitude: " + angularVelocity.magnitude);
                    Debug.Log("paddle Velocity: " + paddle.paddleCurrentVelocity + "magnitude: " + paddle.paddleCurrentVelocity.magnitude);
                    Debug.Log("paddle Linear Velocity: " + paddleLinearVelocity + " magnitude: " + paddleLinearVelocity.magnitude);
                    Debug.Log("paddle Angular Velocity: " + paddleAngularVelocity + " magnitude: " + paddleAngularVelocity.magnitude);

                    // Equation to calculate ball reflection  R = I - 2*(N·I)*N Link: https://www.gamedev.net/forums/topic/78083-how-to-calculate-bounce-angle/
                    Vector3 direction = ball.getVelocity() - (2 * Vector3.Dot(collision.contacts[0].normal, ball.getVelocity()) * collision.contacts[0].normal);

                    // Potential solution for paddle collision https://physics.stackexchange.com/questions/11686/finding-angular-velocity-and-regular-velocity-when-bouncing-off-a-surface-with-f
                    // Conservation of momentum  V1 = ((2*M2)/(M1 +M2))*U2 + (M1 - M2)/(M1 + M2))*U1 Link: https://en.wikipedia.org/wiki/Momentum
                    //Vector3 ballVelocity = 2 * paddle.mass / (ballBody.mass + paddle.mass) * paddle.velocity + ((ballBody.mass - paddle.mass) / (ballBody.mass + paddle.mass) * ball.getVelocity());
                    Vector3 ballVelocity = (ballBody.mass * ball.getVelocity() + paddle.mass * paddle.paddleCurrentVelocity + paddle.mass * 0.77f * (paddle.paddleCurrentVelocity - ball.getVelocity())) / (paddle.mass + ballBody.mass);
                    // Paddle velocity cross CoM vector gives how much of the paddles velocity is tangential to the ball.
                    //Vector3 angularVelocity = Vector3.Cross(distance, paddle.velocity);

                    ball.setAngularVelocity(angularVelocity);
                    //Debug.Log(3f * 1.1f * normalVelocity.magnitude / ball.getRadius());


                    ball.setVelocity(ballVelocity.magnitude * direction.normalized);

                    // The code below was an attempt at http://cgraney.jctcfaculty.org/cmgresearch/PhysicsAstro/pong.pdf
                    //float velocityMagnitude = Mathf.Sqrt(ball.getVelocity().magnitude * ball.getVelocity().magnitude + (2.0f / 5.0f) * (ball.getRadius() * ball.getRadius()) * (ball.getAngularVelocity().magnitude * ball.getAngularVelocity().magnitude));
                    //float balltheta = Mathf.Atan(Mathf.Sqrt(2.0f / 5.0f) * ball.getRadius() * ball.getAngularVelocity().magnitude / ball.getVelocity().magnitude);
                    //Vector3 ballDirection = ball.getVelocity() / Mathf.Cos(balltheta);
                    //Vector3 inAngVel = Vector3.Cross(paddle.velocity, distance);
                    //Vector3 ballVelocity = ball.getVelocity() + parameterJ * collision.contacts[0].normal / ballBody.mass;
                    //Vector3 ballAngularVelocity = ball.getAngularVelocity() + (Vector3.Cross(distance, parameterJ * collision.contacts[0].normal)) / (float)ball.getInertia();

                    //ball.setVelocity(ballVelocity.magnitude * (-direction.normalized));

                }
            }
            
        }
        else if(collision.gameObject.tag == "Net")
        {
            Vector3 ballVelocity = (ballBody.mass * ball.getVelocity() + 100.0f * Vector3.zero + 100.0f * 0.2f * (Vector3.zero - ball.getVelocity())) / (100.0f + ballBody.mass);
            // Paddle velocity cross CoM vector gives how much of the paddles velocity is tangential to the ball.

            ball.setVelocity(ballVelocity);
        }
        else if (collision.gameObject.tag == "floor")
        {
            Destroy(this.gameObject, 2.0f);
        }

        colliding = false;

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

        outputString += trialCount + "," + launchingTime + "," + launchingPosition.x + "," + launchingPosition.y + "," + launchingPosition.z + "," + launchingAngles.x + "," + launchingAngles.y + "," + launchingAngles.z + "," + launchingLinearVelocity.x + "," + launchingLinearVelocity.y + "," + launchingLinearVelocity.z + "," +
                                launchingAngularVelocity.x + "," + launchingAngularVelocity.y + "," + launchingAngularVelocity.z + "," + landingPoint1.x + "," + landingPoint1.y + "," + landingPoint1.z + "," + landingPoint1LinearVelocity.x + "," + landingPoint1LinearVelocity.y + "," + landingPoint1LinearVelocity.z + "," +
                                landingPoint1AngularVelocity.x + "," + landingPoint1AngularVelocity.y + "," + landingPoint1AngularVelocity.z + "," + landingPoint1Quarter + "," + paddleCollisionPosition.x + "," + paddleCollisionPosition.y + "," + paddleCollisionPosition.z + "," + paddleCollisionLinearVelocity.x + "," + paddleCollisionLinearVelocity.y + "," +
                                paddleCollisionLinearVelocity.z + "," + paddleCollisionAngularVelocity.x + "," + paddleCollisionAngularVelocity.y + "," + paddleCollisionAngularVelocity.z + "," + landingPoint2.x + "," + landingPoint2.y + "," + landingPoint2.z + "," + landingPoint2LinearVelocity.x + "," +
                                landingPoint2LinearVelocity.y + "," + landingPoint2LinearVelocity.z + "," + landingPoint2AngularVelocity.x + "," + landingPoint2AngularVelocity.y + "," + landingPoint2AngularVelocity.z + "," + landingPoint2Quarter + "," + tableReferencePoint.position.x + "," + tableReferencePoint.position.y + "," + tableReferencePoint.position.z + "," + isHit;



        parameters.writeToFile(outputString);
    }
}

// ================================================================================ //
// AUTONOMOUS AGENTS                                                                //
// -------------------------------------------------------------------------------- //
// This script contains logic that enables an Agent to avoid obstacles,             //
// flee from enemies and move freely around the scene.                              //
// -------------------------------------------------------------------------------- //
// Author:  Joseph Breslin (2019)                                                   //
// ================================================================================ //

using UnityEngine;
using System.Collections;

public class AutonomousAgent : MonoBehaviour
{
    // Agent Properties
    public float mass = 1;
    public float maxSpeed = 5.0f;
    Vector3 force = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;                
    bool isPlayer = false;                              // Flag to determine if this agent is the player 

    // Wander behaviour properties
    public float wanderTime = 3f;                       // 'wanderTime' is the time in which the wander target swivel updates rotation
    Quaternion wanderRotation = Quaternion.identity;    // Assign to the swivel
    Transform wanderSwivel;                             
    Transform wanderTargetTransform = null;

    // Flee and arrive properties
    public float slowingDistance = 3.0f;                // Used to determine at what point the agent will slow down
    Transform fleeTargetTransform = null;
    float deceleration = 0.9f;                          // Scaler for reducing acceleration

    // Obstacle Avoidance Properties
    float maxSight = 2.5f;                              
    enum AvoidanceStates { LEFT, RIGHT, NONE};         
    bool isAvoiding = false;                            // Used to determine if the agent must avoid an obstacle

    // Weights used to determine the strength of each behaviour
    public float wanderWeight = .5f;
    public float fleeArriveWeight = 1f;
    public float obstacleAvoidanceWeight = 2f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);              // Assign Random Rotation

        wanderSwivel = transform.GetChild(0);                                           // Assign child object to act as the wander swivel    
        wanderTargetTransform = wanderSwivel.GetChild(0);                               // Assign wander swivel child object to act as the wander target
        StartCoroutine(StartRotation());                                                // Begin wander rotation protocol

        if(GameObject.FindGameObjectWithTag("Player") == this.gameObject)
        {
            isPlayer = true;
        }
        fleeTargetTransform = GameObject.FindGameObjectWithTag("Player").transform;     // Assign the player tagged object transform as the 'fleeTargetTransform. 
    }                                                                                   // This is only used if the player is not this gameObject.

    void Update()
    {
        force = Calculate();                                                            // Force is the sum of all behaviour output functions.
        AccelerationSmoothing();                                                        // Function to smoothen the acceleration value.
        ApplyAccelerationToVelocity();                                                  // Apply's acceleration to the velocity value, also clamps to 'maxSpeed' 
        ApplyVelocityLookAt();                                                          // Applies the velocity value to the transform position of this gameObject over time.     
    }                                                                                   // This also uses the look at function to point towards the target.

    private void FixedUpdate()
    {
        ObstacleAvoidanceRayCasting();                                                  // Use raycasting to determine the avoidance direction and assign avoidance force
    }

    Vector3 Calculate()                                                                 // Calculate is the sum of all behaviour outputs and informs the agent's force value.
    {
        Vector3 force = Vector3.zero;
        force += Wander() * wanderWeight;
        force += CalculateObstacleAvoidance() * obstacleAvoidanceWeight;
        if (!isPlayer)                                                                  // If this agent is the player tagged object then it will not flee from the player tagged object. 
        {
            force += FleeArrive() * fleeArriveWeight;
        }
        return force;
    }

    void AccelerationSmoothing()
    {
        Vector3 tempAcceleration = force / mass;                                        
        float smoothRate = Mathf.Clamp(9.0f * Time.deltaTime, 0.15f, 0.4f) / 2.0f;
        acceleration = Vector3.Lerp(acceleration, tempAcceleration, smoothRate);        // 'acceleration' is lerped to 'tempAcceleration', utilising a clamped smooth rate.
    }

    void ApplyAccelerationToVelocity()
    {
        velocity += acceleration * Time.deltaTime;                                      // Acceleration is added to velocity, the magnitude of velocity is then clamped to the 'maxSpeed'
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    void ApplyVelocityLookAt()
    {
        if (velocity.magnitude > 0.0001f)
        {
            transform.LookAt(transform.position + velocity, transform.up);              // Agent's rotation is determined using the lookAt function.
            velocity *= 0.99f;                                                          // Decelerate agent
        }
        transform.position += velocity * Time.deltaTime;                                // Agent's position is updated over time, by adding velocity multiplied by the time delta to the position.
    }

    IEnumerator StartRotation()                                                         // Use this to trigger wander rotation every n seconds
    {
        UpdateWanderSwivelRotation();
        yield return new WaitForSeconds(wanderTime);
        StartCoroutine(StartRotation());
    }
    void UpdateWanderSwivelRotation()
    {
        wanderRotation.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        wanderSwivel.rotation = wanderRotation;                                         // Update wanderSwivel rotation with a random rotation
    }

    Vector3 Wander()                                                                    // Returns the wanderValue used to determine the agents movement direction 
    {
        Vector3 desiredVelocity = wanderTargetTransform.position - transform.position;             
        desiredVelocity.Normalize();
        desiredVelocity *= maxSpeed;
        return desiredVelocity - velocity;
    }

    Vector3 FleeArrive()
    {
        Vector3 toTarget = transform.position - fleeTargetTransform.position;           // Determine the distance to the flee target
        float distance = toTarget.magnitude;
        if (distance > slowingDistance)
        {
            return Vector3.zero;                                                        // Return zero velocity if the agent is out of flee range.
        }
        float rampedSpeed = maxSpeed * (distance / slowingDistance * deceleration);     
        float clamped = Mathf.Min(rampedSpeed, maxSpeed);
        Vector3 desiredVelocity = clamped * (toTarget / distance);
        return desiredVelocity - velocity;
    }
                                                                                       
    Vector3 CalculateObstacleAvoidance()                                                // Change direction of agent when it is avoiding an obstacle
    {
        Vector3 avoidanceForce = Vector3.zero;
        if (isAvoiding)
        {
            avoidanceForce = (transform.forward * (-1 * maxSight));                     
        }
        avoidanceForce.Normalize();
        avoidanceForce *= maxSpeed;
        return avoidanceForce;
    }
                                                                                        // Two Raycasts are used, one from each shoulder facing forward
    void ObstacleAvoidanceRayCasting()
    {
        isAvoiding = false;
        RaycastHit leftHit;
        RaycastHit rightHit;

        Vector3 leftBumper = transform.position - transform.right * 0.5f;
        if (Physics.Raycast(leftBumper, transform.forward, out leftHit, maxSight))      // Flag if contact is made with left raycast
        {
            isAvoiding = true;
        }                                                                 
        Vector3 rightBumper = transform.position + transform.right * 0.5f;
        if (Physics.Raycast(rightBumper, transform.forward, out rightHit, maxSight))    // Flag if contact is made with right raycast
        {
            isAvoiding = true;
        }
    }



}
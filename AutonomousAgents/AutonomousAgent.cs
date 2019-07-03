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
    //AGENT
    Vector3 force = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;
    public float mass = 1;
    public float maxSpeed = 5.0f;
    bool isPlayer = false;

    //WANDER
    public float wanderTime = 3f;
    Quaternion wanderRotation = Quaternion.identity;              //Assign to the swivel
    Transform wanderSwivel;
    Transform wanderTargetTransform = null;

    //FleeArrive
    public float slowingDistance = 3.0f;
    Transform fleeTargetTransform = null;
    float deceleration = 0.9f;

    //OBSTACLE AVOIDANCE
    float maxSight = 2.5f;
    enum AvoidanceStates { LEFT, RIGHT, NONE};
    AvoidanceStates avoidanceState = AvoidanceStates.NONE;

    //WEIGHTS
    public float wanderWeight = .5f;
    public float fleeArriveWeight = 1f;
    public float obstacleAvoidanceWeight = 2f;

    void Start()
    {

        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);                  //Assign Random Rotation

        wanderSwivel = transform.GetChild(0);
        wanderTargetTransform = wanderSwivel.GetChild(0);
        StartCoroutine(StartRotation());

        if(GameObject.FindGameObjectWithTag("Player") == this.gameObject)
        {
            isPlayer = true;
        }
        fleeTargetTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        force = Calculate();
        Banking();
    }

    private void FixedUpdate()
    {
        ObstacleAvoidanceRayCasting();
    }

    #region Wander Rotation
    IEnumerator StartRotation()
    {
        RotateSwivel();
        yield return new WaitForSeconds(wanderTime);
        StartCoroutine(StartRotation());
    }
    void RotateSwivel()
    {
        wanderRotation.eulerAngles = GetEulerAngle();
        wanderSwivel.rotation = wanderRotation;
    }

    Vector3 GetEulerAngle()
    {
        Vector3 newRotation = new Vector3(0, Random.Range(0, 360), 0);
        return newRotation;
    }
    #endregion

    Vector3 Wander()
    {
        Vector3 desired = wanderTargetTransform.position - transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        return desired - velocity;
    }

    Vector3 FleeArrive()
    {
        Vector3 toTarget = transform.position - fleeTargetTransform.position;
        float distance = toTarget.magnitude;
        if (distance > slowingDistance)
        {
            Debug.DrawLine(transform.position, fleeTargetTransform.position, Color.cyan);
            return Vector3.zero;
        }
        else
        {
            Debug.DrawLine(transform.position, fleeTargetTransform.position, Color.red);
        }
        float rampedSpeed = maxSpeed * (distance / slowingDistance * deceleration);
        float clamped = Mathf.Min(rampedSpeed, maxSpeed);
        Vector3 desired = clamped * (toTarget / distance);
        return desired - velocity;
    }

    Vector3 Calculate()
    {
        force = Vector3.zero;
        force += Wander() * wanderWeight;
        force += CalculateObstacleAvoidance() * obstacleAvoidanceWeight;
        if(!isPlayer)
            force += FleeArrive() * fleeArriveWeight;
        return force;
    }
    
    #region Banking
    void Banking()
    {
        AccelerationSmoothing();
        ApplyAccelerationToVelocity();
        ApplyLerpAndLookAt();
    }
    void AccelerationSmoothing()
    {
        Vector3 tempAcceleration = force / mass;
        float smoothRate = Mathf.Clamp(9.0f * Time.deltaTime, 0.15f, 0.4f) / 2.0f;
        acceleration = Vector3.Lerp(acceleration, tempAcceleration, smoothRate);
    }
    void ApplyAccelerationToVelocity()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }
    void ApplyLerpAndLookAt()
    {
        float smoothRate = Time.deltaTime * 3.0f;
        if (velocity.magnitude > 0.0001f)
        {
            Debug.DrawLine(transform.position, transform.position + velocity, Color.magenta);
            transform.LookAt(transform.position + velocity, transform.up);
            velocity *= 0.99f;
        }
        transform.position += velocity * Time.deltaTime;
    }


    #endregion

    Vector3 CalculateObstacleAvoidance()
    {
        Vector3 avoidanceForce = Vector3.zero;
        switch (avoidanceState)
        {
            case AvoidanceStates.NONE:
                avoidanceForce = Vector3.zero;
                break;
            case AvoidanceStates.RIGHT:
                //Turn Left to avoid right
                avoidanceForce = (transform.forward * (-1 * maxSight)); //+ (transform.right * maxSight * -1); //(maxSight/2) *
                break;
            case AvoidanceStates.LEFT:
                //turn right to avoid left
                avoidanceForce = (transform.forward * (-1 * maxSight));// + (transform.right * maxSight);
                break;
            default:
                avoidanceForce = Vector3.zero;
                break;
        }
        avoidanceForce.Normalize();
        //avoidanceForce *= maxSpeed;
        return avoidanceForce;
    }

    void ObstacleAvoidanceRayCasting()
    {
        bool leftContact = false;
        bool rightContact = false;
        float offset = 0.5f;
        float leftLength = maxSight;
        float rightLength = maxSight;

        //Left Bumper
        RaycastHit leftHit;
        Vector3 leftBumper = transform.position - transform.right * offset;
        if (Physics.Raycast(leftBumper, transform.forward, out leftHit, maxSight))
        {
            Debug.DrawRay(leftBumper, transform.forward * maxSight, Color.red);
            leftContact = true;
            leftLength = Vector3.Distance(leftBumper, leftHit.point);
        }
        else
        {
            Debug.DrawRay(leftBumper, transform.forward * maxSight, Color.yellow);
        }

        //Right Bumber
        RaycastHit rightHit;
        Vector3 rightBumper = transform.position + transform.right * offset;
        if (Physics.Raycast(rightBumper, transform.forward,out rightHit, maxSight))
        {
            Debug.DrawRay(rightBumper, transform.forward * maxSight, Color.red);
            rightContact = true;
            rightLength = Vector3.Distance(rightBumper, rightHit.point);
        }
        else
        {
            Debug.DrawRay(rightBumper, transform.forward * maxSight, Color.yellow);
        }
   
        if(!rightContact && !leftContact)
        {
            avoidanceState = AvoidanceStates.NONE;
        }
        if (rightContact && leftContact)
        {
            //check to see which ray is longer
            if(leftLength > rightLength)
                avoidanceState = AvoidanceStates.LEFT;
            else
                avoidanceState = AvoidanceStates.RIGHT;

        }
        if (rightContact && !leftContact)
        {
            avoidanceState = AvoidanceStates.RIGHT;
        }
        if (leftContact && !rightContact)
        {
            avoidanceState = AvoidanceStates.LEFT;
        }
    }
}
// ================================================================================ //
// PLAYER MOVEMENT                                                                  //
// -------------------------------------------------------------------------------- //
// This script handles 2D character movement and the associated                     //
// sprite animation.                                                                //
// This script makes use of the Animator, Transform and Rigidbody2D components.     //
// There is a flag to toggle between Ridgidbody2D and direct transform updates.     //
// -------------------------------------------------------------------------------- //
// Author:  Joseph Breslin (2019)                                                   //
// ================================================================================ //

using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    enum Direction { DOWN=0, UP=1, LEFT=2, RIGHT=3 };             
    Direction   direction;                              // Use direction enum to clearly denote the animator's integer parameter.
    Animator    animator;                                                                  
    Rigidbody2D rb2D;                                             
    Vector3     move;                                   // 'move' is used to apply velocity when Rigidbody2D is utilised.
                                                        // 'move' is used as an additive to the player's position when Rigidbody2D is not utilised.
    float   playerSpeed;                                // 'playerSpeed' is utilised as a multiplier to 'move'. It is updated by sprint input.     
    bool    isRunning = false;

    [Range(0, 20)] public float walkSpeed = 1.5f;       // Designer sets walk speed in editor, default is 1.5. 
    [Range(0, 20)] public float sprintSpeed = 3f;       // Designer sets sprint speed in editor, default is 3.

    public float    walkAnimationSpeed = 1f;            // Animation speed settings for walking.
    public float    sprintAnimationSpeed = 1.2f;        // Animation speed settings for sprinting.

    public bool     isRigidBody = false;                // Flag to disable/enable rigidbody movement. Transform component updates are applied if 'isRigidBody' is set to false. 
    public string   sprintInput = "run";                // Input setting for sprint button (assigned to left shift).        
    public string   xAxis = "Horizontal";               // Input setting for horizontal movement.   
    public string   yAxis = "Vertical";                 // Input setting for vertical movement. 

    private void Start()
    {
        animator = GetComponent();
        rb2D = GetComponent();
        playerSpeed = walkSpeed;                        //Initialise 'playerSpeed' with the 'walkSpeed' value.
    }

    private void Update()
    {
        isRunning = Input.GetButton(sprintInput);       //Continuesly poll for sprint input and assign the value to 'isRunning'.
    }

    void FixedUpdate()
    {                                                   //Use fixed update to avoid frame rate variance of player movement.
        if (isRunning)                                  //Check to see if sprint mechanic is enabled.
        {
            playerSpeed = sprintSpeed;                  //Assign 'sprintSpeed' to 'playerSpeed' and update the animator paramater 'Speed' with the sprint animation speed value.
            animator.SetFloat("Speed", sprintAnimationSpeed);
        }
        else
        {
            playerSpeed = walkSpeed;                    //Assign 'walkSpeed' to 'playerSpeed' and update the animator paramater 'Speed' with the walk animation speed value.
            animator.SetFloat("Speed", walkAnimationSpeed);
        }

        //INPUT                                         
        move = new Vector3(Input.GetAxisRaw(xAxis), Input.GetAxisRaw(yAxis));   // Assign the input axis values to the x and y paramaters of 'move'.
        move = (move.magnitude > 1.0f) ? move = move.normalized : move;         // Check to see if the magnitude of the move vector is greater than one. 
                                                                                // If so the player will move too fast in diagonal directions.
                                                                                // Reduce peaks in velocity by normalizing the vector when the magnitude is greater than 1.
        //ANIMATION
        bool isMoving;
        isMoving = move.magnitude < .00001 ? false : true;                      //Return false if player movement input has ceased.
        animator.SetBool("Is_Moving", isMoving);                                //Update the Animator parameter that enables movement animations.   

        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))      //Determine and assign the appropriate direction by checking 'move' x and y co-ordinates.
        {
            if (move.x > 0)
            {
                direction = Direction.RIGHT;
            }
            else if (move.x < 0)
            {
                direction = Direction.LEFT;
            }
        }
        else
        {
            if (move.y > 0)
            {
                direction = Direction.UP;
            }
            else if (move.y < 0)
            {
                direction = Direction.DOWN;
            }
        }

        int dir = (int)direction;                           // Enum integer conversion.
        animator.SetInteger("Direction", dir);              // Set Animator parameter to update the direction in which the player animation will face.

        //MOVEMENT
        if (!isRigidBody)                                   // When Rigidbody2D is disabled: Multiply 'move' by the time delta and 'playerSpeed'. 
        {                                                   // Then add this value to the player transform component position, so to move the player.
            transform.position += move * Time.deltaTime * playerSpeed;
        }
        else
        {
            if (!isMoving)                                  // When Rigidbody2D is enabled: Multiply 'move' by the 'playerSpeed'. Then if the "isMoving" flag is true, assign this value to the rigidBody's velocity.
            {
                rb2D.velocity = Vector3.zero;               // When the "isMoving" flag is false, Assign a vector3 of (0,0,0) to the rigidBody's velocity thus halting player movement.
            }
            else
            {
                rb2D.velocity = move * playerSpeed;
            }
        }
    }
}

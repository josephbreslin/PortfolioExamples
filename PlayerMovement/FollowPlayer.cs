// ================================================================================ //
// CAMERA FOLLOW                                                                    //
// -------------------------------------------------------------------------------- //
// This script re-positions the camera to focus on the player position.             //
// The player is denoted via tag.                                                   //
// This script makes use of the Transform component.                                //
// -------------------------------------------------------------------------------- //
// Author:  Joseph Breslin (2019)                                                   //
// ================================================================================ //

using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Used to move towards 'Player' tagged object")]

    [Tooltip("LerpSpeed is a scaler for the lerp function that alligns camera position with player position"), Range(0, 10)]
    public float lerpSpeed = 1;

    [Tooltip("slerpSpeed is a scaler for the slerp function that alligns camera rotation with player rotation"), Range(0, 10)]
    public float slerpSpeed = 1;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent();
    }

    void Update()
    {
        AlignWithPlayer();
    }

    void AlignWithPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z), Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, Time.deltaTime * slerpSpeed);
    }
}
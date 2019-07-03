// ================================================================================ //
// TOGGLE SPRITE LAYER                                                              //
// -------------------------------------------------------------------------------- //
// This script updates the spriterenderer's sorting order when a tagged Object      //
// moves above or below a specified point.                                          //
// -------------------------------------------------------------------------------- //
// Author:  Joseph Breslin (2019)                                                   //
// ================================================================================ //

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ToggleSpriteLayer : MonoBehaviour {

    Transform taggedObjectTransform;
    SpriteRenderer spriteRenderer;

    // The ToggleSpriteLayerClassifier denotes the object's layer classification.
    public enum ToggleSpriteLayerClassifier {   OBJ_BACK = 6,   OBJ_2 = 7,      OBJ_3 = 8,
                                                OBJ_FRONT = 9,  NPC_BACK = 10,  NPC_2 = 11,
                                                NPC_3 = 12,     NPC_4 = 13,     NPC_FRONT = 14 };

    // The 'ToggleLayerType' is this objects layer classification.
    public ToggleSpriteLayerClassifier ToggleLayerType = ToggleSpriteLayerClassifier.OBJ_BACK;

    public string taggedObjectStr = "Player";               // String used to find the tagged object.

    public float offsetY;                                   // The Y offset added to object position for more precise toggling.
    public bool takeOffsetFromCollider = false;             // Flag to check if the offset y value is to be taken from an associated collider2D.

    [HideInInspector()]
    public int backLayerOrder, frontLayerOrder;             // Back Layer is always equal to the front layer minus 15. Front layer is equal the integer value of 'ToggleLayerType'.

    [HideInInspector()]
    public bool isNPC = false;                              // This flag is equal to true if this object is an NPC or equal to falseif it is not an NPC.

    void Awake()
    {
        spriteRenderer = GetComponent();
        taggedObjectTransform = GameObject.FindGameObjectWithTag(taggedObjectStr).transform;

        SetLayerOrders();
        AssignYOffset();
    }

    void SetLayerOrders()
    {
        frontLayerOrder = (int)ToggleLayerType;
        backLayerOrder = frontLayerOrder - 15;
    }

    void AssignYOffset()
    {                                                       // Check to see if the collider offset should be used. If so Assign collider offset.y to 'offsetY'.
        if (takeOffsetFromCollider)                     
        {
            if (gameObject.GetComponent() != null)
            {
                offsetY = gameObject.GetComponent().offset.y;
            }
        }
    }

    void Update()
    {                                                       // Check the distance between this object and the tagged object. If this falls under the 5 to the power of 2 then the toggling process can begin.
        if ((transform.position - taggedObjectTransform.position).sqrMagnitude < 5f * 5f)       
        {
            float posY = transform.position.y + offsetY;   
            if (taggedObjectTransform.position.y > posY)    //Toggle this objects sprite layer sorting order in accordance to the tagged objects y position.
            {
                spriteRenderer.sortingOrder = frontLayerOrder;
            }
            else
            {
                spriteRenderer.sortingOrder = backLayerOrder;
            }
        }
    }
}
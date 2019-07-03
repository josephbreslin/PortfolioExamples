// ================================================================================================ //
// LOOT                                                                                             //
// -------------------------------------------------------------------------------------------------//
// This script is used so the player can interact with Loot.                                        //
// This script adds items to the inventory and updates the dialogue panel with loot information.    //
// This script animates the game object's sprite renderer and plays the loot associated audio.      // 
// This script updates the child game objects's icon. The icon denotes interaction type.            //
// -------------------------------------------------------------------------------------------------//
// Author:  Joseph Breslin (2019)                                                                   //
// ================================================================================================ //

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]              //Attach a circle collider 2D for OnTriggerEnter mechanics
public class Loot : MonoBehaviour
{
    public string chestName = "Chest";              //Name of this chest (To appear in dialogue panel title on opening of chest)
    public string message = "This is a chest";      //Message to player (To appear in dialogue panel body text on opening of chest)
    public Item[] loot;                             //Items to be recieved from chest
    public bool isOpenedPermanently = false;        //Flag to restrict from opening again
    public string interactButton = "Interact";      //Interaction button string
    public bool isDestuctable = true;               //Flag if this is to be destroyed after opening
    public Quest quest;                             //Flag to obtain quest from item
    public bool isQuest;                            //Quest to be obtained from item

    private SpriteRenderer icon;                    //Icon above the loot box
    private string lootSound = "Menu_Click";        //Audio
    private float iconFadeSpeed = 20f;
    private float panelDelayRate = 0.02f;           //Used to deduct time in fade in/ fade out functions
    private bool isVisable = false;                 //Flags that toggles the loot box icon's visability
    private bool isWithinOpenRange = false;         //Flag that denotes whether the player is within range to open the box

    private void Start()
    {
        message = OnOpenMessage();                      //Assign the combonation of the loot item name text and the message text.
        icon = transform.GetChild(0).GetComponent();
    }

    private void Update()
    {
        if (isWithinOpenRange)
        {
            if (Input.GetButtonDown(interactButton))    //Interaction by the player using the Interact button
            {
                OpenLoot();
            }
        }
        isVisable = isWithinOpenRange;
                                                        //Lerping function to animate icon fade
        icon.color = isVisable == true ? Color.Lerp(icon.color, 
                                                    Color.white, 
                                                    Time.fixedDeltaTime * iconFadeSpeed) : Color.Lerp(  icon.color, 
                                                                                                        Color.clear, 
                                                                                                        Time.fixedDeltaTime * iconFadeSpeed);
    }

    private string OnOpenMessage()                      //Operation to assign the correct text to the message variable
    {
        string openMessage = "";
        for (int i = 0; i < loot.Length; i++)
        {
            if (i != loot.Length - 1)
            {
                openMessage = message + " " + loot[i].itemName + " and ";
            }
            else
            {
                openMessage = message + " " + loot[i].itemName + ".";
            }              
        }
        return openMessage;
    }

    //Co-routine that handles the initial dialogue panel fade-in and countdown before fadeout is called.
    private IEnumerator FadeIn(float time, bool disableAfterFade)
    {
        Dialogue_Manager.Instance.FadePanel(1f, 0f);        //Fade in panel immediatly i.e. zero second fade.
        yield return new WaitForSeconds(0);
        time -= panelDelayRate;                             //Countdown
        if (time < 0)
        {
            Dialogue_Manager.Instance.FadePanel(0f, 0f);    //Fade out
        }
        else
        {
            StartCoroutine(FadeIn(time, disableAfterFade)); //Continue fade in / countdown
        }
    }

    private void OpenLoot()
    {
        AudioManager.instance.Audio_ChestOpen();            //Play appropriate sound for the chest opening using Audio Manager
        GetComponent().SetTrigger("Open_Chest");  //Play the chest open Animation

        isOpenedPermanently = true;                         //Set Flags to prevent chest opening again
        isWithinOpenRange = false;

        foreach (Item i in loot)                            //increase amount of loot items in the inventory 
        {
            Inventory._instance.AddItem(i, Inventory._instance.inventoryItems);        
        }

        Dialogue_Manager.Instance.title.text = chestName;   //Update the dialogue panel with the correct information
        Dialogue_Manager.Instance.body.text = message;

        StartCoroutine(FadeIn(1f, isDestuctable));          //Start panel Fade in and countdown
  
        if (isDestuctable)                                  //Disable all renderers could be replaced with Destroy(gameObject,3f);
        {
            Invoke("SwitchOffRenderers", 3f);
        }
    }

    private void SwitchOffRenderers()
    {
        SpriteRenderer spriteRenderer = GetComponent();
        spriteRenderer.enabled = false;
        icon.GetComponent().enabled = false;
        foreach (SpriteRenderer s in transform.GetComponentsInChildren())
        {
            spriteRenderer.enabled = false;
        }
    }

    // Trigger Colliders 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOpenedPermanently)
        {
            isWithinOpenRange = true;
        }    
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOpenedPermanently)
        {
            isWithinOpenRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isWithinOpenRange = false;
        }
    }
}
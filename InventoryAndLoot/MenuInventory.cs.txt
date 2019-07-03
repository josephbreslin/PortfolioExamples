// ================================================================================ //
// MENU INVENTORY                                                                   //
// -------------------------------------------------------------------------------- //
// This script handles how the inventory is displayed on screen through the Menu.   //
// It is used in conjunction the rest of the menu systems including Menu_Toggle.cs  //
// Types.cs is included to obtain the InventoryItem struct.                         //
// -------------------------------------------------------------------------------- //
// Author:  Joseph Breslin (2019)                                                   //
// ================================================================================ //

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Types;

public class MenuInventory : MonoBehaviour {

    enum ItemSortTypes {CONSUME=0,COMBAT=1,KOR=2,WEAPON=3,ARMOUR=4};        // Sorting types used to parse items
    List<InventoryItem> sortedInventoryList = new List<InventoryItem>();    // Sorted list of inventoryItems
    InventoryItem selectedItem;             

    public Sprite returnSprite;             // Use 'returnSprite' for the return button image
    public GameObject imagePanel;            
    public GameObject descriptionPanel;     
    public GameObject promptPanel;          
    public GameObject selectItemPanel;      
    public GameObject iconPanel;            
    public GameObject optionPanel;          
    
   
    public void DisplayItems(int index)     
    {
        sortedInventoryList.Clear();
        EItemType sortByType = EItemType.NONE;
        
        switch ((ItemSortTypes)index)               
        {
            case ItemSortTypes.CONSUME:
                sortByType = EItemType.RECOVERY;
                break;
            case ItemSortTypes.COMBAT:
                sortByType = EItemType.DAMAGE;
                break;
            case ItemSortTypes.KOR:
                sortByType = EItemType.KOR;
                break;
            case ItemSortTypes.WEAPON:
                sortByType = EItemType.WEAPON;
                break;
            case ItemSortTypes.ARMOUR:
                sortByType = EItemType.ARMOR;
                break;
            default:
                sortByType = EItemType.NONE;
                break;
        }

        foreach(InventoryItem inventoryItem in Inventory._instance.inventoryItems)      //Add the sorted items to the 'InventoryItemList'
        {
            if(inventoryItem.itemAmount > 0)
            {
                if(inventoryItem.item.itemType == sortByType)
                {
                    sortedInventoryList.Add(inventoryItem);   
                }
            }
        }

        if (sortedInventoryList.Count > 0)
        {
            AudioManager.instance.Audio_MenuSelect();   //Play select audio for positive player feedback
            
            ToggleDescriptionPanel(false);              //toggle all panels
            ToggleIconPanel(false);                     
            ToggleOptionPanel(false);                   
            ToggleImagePanel(false);                    
            TogglePromptPanel(false);                   
            ToggleSelectItemPanel(true);

            AddReturnButton();                          //Add the return button so the player can escape the displayed inventory
            AddItemButtons();        
        }
        else
        {
            AudioManager.instance.Audio_MenuKorFail();  //Play select audio for negative player feedback
        }
    }

    public void AddItemButtons()
    {
        for (int i = 0; i < sortedInventoryList.Count; i++)
        {
            //Create Button, set parent to the selectitem panel
            GameObject buttonOBJ = Instantiate(Menu.Instance.itemPrefab, selectItemPanel.transform, false) as GameObject;   
            Button button = buttonOBJ.GetComponent<Button>();
            Highlighted_Item_Behaviour HIB = buttonOBJ.GetComponent<Highlighted_Item_Behaviour>();                              //Assign highlighted functions and data
            HIB.buttonMenuType = E_Menu.INVENTORY;
            HIB.decriptionText = sortedInventoryList[i].item.itemName + "\n" + sortedInventoryList[i].item.itemDescription;
            HIB.menuSprite = sortedInventoryList[i].item.menuSprite as Sprite;
            HIB.inventoryItem = sortedInventoryList[i];

            button.onClick.AddListener(
                                        delegate                         //Assign functions for when the button is selected
                                        {
                                            ToggleItemButtonInteractions(false);
                                            TogglePromptPanel(true);
                                            selectedItem = HIB.inventoryItem;
                                            Menu.Instance.SelectGameObject(promptPanel.transform.GetChild(1).gameObject);
                                            AudioManager.instance.Audio_MenuSelect();
                                        });
            
            TMP_Text titleText = buttonOBJ.transform.GetChild(0).GetComponent<TMP_Text>();          //update button TMPText
            TMP_Text amountText = buttonOBJ.transform.GetChild(1).GetComponent<TMP_Text>();         
            titleText.text = sortedInventoryList[i].item.itemName;                                  
            amountText.text = sortedInventoryList[i].itemAmount.ToString();

            if (i == 0)
            {
                Menu.Instance.SelectGameObject(buttonOBJ);                                          //Assign the first selected button
            }
        }
    }

    public void AddReturnButton()
    {
        GameObject backButtonOBJ = Instantiate(Menu.Instance.itemPrefab, selectItemPanel.transform, false) as GameObject;   //Create Button, set parent to the selectitem panel
        Button backButton = backButtonOBJ.GetComponent<Button>();

        Highlighted_Item_Behaviour backHIB = backButtonOBJ.GetComponent<Highlighted_Item_Behaviour>();      //Assign highlighted functions and data
        backHIB.decriptionText = "Return to sort items";
        backHIB.menuSprite = returnSprite as Sprite;

        backButton.onClick.AddListener(
                                        delegate                                    //Assign functions for when the button is selected                                
                                        {
                                            AudioManager.instance.Audio_MenuCancel();
                                            ResetMenuInventory();
                                            Debug.Log("Moving Back");
                                        });

        TMP_Text backTitleText = backButtonOBJ.transform.GetChild(0).GetComponent<TMP_Text>();      //update button TMPText
        TMP_Text backAmountText = backButtonOBJ.transform.GetChild(1).GetComponent<TMP_Text>();
        backTitleText.text = "Back";
        backAmountText.text = "";
    }

    public void ResetMenuInventory()
    {
        foreach(Button b in selectItemPanel.transform.GetComponentsInChildren<Button>())
        {
            Destroy(b.gameObject);
        }
        sortedInventoryList.Clear();
        ToggleDescriptionPanel(false);
        ToggleImagePanel(false);
        TogglePromptPanel(false);
        ToggleSelectItemPanel(false);
        ToggleIconPanel(true);
        ToggleOptionPanel(true);
        Menu.Instance.SetFirstSelectedArrowOnEnable();                          //Return selected button
    } 

    public void UseItem()
    {
        if (selectedItem.item.itemType == EItemType.RECOVERY)
        {
            //Use Item code requires dependency not included in this example

            for (int i = 0; i < Inventory._instance.inventoryItems.Length; i++)
            {
                if (Inventory._instance.inventoryItems[i].item == selectedItem.item)
                {
                    if (Inventory._instance.inventoryItems[i].itemAmount > 0)
                    {
                        Inventory._instance.inventoryItems[i].itemAmount -= 1;      // Deduct Item amount
                    }
                }
            }
            UpdateHighlightedItemBehaviour();
            AudioManager.instance.Audio_MenuKorSuccess();
        }
        else
        {
            AudioManager.instance.Audio_MenuKorFail();
        }
        
        ToggleItemButtonInteractions(true);
        TogglePromptPanel(false);
        Menu.Instance.SelectGameObject(selectItemPanel.transform.GetChild(0).gameObject);
    }

    public void RemoveItem()
    {
        for(int i = 0; i < Inventory._instance.inventoryItems.Length; i++)
        {
            if(Inventory._instance.inventoryItems[i].item == selectedItem.item)
            {
                if(Inventory._instance.inventoryItems[i].itemAmount > 0)
                {
                    Inventory._instance.inventoryItems[i].itemAmount -= 1;          // Deduct Item amount
                }
            }
        }
        UpdateHighlightedItemBehaviour();

        AudioManager.instance.Audio_MenuKorSuccess();

        ToggleItemButtonInteractions(true);
        TogglePromptPanel(false);

        Menu.Instance.SelectGameObject(selectItemPanel.transform.GetChild(0).gameObject);        
    }

    public void UpdateHighlightedItemBehaviour()
    {
        foreach (Highlighted_Item_Behaviour hig in selectItemPanel.GetComponentsInChildren<Highlighted_Item_Behaviour>())
        {
            if (hig.inventoryItem.item == selectedItem.item)
            {
                if (selectedItem.itemAmount > 1)
                {
                    selectedItem.itemAmount -= 1;
                    hig.transform.GetChild(1).GetComponent<TMP_Text>().text = selectedItem.itemAmount.ToString();
                }
                else
                {
                    Destroy(hig.gameObject);                                        //Remove Item button
                }
            }
        }
    }

    public void CancelPrompt()                                                              // Cancel the prompt panel
    {
        AudioManager.instance.Audio_MenuCancel();

        ToggleItemButtonInteractions(true);
        TogglePromptPanel(false);
        Menu.Instance.SelectGameObject(selectItemPanel.transform.GetChild(0).gameObject);
    }

    void ToggleItemButtonInteractions(bool isInteractable)                                  // This toggles the item button's interaction states
    {
        Button[] buttons = selectItemPanel.transform.GetComponentsInChildren<Button>();     
        foreach (Button b in buttons)
        {
            b.interactable = isInteractable;
        }
    }

    private void OnDisable()
    {
        sortedInventoryList.Clear();                                    // When this component is disabled the 'InventoryItemList' is reset by removing all data
        ResetMenuInventory();                                           // Logic for resetting all panels
    }

    private void OnEnable()
    {
        Menu.Instance.SetFirstSelectedArrowOnEnable();
         TogglePromptPanel(false);
    }

    public void ToggleImagePanel(bool isOn)                                                     // Toggles the image panel on and off.
    {
        imagePanel.SetActive(isOn);
    }

    public void ToggleDescriptionPanel(bool isOn)                                               // Toggles the description panel on and off.
    {
        descriptionPanel.SetActive(isOn);
    }

    public void TogglePromptPanel(bool isOn)                                                    // Toggles the prompt panel on and off.
    {
        promptPanel.SetActive(isOn);
    }

    public void ToggleSelectItemPanel(bool isOn)                                                // Toggles the select item panel on and off.
    {
        selectItemPanel.SetActive(isOn);
    }

    public void ToggleIconPanel(bool isOn)                                                      // Toggles the icon panel on and off.
    {
        iconPanel.SetActive(isOn);
    }

    public void ToggleOptionPanel(bool isOn)                                                    // Toggles the option panel on and off.
    {
        optionPanel.SetActive(isOn);
    }

    public void DisplayItemSprite(Sprite itemSprite)                                            // Updates the item image panel's child sprite with the 'itemSprite' and toggles the panel's visability.
    {
        ToggleImagePanel(true);
        imagePanel.transform.GetChild(0).GetComponent<Image>().sprite = itemSprite as Sprite;
    }

    public void DisplayItemDescription(string description)                                      // Updates the description panel text with the 'description' string and toggles the panel's visability.
    {
        ToggleDescriptionPanel(true);
        descriptionPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = description;
    }
}

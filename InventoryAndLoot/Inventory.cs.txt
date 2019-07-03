// ======================================================================================================== //
// INVENTORY                                                                                                //
// -------------------------------------------------------------------------------------------------------- //
// This script contains the data structure that holds the inventory information for each item.              //
// Items must be added to the inventoryItems array from the inspector.                                      //
// The itemAmount value of these items must be set to zero on first playthrough.                            // 
// The amount of each item can be saved using the Save Inventory function. There is also a load function.   //
// ---------------------------------------------------------------------------------------------------------//
// Author:  Joseph Breslin (2019)                                                                           //
// ======================================================================================================== //

using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class Inventory : MonoBehaviour {

    [Serializable]                                  //serialized so to populate values in inspector
    public struct InventoryItem
    {
        public Item item;                           //Item is a scriptable object containing specific item data
        public uint itemAmount;                     //The amount of each item is stored here
    }
    public InventoryItem[] inventoryItems;          //This Array must contain everyiItem in the Game

    public static Inventory _instance { get; private set; }

    private void Awake()                            //Singleton pattern for object persistence
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(Item item, InventoryItem[] _inventoryItems)     //This function adds an item to the inventory by increasing the item amount
    {
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            if (_inventoryItems[i].item == item)
            {
                _inventoryItems[i].itemAmount++;
            }
        }
    }

    public void SaveInventory()                                        //Used in conjunction with Game Manager 
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/inventoryData.dat");
        uint[] itemAmounts = new uint[inventoryItems.Length];
        for (int i = 0; i < itemAmounts.Length; i++)
        {
            itemAmounts[i] = inventoryItems[i].itemAmount;
        }
        bf.Serialize(file, itemAmounts);
        file.Close();
    }

    public void LoadInventory()                                          //Used in conjunction with Game Manager 
    {
        if (File.Exists(Application.persistentDataPath + "/inventoryData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/inventoryData.dat", FileMode.Open);
            uint[] itemAmounts = (uint[])bf.Deserialize(file);

            if (itemAmounts.Length == inventoryItems.Length)
            {
                for (int i = 0; i < inventoryItems.Length; i++)
                {
                    inventoryItems[i].itemAmount = itemAmounts[i];
                }
            }
            file.Close();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int          id;
    public string       playerName;
    public List<Item>   inventory;
    public Room         currentRoom;

    public bool HasItem(string p_itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
            if (inventory[i].itemName.ToLower() == p_itemName)
                return true;
        return false;
    }
    public Item GetItem(string p_itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
            if (inventory[i].itemName.ToLower() == p_itemName)
                return inventory[i];
        return null;
    }
}

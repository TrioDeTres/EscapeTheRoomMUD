using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlayerData
{
    public int          id;
    public string       playerName;
    public List<Item>   inventory;
    public Room         currentRoom;
    public bool         ready;

    public PlayerData()
    {
        this.ready = false;
        this.inventory = new List<Item>();
    }

    public bool HasItem(string itemName)
    {
        return inventory.Any(t => t.itemName.ToLower() == itemName);
    }

    public Item GetItem(string itemName)
    {
        return inventory.FirstOrDefault(t => t.itemName.ToLower() == itemName);
    }
}

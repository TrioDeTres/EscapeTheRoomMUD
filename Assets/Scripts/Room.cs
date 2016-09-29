using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardinalPoint
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}

public class Room : MonoBehaviour
{
    public List<RoomTransition> adjacentRooms;
    public List<Item>           items;
    public List<PlayerData>     playersInRoom;

    public int                  roomID;
    public string               roomName;
    public string               roomFullName;

    [TextArea(10, 20)] public string    roomDescription;

    public Room()
    {
        adjacentRooms = new List<RoomTransition>();
        playersInRoom = new List<PlayerData>();
    }

    public bool HasItem(string p_itemName)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName.ToLower() == p_itemName)
                return true;
        return false;
    }
    public Item GetItem(string p_itemName)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName.ToLower() == p_itemName)
                return items[i];
        return null;
    }

    public List<Item> GetPickableItems()
    {
        List<Item> __items = new List<Item>();
        for (int i = 0; i < items.Count; i++)
            if (items[i].isPickable)
                __items.Add(items[i]);
        return __items;
    }
}



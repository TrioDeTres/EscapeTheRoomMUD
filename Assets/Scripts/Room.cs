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
    [TextArea] public string    roomDescription;

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
}



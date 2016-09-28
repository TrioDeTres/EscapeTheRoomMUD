using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public List<Room> rooms;

    public bool CheckDirectionNotExists(PlayerData p_player, CardinalPoint p_direction)
    {
        return p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom == null;
    }

    public bool CheckIfRoomIsLocked(PlayerData p_player, CardinalPoint p_direction)
    {
        return p_player.currentRoom.adjacentRooms[(int)p_direction].isLocked;
    }

    public string GetMessageWhenLocked(PlayerData p_player, CardinalPoint p_direction)
    {
        return p_player.currentRoom.adjacentRooms[(int)p_direction].messageWhenLocked;
    }

    public void MoveToRoom(PlayerData p_player, CardinalPoint p_direction, bool p_localPlayer)
    {
        Room __oldRoom = p_player.currentRoom;
        __oldRoom.playersInRoom.Remove(p_player);
        p_player.currentRoom = p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom;
        p_player.currentRoom.playersInRoom.Add(p_player);

        if (p_localPlayer) { 
            UIManager.CreateMessage("You moved " + p_direction.ToString().ToLower() + ". You are now at the " + p_player.currentRoom.roomFullName + ".", MessageColor.LIGHT_BLUE);
        }
    }

    public void TryToLookRoom(PlayerData p_player)
    {
        string __desc = "Room Description: " + Environment.NewLine + p_player.currentRoom.roomDescription;
        List<Item> __items = p_player.currentRoom.GetPickableItems();
        if (__items.Count > 0)
        {
            __desc += " Other items in the room are:";

            for (int i = 0; i < __items.Count; i++)
                __desc += " " +__items[i].GetFullName() + ",";

            __desc = __desc.Remove(__desc.Length -1);
            __desc += ".";
        }
        UIManager.CreateMessage(__desc, MessageColor.LIGHT_BLUE);
    }
}

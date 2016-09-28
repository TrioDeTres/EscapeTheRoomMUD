using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public List<Room> rooms;
    
    public void TryToMoveToRoom(PlayerData p_player, CardinalPoint p_direction)
    {
        //No room in direction
        if (p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom == null)
            UIManager.CreateMessage("There is nothing on this direction", MessageColor.RED);
        //Locked room
        else if (p_player.currentRoom.adjacentRooms[(int)p_direction].isLocked)
            UIManager.CreateMessage(p_player.currentRoom.adjacentRooms[(int)p_direction].messageWhenLocked, 
                MessageColor.WHITE);
        //Moved
        else
        {
            Room __oldRoom = p_player.currentRoom;
            __oldRoom.playersInRoom.Remove(p_player);
            p_player.currentRoom = p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom;
            p_player.currentRoom.playersInRoom.Add(p_player);
            UIManager.CreateMessage("You moved " + p_direction.ToString().ToLower() +
               ". You are now at the " + p_player.currentRoom.roomFullName + ".", MessageColor.LIGHT_BLUE);
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

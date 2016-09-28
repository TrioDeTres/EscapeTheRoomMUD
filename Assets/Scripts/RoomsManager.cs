using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public List<Room> rooms;

    public bool TryToMoveToRoomInDirection(PlayerData p_player, CardinalPoint p_direction)
    {
        return p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom != null;
    }

    public bool TryToMoveToLockedRoom(PlayerData p_player, CardinalPoint p_direction)
    {
        return !p_player.currentRoom.adjacentRooms[(int)p_direction].isLocked;
    }

    public void MoveToRoom(PlayerData p_player, CardinalPoint p_direction)
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
        UIManager.CreateMessage("Room Description: " + Environment.NewLine +
            p_player.currentRoom.roomDescription, MessageColor.LIGHT_BLUE);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public event Action<string, MessageColor> OnSendMessage;
    public List<Room> rooms;
    
    public void TryToMoveToRoom(PlayerData p_player, CardinalPoint p_direction)
    {
        //Debug.Log(p_player.currentRoom.roomID.ToString() +  p_direction);
        if (p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom == null)
            UIManager.CreateDefautMessage(DefaultMessageType.MOVE_CMD_NO_ROOM_IN_DIRECTION);
        else if (p_player.currentRoom.adjacentRooms[(int)p_direction].isLocked)
        {
            UIManager.CreateDefautMessage(DefaultMessageType.MOVE_CMD_LOCKED_ROOM,
                new List<string> { p_player.currentRoom.adjacentRooms[(int)p_direction].messageWhenLocked });
        }
        else
        {
            Room __oldRoom = p_player.currentRoom;
            __oldRoom.playersInRoom.Remove(p_player);
            p_player.currentRoom = p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom;
            p_player.currentRoom.playersInRoom.Add(p_player);
            OnSendMessage("Changing player to room " + p_player.currentRoom.roomID.ToString(), 
                MessageColor.WHITE);
        }
    }

    public void TryToLookRoom(PlayerData p_player)
    {
        OnSendMessage(p_player.currentRoom.roomDescription, MessageColor.WHITE);
    }
}

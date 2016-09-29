﻿using System;
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

    public string GetMessageWhenDirectionNotExists()
    {
        return "Server says: There is nothing on this direction";
    }

    public string GetMessageWhenLocked(PlayerData p_player, CardinalPoint p_direction)
    {
        return "Server says: " + p_player.currentRoom.adjacentRooms[(int)p_direction].messageWhenLocked;
    }

    public string GetMessageWhenMovedSuccessfully(string playerName)
    {
        return "Server says: " + playerName + " has left room.";
    }

    public void MoveToRoom(PlayerData p_player, CardinalPoint p_direction, bool p_localPlayer)
    {
        Room __oldRoom = p_player.currentRoom;
        __oldRoom.playersInRoom.Remove(p_player);
        p_player.currentRoom = p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom;
        p_player.currentRoom.playersInRoom.Add(p_player);

        if (p_localPlayer) { 
            UIManager.CreateMessage("Server says: You moved " + p_direction.ToString().ToLower() + ". You are now at the " + p_player.currentRoom.roomFullName + ".", MessageColor.LIGHT_BLUE);
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

    public Room FindRoomById(int p_roomId)
    {
        return rooms.Find(r => r.roomID == p_roomId);
    }

    public Room PickRoomForPlayer(int playerId)
    {
        if (playerId == 1 || playerId == 3)
        {
            return FindRoomById(0); // RoomA0_DinningRoom
        }
        else
        {
            return FindRoomById(4); // RoomB0_Playroom
        }
    }

    public List<PlayerData> FindPlayersInRoom(int roomId)
    {
        return FindRoomById(roomId).playersInRoom;
    }

    public void ClearState()
    {
        rooms.Clear();
    }
}

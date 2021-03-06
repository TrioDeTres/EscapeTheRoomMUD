﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public List<Room> rooms;

    public bool CheckDirectionNotExists(PlayerData p_player, CardinalPoint p_direction)
    {
        return FindRoomInDirection(p_player, p_direction) == null;
    }

    public bool CheckIfRoomIsLocked(PlayerData p_player, CardinalPoint p_direction)
    {
        return p_player.currentRoom.adjacentRooms[(int)p_direction].isLocked;
    }

    public string GetMessageWhenDirectionNotExists()
    {
        return "Server says: There is nothing on this direction";
    }
    public string GetMessageWhenRoomDontHaveItem()
    {
        return "Server says: There is no item with this name in the room.";
    }
    public string GetMessageWhenRoomDontHaveTargetItem()
    {
        return "Server says: The target item is not on the room.";
    }
    public string GetMessageWhenLocked(PlayerData p_player, CardinalPoint p_direction)
    {
        return "Server says: " + p_player.currentRoom.adjacentRooms[(int)p_direction].messageWhenLocked;
    }

    public string GetMessageWhenMovedOutSuccessfully(string playerName)
    {
        return "Server says: " + playerName + " has left room.";
    }

    public string GetMessageWhenMovedInSuccessfully(string playerName)
    {
        return "Server says: " + playerName + " has joined in the room.";
    }

    public void MoveToRoom(PlayerData p_player, CardinalPoint p_direction, bool p_localPlayer)
    {
        Room __oldRoom = p_player.currentRoom;
        __oldRoom.playersInRoom.Remove(p_player);
        p_player.currentRoom = p_player.currentRoom.adjacentRooms[(int)p_direction].targetRoom;
        p_player.currentRoom.playersInRoom.Add(p_player);

        if (p_localPlayer)
        {
            UIManager.CreateMessage("Server says: You moved " + p_direction.ToString().ToLower() + ". You are now at the " + 
                p_player.currentRoom.roomFullName + "." + GetPlayersInRoomMessage(p_player.currentRoom, p_player.id), MessageColor.LIGHT_BLUE);
            if (p_player.currentRoom.roomID == 8)
                UIManager.CreateMessage("Congratulations! You reached the hall, you won the game.");
        }
    }
    public string GetPlayersInRoomMessage(Room p_room, int p_id)
    {
        bool __onlyEmpty = true;
        string __msg = " You can see the players: ";
        for (int i = 0; i < p_room.playersInRoom.Count; i++)
        {
            if (p_room.playersInRoom[i].playerName != " " &&
                !string.IsNullOrEmpty(p_room.playersInRoom[i].playerName) &&
                p_id != p_room.playersInRoom[i].id)
                __onlyEmpty = false;
        }
        if (__onlyEmpty)
            return string.Empty;
        for (int i = 0; i < p_room.playersInRoom.Count; i++)
        {
            if (p_room.playersInRoom[i].playerName != " " &&
                !string.IsNullOrEmpty(p_room.playersInRoom[i].playerName) &&
                p_id != p_room.playersInRoom[i].id)
                __msg += p_room.playersInRoom[i].playerName + ", ";
        }
        __msg = __msg.Remove(__msg.Length - 2, 2);
        __msg += ".";
        return __msg;
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

    public Room FindRoomInDirection(PlayerData playerData, CardinalPoint cardinalPoint)
    {
        return playerData.currentRoom.adjacentRooms[(int) cardinalPoint].targetRoom;
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

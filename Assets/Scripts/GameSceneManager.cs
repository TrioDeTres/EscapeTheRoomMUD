﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UIManager uiManager;
    public CommandManager commandManager;
    public RoomsManager roomsManager;
    public ItemsManager itemsManager;
    public PlayersManager playersManager;
    public NetworkManager networkManager;

    void Start()
    {
        commandManager.OnTryToMoveToRoom += TryToMoveToRoom;
        commandManager.OnTryToShowInventory += TryToShowInventory;
        commandManager.OnTryToGetItem += TryToGetItem;
        commandManager.OnTryToDropItem += TryToDropItem;
        commandManager.OnTryToLookRoom += TryToLookRoom;
        commandManager.OnTryToLookItem += TryToLookItem;
        commandManager.OnTryToUseItem += TryToUseItem;
        commandManager.OnTryToUseHole += TryToUseHole;
        commandManager.OnTryToUseSuitcase += TryToUseSuitcase;
        commandManager.OnTryToConnectOnServer += OnTryToConnectOnServer;
        commandManager.OnTryToStartServer += OnTryToSetupServer;
        commandManager.OnTryToStopServer += OnTryToStopServer;
        commandManager.OnTryToSay += TryToSay;
        commandManager.OnTryToSayHole += TryToSayHole;
        commandManager.OnTryToWhisper += TryToWhisper;
        commandManager.OnSendMessageToPlayers += OnSendMessageToPlayers;
        commandManager.OnSendPlayerNameToServer += OnSendPlayerNameToServer;
        commandManager.OnSendPlayerReadyToServer += OnSendPlayerReadyToServer;

        networkManager.OnSuitcaseOpened += OnSuitcaseOpened;
        networkManager.OnPlayInteraction += OnPlayInteraction;
        networkManager.OnUseHole += OnUseHole;
        uiManager.OnExecuteMessage += commandManager.ParseMessage;
    }

    private void OnUseHole(int p_id, string p_sourceName)
    {
        Item __source = itemsManager.GetItem(p_sourceName);
        PlayerData __data = playersManager.FindPlayerById(p_id);
        __data.inventory.Remove(__source);
        if (__data.currentRoom.roomID == 0)
            roomsManager.rooms[4].items.Add(__source);
        else
            roomsManager.rooms[0].items.Add(__source);
    }

    private void TryToUseHole(string p_source)
    {
        networkManager.AskServerToUseHole(playersManager.activePlayer, p_source);
    }

    private void OnPlayInteraction(int p_id, string p_sourceName, string p_targetName)
    {
        Item __source = itemsManager.GetItem(p_sourceName);
        Item __target = playersManager.FindPlayerById(p_id).currentRoom.GetItem(p_targetName);

        //remove source from user
        for (int i = 0; i < playersManager.players.Count; i++)
            if (playersManager.players[i].HasItem(p_sourceName))
                playersManager.players[i].inventory.Remove(__source);
        PlayInteraction(__target.GetInteraction(__source), p_id);
    }

    private void OnSuitcaseOpened(int p_playerID)
    {
        if (!itemsManager.GetItem("suitcase").interactions[0].active && p_playerID == playersManager.activePlayer.id)
            UIManager.CreateMessage("The suitcase is already open.", MessageColor.RED);
        else

            PlayInteraction(itemsManager.GetItem("suitcase").interactions[0], p_playerID);
    }

    private void OnTryToConnectOnServer(string address, int port)
    {
        networkManager.ConnectToServer(address, port);
    }

    private void OnTryToSetupServer(int port)
    {
        networkManager.StartServer(port);
        networkManager.ConnectToServer(IPAddress.Loopback.ToString(), port);
    }

    private void OnTryToStopServer()
    {
        networkManager.StopServer();
    }

    private void OnSendMessageToPlayers(int activePlayerId, string message)
    {
        networkManager.SendMessageToPlayers(activePlayerId, message);
    }

    private void OnSendPlayerNameToServer(string name)
    {
        networkManager.SendPlayerNameToServer(name);
    }

    private void OnSendPlayerReadyToServer()
    {
        networkManager.SendPlayerReadyToServer(playersManager.activePlayer);
    }

    private void TryToUseSuitcase(string p_param)
    {
        Debug.Log(p_param);
        networkManager.AskServerToUseSuitcase(playersManager.activePlayer, p_param);
    }

    private void TryToUseItem(string p_source, string p_target)
    {
        networkManager.AskServerToUseItem(playersManager.activePlayer, p_source, p_target);
    }

    private void PlayInteraction(ItemInteraction p_interaction, int p_playerId)
    {
        if (!string.IsNullOrEmpty(p_interaction.messageWhenActivated))
        {
            if (p_playerId == playersManager.activePlayer.id)
                UIManager.CreateMessage(p_interaction.messageWhenActivated, MessageColor.BLUE);
            //else
            //   UIManager.CreateMessage(p_interaction.messageWhenActivated, MessageColor.BLUE);
        }

        foreach (InteractionChangeItemDescription __interaction in p_interaction.changeItemDescriptions)
            __interaction.target.itemDescription = __interaction.newDescription;
        foreach (InteractionMoveItem __interaction in p_interaction.moveItems)
        {
            if (__interaction.oldRoom != null)
                __interaction.oldRoom.items.Remove(__interaction.target);
            if (__interaction.newRoom != null)
                __interaction.newRoom.items.Add(__interaction.target);
        }
        foreach (InteractionChangeRoomDescription __interaction in p_interaction.changeRoomDescriptions)
            __interaction.target.roomDescription = __interaction.newDescription;
        foreach (InteractionChangeRoomTransitionLocked __interaction in p_interaction.changeRoomTransition)
            __interaction.target.adjacentRooms[(int)__interaction.direction].isLocked = __interaction.isLocked;

        p_interaction.active = false;
    }
    private void TryToSay(string p_message)
    {
        networkManager.AskServerToSay(playersManager.activePlayer, p_message);
    }
    private void TryToSayHole(string p_message)
    {
        networkManager.AskServerToSayHole(playersManager.activePlayer, p_message);
    }
    private void TryToWhisper(string p_target, string p_message)
    {
        networkManager.AskServerToWhisper(playersManager.activePlayer, p_target, p_message);
    }
    private void TryToDropItem(string p_itemName)
    {
        networkManager.AskServerToDropItem(playersManager.activePlayer, p_itemName);
    }

    private void TryToGetItem(string p_itemName)
    {
        networkManager.AskServerToGetItem(playersManager.activePlayer, p_itemName);
    }

    private void TryToShowInventory(string p_target)
    {
        playersManager.TryToDisplayInventory(playersManager.activePlayer, p_target);
    }

    private void TryToMoveToRoom(CardinalPoint p_direction)
    {
        networkManager.AskServerMoveToRoom(playersManager.activePlayer, p_direction);
    }

    private void TryToLookRoom()
    {
        roomsManager.TryToLookRoom(playersManager.activePlayer);
    }

    private void TryToLookItem(string p_itemName)
    {
        //Item don't exist
        if (!itemsManager.HasItem(p_itemName))
            UIManager.CreateMessage("Item not found.", MessageColor.RED);
        //Player have the item
        else if (playersManager.activePlayer.HasItem(p_itemName))
        {
            Item __item = playersManager.activePlayer.GetItem(p_itemName);
            UIManager.CreateMessage(__item.GetFullName() + ":" + 
                Environment.NewLine + __item.itemDescription, MessageColor.LIGHT_BLUE);
        }
        //Room have the item
        else if (playersManager.activePlayer.currentRoom.HasItem(p_itemName))
        {
            Item __item = playersManager.activePlayer.currentRoom.GetItem(p_itemName);
            UIManager.CreateMessage(__item.GetFullName() + ":" +
                Environment.NewLine + __item.itemDescription, MessageColor.LIGHT_BLUE);
        }
        //Not found
        else
            UIManager.CreateMessage("Item not found.", MessageColor.RED);
    }
}

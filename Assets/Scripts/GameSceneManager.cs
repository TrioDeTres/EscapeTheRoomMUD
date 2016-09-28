using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UIManager uiManager;
    public CommandManager commandManager;
    public RoomsManager roomsManager;
    public ItemsManager itemsManager;
    public PlayersManager playersManager;
    public PlayerData activePlayer;
    public NetworkManager networkManager;

    void Start()
    {
        networkManager.OnMoveToRoom += MoveToRoom;

        commandManager.OnTryToMoveToRoom += TryToMoveToRoom;
        commandManager.OnTryToShowInventory += TryToShowInventory;
        commandManager.OnTryToGetItem += TryToGetItem;
        commandManager.OnTryToDropItem += TryToDropItem;
        commandManager.OnTryToLookRoom += TryToLookRoom;
        commandManager.OnTryToLookItem += TryToLookItem;
        commandManager.OnTryToUseItem += TryToUseItem;
        commandManager.OnTryToUseSuitcase += TryToUseSuitcase;
        commandManager.OnTryToConnectOnServer += OnTryToConnectOnServer;
        commandManager.OnTryToStartServer += OnTryToSetupServer;
        commandManager.OnTryToStopServer += OnTryToStopServer;

        uiManager.OnExecuteMessage += commandManager.ParseMessage;
        activePlayer.currentRoom = roomsManager.rooms[2];
    }

    private void OnTryToConnectOnServer(string p_address, int p_port, string p_playerName)
    {
        networkManager.ConnectOnServer(p_address, p_port);
    }

    private void OnTryToSetupServer(int p_port)
    {
        networkManager.StartServer(p_port);
    }

    private void OnTryToStopServer()
    {
        networkManager.StopServer();
    }

    private void TryToUseSuitcase(string p_param)
    {
        int __result = 0;
        //Player is not on the Office
        if (activePlayer.currentRoom != roomsManager.rooms[1])
            UIManager.CreateMessage("Item not found.", MessageColor.RED);
        //Valid password
        if (int.TryParse(p_param, out __result))
        {
            //Right password
            if (__result == 123456)
                PlayInteraction(itemsManager.GetItem("suitcase").interactions[0]);
            //Wrong password
            else
                UIManager.CreateMessage("This is the wrong password.", MessageColor.RED);
        }
        //Invalid password
        else
            UIManager.CreateMessage("This is an invalid password.", MessageColor.RED);
    }

    private void TryToUseItem(string p_source, string p_target)
    {
        //Player don't have the item
        if (!activePlayer.HasItem(p_source))
            UIManager.CreateMessage("You don't have this item on your inventory.", MessageColor.RED);
        //There is no target item on the room
        else if (!activePlayer.currentRoom.HasItem(p_target))
            UIManager.CreateMessage("The target item is not on the room.", MessageColor.RED);
        //The items exist
        else
        {
            Item __source = activePlayer.GetItem(p_source);
            Item __target = activePlayer.currentRoom.GetItem(p_target);

            //Source can't be used
            if (!__source.isUsable)
                UIManager.CreateMessage("The target item cannot be used.", MessageColor.RED);
            //No interaction between items
            else if (!__target.HasInteractionWithItem(__source))
                UIManager.CreateMessage("These items can't be use together.", MessageColor.RED);
            //Interaction not active
            else if (!__target.GetInteraction(__source).active)
                UIManager.CreateMessage("Nothing happened.", MessageColor.RED);
            else
                PlayInteraction(__target.GetInteraction(__source));
        }
    }

    private void PlayInteraction(ItemInteraction p_interaction)
    {
        if (!string.IsNullOrEmpty(p_interaction.messageWhenActivated))
            UIManager.CreateMessage(p_interaction.messageWhenActivated, MessageColor.BLUE);

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

    private void TryToDropItem(string p_itemName)
    {
        if (activePlayer.HasItem(p_itemName.ToLower()))
        {
            Item __item = activePlayer.GetItem(p_itemName.ToLower());
            activePlayer.currentRoom.items.Add(__item);
            activePlayer.inventory.Remove(__item);
            UIManager.CreateDefautMessage(DefaultMessageType.DROP_ITEM,
                    new List<string> { __item.itemName });
        }
        else
            UIManager.CreateMessage("You don't have this item on your inventory.", MessageColor.RED);
    }

    private void TryToGetItem(string p_itemName)
    {
        if (activePlayer.currentRoom.HasItem(p_itemName.ToLower()))
        {
            Item __item = activePlayer.currentRoom.GetItem(p_itemName.ToLower());
            if (__item.isPickable)
            {
                activePlayer.inventory.Add(__item);
                activePlayer.currentRoom.items.Remove(__item);
                UIManager.CreateDefautMessage(DefaultMessageType.GET_ITEM,
                    new List<string> { __item.GetFullName() });
            }
            else
                UIManager.CreateMessage("This item can't be picked up.", MessageColor.RED);
        }
        else
            UIManager.CreateMessage("There is no item with this name in the room.", MessageColor.RED);
    }

    private void TryToShowInventory(string p_target)
    {
        playersManager.TryToDisplayInventory(activePlayer, p_target);
    }

    private void TryToMoveToRoom(CardinalPoint p_direction)
    {
        networkManager.AskServerMoveToRoom(activePlayer, p_direction);
    }

    private void MoveToRoom(string[] p_inputs)
    {
        UIManager.CreateMessage("Received inputs > " + string.Join(", ", p_inputs), MessageColor.LIGHT_BLUE);
    }

    private void TryToLookRoom()
    {
        roomsManager.TryToLookRoom(activePlayer);
    }

    private void TryToLookItem(string p_itemName)
    {
        //Item don't exist
        if (!itemsManager.HasItem(p_itemName))
            UIManager.CreateMessage("Item not found.", MessageColor.RED);
        //Player have the item
        else if (activePlayer.HasItem(p_itemName))
        {
            Item __item = activePlayer.GetItem(p_itemName);
            UIManager.CreateMessage(__item.GetFullName() + ":" + 
                Environment.NewLine + __item.itemDescription, MessageColor.LIGHT_BLUE);
        }
        //Room have the item
        else if (activePlayer.currentRoom.HasItem(p_itemName))
        {
            Item __item = activePlayer.currentRoom.GetItem(p_itemName);
            UIManager.CreateMessage(__item.GetFullName() + ":" +
                Environment.NewLine + __item.itemDescription, MessageColor.LIGHT_BLUE);
        }
        //Not found
        else
            UIManager.CreateMessage("Item not found.", MessageColor.RED);
    }
}

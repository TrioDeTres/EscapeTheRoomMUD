using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public UIManager        uiManager;
    public CommandManager   commandManager;
    public RoomsManager     roomsManager;
    public ItemsManager     itemsManager;
    public PlayersManager   playersManager;
    public PlayerData       activePlayer;

	void Start ()
    {
        commandManager.OnTryToMoveToRoom += TryToMoveToRoom;
        commandManager.OnTryToShowInventory += TryToShowInventory;
        commandManager.OnTryToGetItem += TryToGetItem;
        commandManager.OnTryToDropItem += TryToDropItem;
        commandManager.OnTryToLookRoom += TryToLookRoom;
	    commandManager.OnTryToLookItem += TryToLookItem;
        commandManager.OnMessageError += UIManager.CreateDefautMessage;
        uiManager.OnExecuteMessage += commandManager.ParseMessage;
        roomsManager.OnSendMessage += UIManager.CreateMessage;
        itemsManager.OnSendMessage += UIManager.CreateMessage;
        itemsManager.OnMessageError += UIManager.CreateDefautMessage;
        activePlayer.currentRoom = roomsManager.rooms[1];
	}

    private void TryToDropItem(string p_itemName)
    {
        if (activePlayer.HasItem(p_itemName.ToLower()))
        {
            Item __item = activePlayer.GetItem(p_itemName.ToLower());
            activePlayer.currentRoom.items.Add(__item);
            activePlayer.inventory.Remove(__item);
            UIManager.CreateDefautMessage(DefaultMessageType.DROP_CMD_DROP_ITEM,
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
                UIManager.CreateDefautMessage(DefaultMessageType.GET_CMD_GET_ITEM,
                    new List<string> { __item.itemName });
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
        roomsManager.TryToMoveToRoom(activePlayer, p_direction);
    }

    private void TryToLookRoom()
    {
        roomsManager.TryToLookRoom(activePlayer);
    }

    private void TryToLookItem(string p_itemName)
    {
        itemsManager.TryToLookItem(p_itemName);
    }
}

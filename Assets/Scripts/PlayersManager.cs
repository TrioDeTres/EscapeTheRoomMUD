using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    public List<PlayerData> players;

    public void TryToDisplayInventory(PlayerData p_activePlayer, string p_name)
    {
        if (string.IsNullOrEmpty(p_name))
            DisplayInventoryOfPlayer(p_activePlayer);
        else if (HasPlayerWithName(p_name))
        {
            PlayerData __data = GetPlayerByName(p_name);
            if (__data.currentRoom == p_activePlayer.currentRoom)
                DisplayInventoryOfPlayer(__data);
            else
                UIManager.CreateDefautMessage(DefaultMessageType.INVENTORY_CMD_NO_PLAYER_IN_ROOM,
                    new List<string>() { p_name });
        }
        else
            UIManager.CreateDefautMessage(DefaultMessageType.INVENTORY_CMD_NO_PLAYER_IN_GAME,
                    new List<string>() { p_name });

    }
    private void DisplayInventoryOfPlayer(PlayerData p_activePlayer)
    {
        if (p_activePlayer.inventory.Count == 0)
            UIManager.CreateDefautMessage(DefaultMessageType.INVENTORY_CMD_EMPTY_INVENTORY);
        else
        {
            string __inventoryText = "";
            for (int i = 0; i < p_activePlayer.inventory.Count; i++)
            {
                __inventoryText += p_activePlayer.inventory[i].itemName;
                if (i < p_activePlayer.inventory.Count - 1)
                    __inventoryText += ", ";
            }
            UIManager.CreateDefautMessage(DefaultMessageType.INVENTORY_CMD_SHOW_INVENTORY, 
                new List<string> { __inventoryText });
        }
    }
    public PlayerData GetPlayerByName(string p_name)
    {
        for (int i = 0; i < players.Count; i++)
            if (players[i].playerName == p_name)
                return players[i];
        return null;
    }

    public bool HasPlayerWithName(string p_name)
    {
        for (int i = 0; i < players.Count; i++)
            if (players[i].playerName == p_name)
                return true;
        return false;
    }
}

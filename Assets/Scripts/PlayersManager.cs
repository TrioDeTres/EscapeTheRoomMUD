using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    public List<PlayerData> players = new List<PlayerData>();

    public void CreatePlayer(int p_id, string p_name)
    {
        PlayerData player = new PlayerData();

        player.playerName = p_name;
        player.id = p_id;

        players.Add(player);
    }

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
                UIManager.CreateMessage("There is no player in the room with the name " + p_name);
        }
        else
            UIManager.CreateMessage("There is no player in the game with the name " + p_name);
    }
    private void DisplayInventoryOfPlayer(PlayerData p_activePlayer)
    {
        if (p_activePlayer.inventory.Count == 0)
            UIManager.CreateMessage("The inventory is empty.", MessageColor.RED);
        else
        {
            string __inventoryText = "";
            for (int i = 0; i < p_activePlayer.inventory.Count; i++)
            {
                __inventoryText += p_activePlayer.inventory[i].GetFullName();
                if (i < p_activePlayer.inventory.Count - 1)
                    __inventoryText += ", ";
            }
            UIManager.CreateDefautMessage(DefaultMessageType.SHOW_INVENTORY, 
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

    public PlayerData FindPlayerById(int p_playerId)
    {
        return players.Find(p => p.id == p_playerId);
    }
}

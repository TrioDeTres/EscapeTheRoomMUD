using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayersManager : MonoBehaviour
{
    public PlayerData activePlayer;
    public List<PlayerData> players;

    public bool IsActivePlayerSet { get; set; }

    public PlayersManager()
    {
        players = new List<PlayerData>();
        IsActivePlayerSet = false;
    }

    public PlayerData CreatePlayer(int id, string name, Room currentRoom)
    {
        PlayerData player = new PlayerData
        {
            playerName = name,
            id = id,
            currentRoom = currentRoom,
            inventory = new List<Item>()
        };

        players.Add(player);

        return player;
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
                UIManager.CreateMessage("There is no player in the room with the name " + p_name + ".");
        }
        else
            UIManager.CreateMessage("There is no player in the game with the name " + p_name + ".");
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
    public PlayerData GetPlayerByName(string name)
    {
        return players.FirstOrDefault(t => t.playerName == name);
    }

    public bool HasPlayerWithName(string name)
    {
        return players.Any(t => t.playerName == name);
    }

    public PlayerData FindPlayerById(int playerId)
    {
        return players.Find(p => p.id == playerId);
    }

    public bool CanAddPlayerToGame()
    {
        return players.Count <= 4;
    }

    public void ClearState()
    {
        IsActivePlayerSet = false;
        activePlayer = null;
        players.Clear();
    }

    public string GetMessageWhenPlayerDontHaveItem()
    {
        return "Server says: You don't have this item on your inventory.";
    }
}

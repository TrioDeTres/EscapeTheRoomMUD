using System.Collections.Generic;
using UnityEngine;

public class LobbyManager
{
    public List<PlayerData> PlayersWaiting { get; private set; }
    public bool isLobbyActive;

    private static volatile LobbyManager instance;
    private static readonly object syncRoot = new Object();

    private LobbyManager()
    {
        PlayersWaiting = new List<PlayerData>();
        isLobbyActive = true;
    }

    public static LobbyManager Instance
    {
        get
        {
            if (instance != null) return instance;

            lock (syncRoot)
            {
                if (instance == null) { 
                    instance = new LobbyManager();
                }
            }

            return instance;
        }
    }

    public void ClearState()
    {
        isLobbyActive = false;
        PlayersWaiting.Clear();
    }
}

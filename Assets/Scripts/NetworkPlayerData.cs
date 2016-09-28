
using UnityEngine.Networking;

public class NetworkPlayerData : MessageBase
{
    public int id;
    public string name;
    public int roomId;

    public NetworkPlayerData() { }

    public NetworkPlayerData(PlayerData playerData)
    {
        this.id = playerData.id;
        this.name = playerData.playerName;
        this.roomId = playerData.currentRoom.roomID;
    }

    public NetworkPlayerData(int id, string name, int roomId)
    {
        this.id = id;
        this.name = name;
        this.roomId = roomId;
    }
}
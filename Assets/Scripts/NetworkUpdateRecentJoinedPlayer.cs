using UnityEngine.Networking;

public class NetworkUpdateRecentJoinedPlayer : MessageBase
{
    public NetworkPlayerData[] players;
    public bool isLobbyStillActive;

    public NetworkUpdateRecentJoinedPlayer() {}

    public NetworkUpdateRecentJoinedPlayer(NetworkPlayerData[] players)
    {
        this.players = players;
    }
}

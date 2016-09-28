using UnityEngine.Networking;

public class NetworkUpdateRecentJoinedPlayer : MessageBase
{
    public NetworkPlayerData[] players;

    public NetworkUpdateRecentJoinedPlayer() {}

    public NetworkUpdateRecentJoinedPlayer(NetworkPlayerData[] players)
    {
        this.players = players;
    }
}

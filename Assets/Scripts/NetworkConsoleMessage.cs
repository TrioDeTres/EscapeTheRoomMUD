using UnityEngine.Networking;

public class NetworkConsoleMessage : MessageBase
{
    public int playerId;
    public string message;
    public MessageColor color;

    public NetworkConsoleMessage() {}

    public NetworkConsoleMessage(int playerId, string message, MessageColor color)
    {
        this.playerId = playerId;
        this.message = message;
        this.color = color;
    }
}

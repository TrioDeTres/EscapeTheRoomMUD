using UnityEngine.Networking;

public class NetworkDefaultMessage : MessageBase
{
    public string[] inputs;

    public NetworkDefaultMessage() {}

    public NetworkDefaultMessage(string[] inputs)
    {
        this.inputs = inputs;
    }
}

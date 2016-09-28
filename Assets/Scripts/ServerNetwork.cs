using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Assets.Scripts
{
    public class ServerNetwork : MonoBehaviour
    {
        public bool isServerStarted { get; private set; }

        public PlayersManager playersManager;
        public RoomsManager roomsManager;

        public ServerNetwork()
        {
            isServerStarted = false;
        }

        public void SendMessageToAllClients()
        {
            NetworkServer.SendToAll(MsgType.Command, new StringMessage("Hi you from server"));
        }

        public void StartServer(int p_port)
        {
            NetworkServer.Listen(p_port);

            isServerStarted = true;

            NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
            NetworkServer.RegisterHandler(MessageConstants.PLAYER_NAME, OnClientInputNickname);
        }

        public void RegisterServerHandler(short p_messageType, NetworkMessageDelegate handler)
        {
            NetworkServer.RegisterHandler(p_messageType, handler);
        }

        public void StopServer()
        {
            NetworkServer.Shutdown();
        }

        private void OnClientConnected(NetworkMessage message)
        {
            NetworkServer.SetClientReady(message.conn);

            UIManager.CreateMessage("Client " + message.conn.address + " connected to your server. Asking his name.", MessageColor.LIGHT_BLUE);

            NetworkDefaultMessage askPlayernameMessage = new NetworkDefaultMessage(new[] { "What's your name?" });

            message.conn.Send(MessageConstants.PLAYER_NAME, askPlayernameMessage);
        }

        private void OnClientInputNickname(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage("Client said his name is " + defaultMessage.inputs[0], MessageColor.LIGHT_BLUE);

            playersManager.CreatePlayer(message.conn.connectionId, defaultMessage.inputs[0]);
        }

        public void CanMoveToRoom(Action<string[]> handler, NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            PlayerData playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
            CardinalPoint cardinalPoint = (CardinalPoint)int.Parse(defaultMessage.inputs[1]);

            if (!roomsManager.TryToMoveToLockedRoom(playerData, cardinalPoint))

            handler(defaultMessage.inputs);
        }
    }
}

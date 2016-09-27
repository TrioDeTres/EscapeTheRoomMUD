using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Assets.Scripts
{
    public static class MessageConstants
    {
        public static short MOVE = 1000;
        public static short PLAYER_NAME = 1001;
    }

    public class NetworkManager : MonoBehaviour
    {
        private static bool isServerStarted = false;
        private static bool isClientConnected = false;

        private static bool isPlayerNameReady = false;

        private static NetworkClient networkClient;

        public Action<string[]> OnMoveToRoom;

        public void Start() {}

        public void TryToMoveRoomOverNetwork(PlayerData p_playerData, CardinalPoint p_cardinalPoint)
        {
            NetworkDefaultMessage message = new NetworkDefaultMessage(new [] { p_playerData.id.ToString(), ((int) p_cardinalPoint).ToString() });
            SendMessageToServer(MessageConstants.MOVE, message);
        }

        public void Connect(string p_address, int p_port)
        {
            networkClient = new NetworkClient();

            networkClient.Connect(p_address, p_port);

            isClientConnected = true;

            networkClient.RegisterHandler(MessageConstants.PLAYER_NAME, OnServerAskedPlayerName);
        }

        private void OnServerAskedPlayerName(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage(string.Join(". ", defaultMessage.inputs), MessageColor.LIGHT_BLUE);
        }

        public static void SendPlayerNameToServer(string p_playerName)
        {
            NetworkDefaultMessage responsePlayerNameMessage = new NetworkDefaultMessage(new [] { p_playerName });

            networkClient.Send(MessageConstants.PLAYER_NAME, responsePlayerNameMessage);

            isPlayerNameReady = true;
        }

        public void Disconnect()
        {
            networkClient.Disconnect();

            isServerStarted = false;
            isPlayerNameReady = false;
            isClientConnected = false;
        }

        public void SendMessageToServer(short p_messageType, NetworkDefaultMessage networkDefaultMessage)
        {
            networkClient.Send(p_messageType, networkDefaultMessage);
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
            NetworkServer.RegisterHandler(MessageConstants.MOVE, OnMoveToRoomOverNetwork);
            NetworkServer.RegisterHandler(MessageConstants.PLAYER_NAME, OnClientInputNickname);
        }

        public void StopServer()
        {
            NetworkServer.Shutdown();
        }

        private void OnClientConnected(NetworkMessage message)
        {
            NetworkServer.SetClientReady(message.conn);

            UIManager.CreateMessage("Client " + message.conn.address + " connected to your server. Asking his name.", MessageColor.LIGHT_BLUE);

            NetworkDefaultMessage askPlayernameMessage = new NetworkDefaultMessage(new [] { "What's your name?" });

            message.conn.Send(MessageConstants.PLAYER_NAME, askPlayernameMessage);
        }

        private void OnClientInputNickname(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage("Client said his name is " + defaultMessage.inputs[0], MessageColor.LIGHT_BLUE);
        }

        private void OnMoveToRoomOverNetwork(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage("Received message from client " + message.conn.address + ".", MessageColor.LIGHT_BLUE);
            OnMoveToRoom(defaultMessage.inputs);
        }

        public static bool IsLocalServerStarted()
        {
            return isServerStarted;
        }

        public static bool IsClientConnected()
        {
            return isClientConnected;
        }

        public static bool IsPlayerNameReady()
        {
            return isPlayerNameReady;
        }
    }
}

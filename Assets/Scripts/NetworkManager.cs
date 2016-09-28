using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public static class MessageConstants
    {
        public static short MOVE = 1000;
        public static short PLAYER_NAME = 1001;
    }

    public class NetworkManager : MonoBehaviour
    {
        private static ClientNetwork clientNetwork;
        private static ServerNetwork serverNetwork;

        public Action<string[]> OnMoveToRoom;

        public void Start()
        {
            clientNetwork = new ClientNetwork();
            serverNetwork = new ServerNetwork();

            serverNetwork.RegisterServerHandler(MessageConstants.MOVE, CanMoveToRoom);
            clientNetwork.RegisterServerHandler(MessageConstants.MOVE, MoveToRoom);
        }

        public void AskServerMoveToRoom(PlayerData p_playerData, CardinalPoint p_cardinalPoint)
        {
            NetworkDefaultMessage message = new NetworkDefaultMessage(new [] { p_playerData.id.ToString(), ((int) p_cardinalPoint).ToString() });
            clientNetwork.SendMessageToServer(MessageConstants.MOVE, message);
        }

        public void CanMoveToRoom(NetworkMessage message)
        {
            serverNetwork.CanMoveToRoom(OnMoveToRoom, message);
        }

        private void MoveToRoom(NetworkMessage message)
        {
            clientNetwork.MoveToRoom(OnMoveToRoom, message);
        }

        // PUBLIC SERVER METHODS
        public static bool IsClientConnected()
        {
            return clientNetwork.isClientConnected;
        }

        public void ConnectOnServer(string p_address, int p_port)
        {
            clientNetwork.Connect(p_address, p_port);
        }

        public void StartServer(int p_port)
        {
            serverNetwork.StartServer(p_port);
        }

        public void StopServer()
        {
            serverNetwork.StopServer();
        }

        public static bool IsPlayerNameReady()
        {
            return clientNetwork.isPlayerNameReady;
        }

        public static bool IsLocalServerStarted()
        {
            return serverNetwork.isServerStarted;
        }

        public static void SendPlayerNameToServer(string p_playerName)
        {
            clientNetwork.SendPlayerNameToServer(p_playerName);
        }
    }
}
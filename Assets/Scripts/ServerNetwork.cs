using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Assets.Scripts
{
    public class ServerNetwork : MonoBehaviour
    {
        public bool isServerStarted { get; private set; }

        public readonly PlayersManager playersManager;
        public readonly RoomsManager roomsManager;

        public ServerNetwork(PlayersManager playersManager, RoomsManager roomsManager)
        {
            this.playersManager = playersManager;
            this.roomsManager = roomsManager;

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

            UIManager.CreateMessage("Created player with id " + message.conn.connectionId + ".", MessageColor.LIGHT_BLUE);

            bool __leftSide = true;
            if (message.conn.connectionId == 1 || message.conn.connectionId == 3)
                roomsManager.rooms[0].playersInRoom.Add(playersManager.FindPlayerById(message.conn.connectionId));
            else
            {
                roomsManager.rooms[5].playersInRoom.Add(playersManager.FindPlayerById(message.conn.connectionId));
                __leftSide = false;
            }
            message.conn.Send(MessageConstants.PLAYER_ID, new NetworkDefaultMessage(new string[] { message.conn.connectionId.ToString(), defaultMessage.inputs[0], __leftSide.ToString() }));
        }

        public void CanMoveToRoom(Action<PlayerData, CardinalPoint> handler, NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            PlayerData playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
            CardinalPoint cardinalPoint = (CardinalPoint)int.Parse(defaultMessage.inputs[1]);

            if (roomsManager.CheckDirectionNotExists(playerData, cardinalPoint))
                message.conn.Send(MsgType.Error, new NetworkDefaultMessage(new string[] { "There is nothing on this direction.", "1" }));
            else if (roomsManager.CheckIfRoomIsLocked(playerData, cardinalPoint))
                message.conn.Send(MsgType.Error, new NetworkDefaultMessage(new string[] { roomsManager.GetMessageWhenLocked(playerData, cardinalPoint), "1" }));
            else
                handler(playerData, cardinalPoint);
        }
    }
}

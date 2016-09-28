using System;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class ClientNetwork
    {
        public bool isClientConnected { get; private set; }
        public bool isPlayerNameReady { get; private set; }

        private NetworkClient networkClient;

        public RoomsManager roomsManager;
        public PlayersManager playersManager;

        public ClientNetwork(PlayersManager playersManager, RoomsManager roomsManager)
        {
            this.playersManager = playersManager;
            this.roomsManager = roomsManager;

            isClientConnected = false;
            isPlayerNameReady = false;
        }

        public void Connect(string p_address, int p_port)
        {
            networkClient = new NetworkClient();

            networkClient.Connect(p_address, p_port);

            isClientConnected = true;

            networkClient.RegisterHandler(MessageConstants.PLAYER_NAME, OnServerAskedPlayerName);
        }

        public void Disconnect()
        {
            networkClient.Disconnect();

            isPlayerNameReady = false;
            isClientConnected = false;
        }

        public void RegisterServerHandler(short p_messageType, NetworkMessageDelegate handler)
        {
            networkClient.RegisterHandler(p_messageType, handler);
        }

        internal void SendPlayerNameToServer(object s)
        {
            throw new NotImplementedException();
        }

        private void OnServerAskedPlayerName(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage(string.Join(". ", defaultMessage.inputs), MessageColor.LIGHT_BLUE);
        }

        private void OnServerCreatePlayer(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            GameSceneManager.activePlayer.id = int.Parse(defaultMessage.inputs[0]);
            GameSceneManager.activePlayer.playerName = defaultMessage.inputs[1];

            int __id = int.Parse(defaultMessage.inputs[0]);

            playersManager.CreatePlayer(__id, defaultMessage.inputs[1]);

            if (__id == 1 || __id == 3)
                roomsManager.rooms[0].playersInRoom.Add(playersManager.FindPlayerById(message.conn.connectionId));
            else
                roomsManager.rooms[5].playersInRoom.Add(playersManager.FindPlayerById(message.conn.connectionId));
        }

        public void SendPlayerNameToServer(string p_playerName)
        {
            NetworkDefaultMessage responsePlayerNameMessage = new NetworkDefaultMessage(new[] { p_playerName });

            networkClient.Send(MessageConstants.PLAYER_NAME, responsePlayerNameMessage);

            isPlayerNameReady = true;
        }

        public void SendMessageToServer(short p_messageType, NetworkDefaultMessage networkDefaultMessage)
        {
            networkClient.Send(p_messageType, networkDefaultMessage);
        }

        public void MoveToRoom(Action<PlayerData, CardinalPoint> handle, NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            PlayerData playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
            CardinalPoint cardinalPoint = (CardinalPoint)int.Parse(defaultMessage.inputs[1]);

            handle(playerData, cardinalPoint);
        }
    }
}

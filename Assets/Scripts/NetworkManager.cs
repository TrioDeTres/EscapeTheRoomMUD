using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public static class MessageConstants
    {
        public static short MESSAGE = 1000;
        public static short MOVE = 1001;
        public static short PLAYER_NAME = 1002;
        public static short PLAYER_ID = 1003;
        public static short INITIALIZE_PLAYER_IN_ROOM = 1004;
        public static short RECENT_JOINED_PLAYER = 1005;
    }

    public class NetworkManager : MonoBehaviour
    {
        private static bool isServerStarted = false;
        private static bool isClientConnected = false;
        private static bool isPlayerNameReady = false;

        private bool isActivePlayerSet = false;

        private static NetworkClient networkClient;

        public PlayersManager playersManager;
        public RoomsManager roomsManager;

        public void AskServerMoveToRoom(PlayerData p_playerData, CardinalPoint p_cardinalPoint)
        {
            NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), ((int)p_cardinalPoint).ToString() });
            SendMessageToServer(MessageConstants.MOVE, message);
        }

        private void TryToMoveOnServer(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            PlayerData playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
            CardinalPoint cardinalPoint = (CardinalPoint) int.Parse(defaultMessage.inputs[1]);

            NetworkConnection clientConnection = message.conn;

            if (roomsManager.CheckDirectionNotExists(playerData, cardinalPoint))
            {
                clientConnection.Send(MessageConstants.MESSAGE, new NetworkDefaultMessage(new[] { playerData.id.ToString(), "There is nothing on this direction", "1" }));
            }
            else if (roomsManager.CheckIfRoomIsLocked(playerData, cardinalPoint))
            {
                clientConnection.Send(MessageConstants.MESSAGE, new NetworkDefaultMessage(new[] { playerData.id.ToString(), roomsManager.GetMessageWhenLocked(playerData, cardinalPoint), "1"}));
            }
            else
            {
                NetworkServer.SendToAll(MessageConstants.MOVE, new NetworkDefaultMessage(new[] { playerData.id.ToString(), ((int)cardinalPoint).ToString() }));
                NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkDefaultMessage(new[] { playerData.id.ToString(), "Player " + playerData.playerName + " moved.", "2" }));
            }
        }

        private void MoveOnClient(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            int networkPlayerId = int.Parse(defaultMessage.inputs[0]);

            PlayerData playerData = playersManager.FindPlayerById(networkPlayerId);
            CardinalPoint cardinalPoint = (CardinalPoint)int.Parse(defaultMessage.inputs[1]);

            roomsManager.MoveToRoom(playerData, cardinalPoint, playersManager.activePlayer.id == networkPlayerId);
        }

        private void CreatePlayerAndUpdateRoomsOnClient(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
            string playerName = defaultMessage.inputs[1];
            int roomId = int.Parse(defaultMessage.inputs[2]);

            Room room = roomsManager.FindRoomById(roomId);

            if (playersManager.FindPlayerById(networkPlayerId) == null) { 
                PlayerData player = playersManager.CreatePlayer(networkPlayerId, playerName, room);

                if (!isActivePlayerSet)
                {
                    playersManager.activePlayer = player;
                    isActivePlayerSet = true;
                }

                room.playersInRoom.Add(player);

                SendMessageToServer(MessageConstants.RECENT_JOINED_PLAYER, new NetworkPlayerData(player));
            }
        }

        private void UpdateRecentJoinedPlayerStateOnClient(NetworkMessage message)
        {
            NetworkUpdateRecentJoinedPlayer defaultMessage = message.ReadMessage<NetworkUpdateRecentJoinedPlayer>();

            foreach (NetworkPlayerData networkPlayer in defaultMessage.players)
            {
                if (playersManager.FindPlayerById(networkPlayer.id) == null) { 
                    Room room = roomsManager.FindRoomById(networkPlayer.roomId);

                    PlayerData playerData = playersManager.CreatePlayer(networkPlayer.id, networkPlayer.name, room);

                    room.playersInRoom.Add(playerData);
                }
            }

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

        public void ConnectToServer(string p_address, int p_port)
        {
            networkClient = new NetworkClient();

            networkClient.Connect(p_address, p_port);

            isClientConnected = true;

            networkClient.RegisterHandler(MessageConstants.PLAYER_NAME, OnServerAskedPlayerName);
            networkClient.RegisterHandler(MessageConstants.MESSAGE, OnServerMessage);
            networkClient.RegisterHandler(MessageConstants.MOVE, MoveOnClient);
            networkClient.RegisterHandler(MessageConstants.INITIALIZE_PLAYER_IN_ROOM, CreatePlayerAndUpdateRoomsOnClient);
            networkClient.RegisterHandler(MessageConstants.RECENT_JOINED_PLAYER, UpdateRecentJoinedPlayerStateOnClient);
        }

        private void OnServerMessage(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            int networkPlayerId = int.Parse(defaultMessage.inputs[0]);

            if (playersManager.activePlayer.id != networkPlayerId)
                UIManager.CreateMessage(defaultMessage.inputs[1], (MessageColor) int.Parse(defaultMessage.inputs[2]));
        }

        private void OnServerAskedPlayerName(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage(string.Join(". ", defaultMessage.inputs), MessageColor.LIGHT_BLUE);
        }

        public static void SendPlayerNameToServer(string p_playerName)
        {
            NetworkDefaultMessage responsePlayerNameMessage = new NetworkDefaultMessage(new[] { p_playerName });

            networkClient.Send(MessageConstants.PLAYER_NAME, responsePlayerNameMessage);

            isPlayerNameReady = true;
        }

        public void DisconnectPlayerFromServer()
        {
            networkClient.Disconnect();

            isServerStarted = false;
            isPlayerNameReady = false;
            isClientConnected = false;
            isActivePlayerSet = false;
        }

        public void SendMessageToServer(short p_messageType, MessageBase networkDefaultMessage)
        {
            networkClient.Send(p_messageType, networkDefaultMessage);
        }

        public void StartServer(int p_port)
        {
            NetworkServer.Listen(p_port);

            isServerStarted = true;

            NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnectToServer);
            NetworkServer.RegisterHandler(MessageConstants.MOVE, TryToMoveOnServer);
            NetworkServer.RegisterHandler(MessageConstants.PLAYER_NAME, OnClientInputUsername);
            NetworkServer.RegisterHandler(MessageConstants.RECENT_JOINED_PLAYER, OnClientReady);
        }

        public void StopServer()
        {
            NetworkServer.Shutdown();
        }

        private void OnClientConnectToServer(NetworkMessage message)
        {
            NetworkServer.SetClientReady(message.conn);

            UIManager.CreateMessage("Client " + message.conn.address + " connected to your server. Asking his name.", MessageColor.LIGHT_BLUE);

            NetworkDefaultMessage askPlayernameMessage = new NetworkDefaultMessage(new[] { "What's your name?" });

            message.conn.Send(MessageConstants.PLAYER_NAME, askPlayernameMessage);
        }

        private void OnClientReady(NetworkMessage message)
        {
            NetworkPlayerData defaultMessage = message.ReadMessage<NetworkPlayerData>();

            int clientId = defaultMessage.id;

            NetworkPlayerData[] networkPlayers = playersManager.players
                                                               .FindAll(p => p.id != clientId)
                                                               .Select(pd => new NetworkPlayerData(pd))
                                                               .ToArray();

            message.conn.Send(MessageConstants.RECENT_JOINED_PLAYER, new NetworkUpdateRecentJoinedPlayer(networkPlayers));
        }

        private void OnClientInputUsername(NetworkMessage message)
        {
            NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();

            UIManager.CreateMessage("Client said his name is " + defaultMessage.inputs[0], MessageColor.LIGHT_BLUE);

            // dinning room
            int roomId = 0;
            int recentJoinedPlayerId = message.conn.connectionId;

            NetworkServer.SendToAll(MessageConstants.INITIALIZE_PLAYER_IN_ROOM, new NetworkDefaultMessage(new[] { recentJoinedPlayerId.ToString(), defaultMessage.inputs[0], roomId.ToString() }));
        }
    }
}
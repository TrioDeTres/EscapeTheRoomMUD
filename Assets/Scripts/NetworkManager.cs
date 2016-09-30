﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


public static class MessageConstants
{
    public const short MESSAGE = 1000;
    public const short MOVE = 1001;
    public const short PLAYER_NAME = 1002;
    public const short PLAYER_ID = 1003;
    public const short INITIALIZE_PLAYER_IN_ROOM = 1004;
    public const short RECENT_JOINED_PLAYER = 1005;
    public const short JOIN_IN_LOBBY = 1006;
    public const short PLAYER_READY = 1007;
    public const short GET = 1008;
    public const short DROP = 1009;
}

public class NetworkManager : MonoBehaviour
{
    private static bool isServerStarted;
    private static bool isClientConnected;
    private static bool isPlayerNameReady;
    private static bool isPlayerInLobby;

    private int gameAboutToStartCounter;
        
    private NetworkClient networkClient;

    public Dictionary<int, NetworkConnection> connections;

    public PlayersManager   playersManager;
    public RoomsManager     roomsManager;
    public ItemsManager     itemsManager;

    public NetworkManager()
    {
        connections = new Dictionary<int, NetworkConnection>();
        gameAboutToStartCounter = 5;
            
    }
    ///////////////////////////////////
    // MOVE command
    public void AskServerMoveToRoom(PlayerData playerData, CardinalPoint cardinalPoint)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { playerData.id.ToString(), ((int) cardinalPoint).ToString() });
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
                
            clientConnection.Send(MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenDirectionNotExists(), MessageColor.RED));
        }
        else if (roomsManager.CheckIfRoomIsLocked(playerData, cardinalPoint))
        {
                
            clientConnection.Send(MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenLocked(playerData, cardinalPoint), MessageColor.RED));
        }
        else
        {
            NetworkServer.SendToAll(MessageConstants.MOVE, new NetworkDefaultMessage(new[] { playerData.id.ToString(), ((int)cardinalPoint).ToString() }));

            // only send messages to player in same room
            foreach (PlayerData playerInRoom in playerData.currentRoom.playersInRoom)
            {
                if (connections.ContainsKey(playerInRoom.id))
                {
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenMovedSuccessfully(playerData.playerName), MessageColor.LIGHT_BLUE));
                }
            }
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
    ///////////////////////////////////
    // GET command
    public void AskServerToGetItem(PlayerData p_playerData, string p_itemName)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_itemName });
        SendMessageToServer(MessageConstants.GET, message);
    }
    private void TryToGetItemOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __itemName = defaultMessage.inputs[1];

        NetworkConnection clientConnection = message.conn;
        //If room have item
        if (__playerData.currentRoom.HasItem(__itemName))
        {
            Item __item = __playerData.currentRoom.GetItem(__itemName);
            if (__item.isPickable)
            {  
                NetworkServer.SendToAll(MessageConstants.GET, new NetworkDefaultMessage(new[] { __playerData.id.ToString(), __itemName }));
                //Only send messages to player in same room
                foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
                {
                    if (connections.ContainsKey(playerInRoom.id))
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                        new NetworkConsoleMessage(__playerData.id,
                        itemsManager.GetMessageWhenSomeonePickedItem(__playerData.playerName, __item.GetFullName()),
                        MessageColor.LIGHT_BLUE));
                }
            }
            //Item not pickable
            else
            {
                clientConnection.Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    itemsManager.GetMessageWhenItemCantBePicked(),
                    MessageColor.RED));
            }
        }
        //Room don't have item
        else
        {
            clientConnection.Send(MessageConstants.MESSAGE, 
                new NetworkConsoleMessage(__playerData.id, 
                roomsManager.GetMessageWhenRoomDontHaveItem(), 
                MessageColor.RED));
        }
    }
    private void GetItemOnClient(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
        PlayerData __playerData = playersManager.FindPlayerById(networkPlayerId);
        string __itemName = defaultMessage.inputs[1];

        Item __item = itemsManager.GetItem(__itemName);
        __playerData.inventory.Add(__item);
        __playerData.currentRoom.items.Remove(__item);

        if (playersManager.activePlayer.id == networkPlayerId)
            UIManager.CreateDefautMessage(DefaultMessageType.GET_ITEM,
                new List<string> { __item.GetFullName() });
    }
    ///////////////////////////////////
    // DROP command
    public void AskServerToDropItem(PlayerData p_playerData, string p_itemName)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_itemName });
        SendMessageToServer(MessageConstants.DROP, message);
    }
    private void TryToDropItemOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        NetworkConnection clientConnection = message.conn;
        
        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __itemName = defaultMessage.inputs[1];
            
        //Player have item
        if (__playerData.HasItem(__itemName))
        {
            Item __item = __playerData.GetItem(__itemName);
            NetworkServer.SendToAll(MessageConstants.DROP, new NetworkDefaultMessage(new[] { __playerData.id.ToString(), __itemName }));
            //Only send messages to player in same room
            foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
            {
                if (connections.ContainsKey(playerInRoom.id))
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                        new NetworkConsoleMessage(__playerData.id,
                         itemsManager.GetMessageWhenSomeoneDroppedItem(__playerData.playerName, __item.GetFullName()),
                        MessageColor.LIGHT_BLUE));
            }
        }
        //Player don't have item
        else
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                playersManager.GetMessageWhenPlayerDontHaveItem(),
                MessageColor.RED));
    }
    private void DropItemOnClient(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
        PlayerData __playerData = playersManager.FindPlayerById(networkPlayerId);
        string __itemName = defaultMessage.inputs[1];

        Item __item = itemsManager.GetItem(__itemName);
        __playerData.currentRoom.items.Add(__item);
        __playerData.inventory.Remove(__item);

        if (playersManager.activePlayer.id == networkPlayerId)
            UIManager.CreateDefautMessage(DefaultMessageType.DROP_ITEM,
                new List<string> { __item.GetFullName() });
    }
    ///////////////////////////////////
    private void InitializePlayerOnClient(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        Room room = roomsManager.FindRoomById(playerDataMessage.roomId);

        PlayerData player = playersManager.CreatePlayer(playerDataMessage.id, playerDataMessage.name, room);

        if (!playersManager.IsActivePlayerSet && isPlayerNameReady)
        {
            playersManager.activePlayer = player;
            playersManager.IsActivePlayerSet = true;
        }

        room.playersInRoom.Add(player);

        if (playersManager.activePlayer.id == playerDataMessage.id)
        {
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

        isPlayerInLobby = defaultMessage.isLobbyStillActive;

        SendMessageToServer(MessageConstants.JOIN_IN_LOBBY, new NetworkPlayerData(playersManager.activePlayer));
    }

    private void PlayerInLobbyReadyOnclient(NetworkMessage message)
    {
        isPlayerInLobby = false;
    }

    private void ServerMessageOnClient(NetworkMessage message)
    {
        NetworkConsoleMessage networkConsoleMessage = message.ReadMessage<NetworkConsoleMessage>();

        if (playersManager.activePlayer.id != networkConsoleMessage.playerId)
        {
            UIManager.CreateMessage(networkConsoleMessage.message, networkConsoleMessage.color);
        }
    }

    private void ServerAskedPlayerNameOnClient(NetworkMessage message)
    {
        NetworkConsoleMessage defaultMessage = message.ReadMessage<NetworkConsoleMessage>();

        UIManager.CreateMessage(defaultMessage.message, MessageColor.BLUE);
    }

    public void SendPlayerNameToServer(string playerName)
    {
        networkClient.Send(MessageConstants.PLAYER_NAME, new NetworkPlayerData { name = playerName });

        isPlayerNameReady = true;
    }

    public void OnSendPlayerReadyToServer(PlayerData playerData)
    {
        networkClient.Send(MessageConstants.PLAYER_READY, new NetworkPlayerData(playerData));
    }

    public void SendMessageToPlayers(int activePlayerId, string message)
    {
        SendMessageToServer(MessageConstants.MESSAGE, new NetworkConsoleMessage(activePlayerId, message, MessageColor.WHITE));
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

    public static bool IsClientInLobby()
    {
        return isPlayerInLobby;
    }

    public void ConnectToServer(string address, int port)
    {
        networkClient = new NetworkClient();

        networkClient.Connect(address, port);

        isClientConnected = true;

        networkClient.RegisterHandler(MessageConstants.PLAYER_NAME, ServerAskedPlayerNameOnClient);
        networkClient.RegisterHandler(MessageConstants.MESSAGE, ServerMessageOnClient);
        networkClient.RegisterHandler(MessageConstants.MOVE, MoveOnClient);
        networkClient.RegisterHandler(MessageConstants.INITIALIZE_PLAYER_IN_ROOM, InitializePlayerOnClient);
        networkClient.RegisterHandler(MessageConstants.RECENT_JOINED_PLAYER, UpdateRecentJoinedPlayerStateOnClient);
        networkClient.RegisterHandler(MessageConstants.PLAYER_READY, PlayerInLobbyReadyOnclient);
        networkClient.RegisterHandler(MessageConstants.GET, GetItemOnClient);
        networkClient.RegisterHandler(MessageConstants.DROP, DropItemOnClient);
    }

    public void StartServer(int port)
    {
        NetworkServer.Listen(port);

        isServerStarted = true;

        NetworkServer.RegisterHandler(MsgType.Connect, ClientConnectedOnServer);
        NetworkServer.RegisterHandler(MessageConstants.MOVE, TryToMoveOnServer);
        NetworkServer.RegisterHandler(MessageConstants.MESSAGE, ForwardClientMessageOnServer);
        NetworkServer.RegisterHandler(MessageConstants.PLAYER_NAME, ClientInputUsernameOnServer);
        NetworkServer.RegisterHandler(MessageConstants.RECENT_JOINED_PLAYER, ClientReadyOnServer);
        NetworkServer.RegisterHandler(MessageConstants.JOIN_IN_LOBBY, PlayerJoinedInLobbyOnServer);
        NetworkServer.RegisterHandler(MessageConstants.GET, TryToGetItemOnServer);
        NetworkServer.RegisterHandler(MessageConstants.DROP, TryToDropItemOnServer);
    }

    public void ClearClientState()
    {
        isPlayerNameReady = false;
        isClientConnected = false;
        isPlayerInLobby = false;
    }

    public void ClearServerState()
    {
        isServerStarted = false;
    }

    public void DisconnectPlayerFromServer()
    {
        networkClient.Disconnect();

        ClearClientState();
        playersManager.ClearState();
        roomsManager.ClearState();

        if (isServerStarted)
        {
            StopServer();
        }
    }

    public void SendMessageToServer(short messageType, MessageBase networkDefaultMessage)
    {
        networkClient.Send(messageType, networkDefaultMessage);
    }

    public void StopServer()
    {
        NetworkServer.Shutdown();
        ClearServerState();
    }

    private void ForwardClientMessageOnServer(NetworkMessage message)
    {
        NetworkConsoleMessage consoleMessage = message.ReadMessage<NetworkConsoleMessage>();
        NetworkServer.SendToAll(MessageConstants.MESSAGE, consoleMessage);
    }

    private void ClientConnectedOnServer(NetworkMessage message)
    {
        if (!playersManager.CanAddPlayerToGame())
        {
            UIManager.CreateMessage("Client " + message.conn.address + " tried to connect on server but it's full.", MessageColor.RED);
            NetworkServer.SendToClient(message.conn.connectionId, MessageConstants.MESSAGE, new NetworkConsoleMessage() { message = "Server says: Sorry, server is full. Try again later." });
            message.conn.Disconnect();
        }

        NetworkServer.SetClientReady(message.conn);

        UIManager.CreateMessage("Client " + message.conn.address + " connected to your server. Asking his name.", MessageColor.LIGHT_BLUE);

        NetworkServer.SendToClient(message.conn.connectionId, MessageConstants.PLAYER_NAME, new NetworkConsoleMessage() { message = "Server says: What's your name?" });
    }

    private void PlayerJoinedInLobbyOnServer(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        PlayerData playerData = playersManager.FindPlayerById(playerDataMessage.id);

        LobbyManager lobbyManager = LobbyManager.Instance;

        if (!lobbyManager.isLobbyActive) {
           NetworkServer.SendToClient(playerData.id, MessageConstants.PLAYER_READY, NetworkEmptyMessage.EMPTY);
        }
        else if (lobbyManager.PlayersWaiting.Count >= 1)
        {
            lobbyManager.ClearState();
                
            NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Servers says: Enough players in lobby, game will start soon.", MessageColor.BLUE));

            StartCoroutine(GameAboutToStartMessage());
        }
        else
        {
            LobbyManager.Instance.PlayersWaiting.Add(playerData);
            NetworkServer.SendToClient(playerData.id, MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Server says: Welcome to EscapeTheRoom lobby. Waiting for others players to start game. When prepared type 'ready'.", MessageColor.BLUE));
        }
    }

    private IEnumerator GameAboutToStartMessage()
    {
        while (gameAboutToStartCounter != 0)
        {
            gameAboutToStartCounter -= 1;

            NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Server says: Game starting in " + gameAboutToStartCounter + " seconds.", MessageColor.BLUE));

            yield return new WaitForSeconds(1.0f);
        }

        NetworkServer.SendToAll(MessageConstants.PLAYER_READY, NetworkEmptyMessage.EMPTY);
        NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Server says: GO!", MessageColor.BLUE));
    }

    private void ClientReadyOnServer(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        int clientId = playerDataMessage.id;

        NetworkPlayerData[] networkPlayers = playersManager.players
                                                            .FindAll(p => p.id != clientId)
                                                            .Select(pd => new NetworkPlayerData(pd))
                                                            .ToArray();

        NetworkUpdateRecentJoinedPlayer updateRecentJoinedPlayerMessage = new NetworkUpdateRecentJoinedPlayer(networkPlayers)
        {
            isLobbyStillActive = LobbyManager.Instance.isLobbyActive
        };

        NetworkServer.SendToClient(clientId, MessageConstants.RECENT_JOINED_PLAYER, updateRecentJoinedPlayerMessage);
    }

    private void ClientInputUsernameOnServer(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        UIManager.CreateMessage("Client said his name is " + playerDataMessage.name, MessageColor.LIGHT_BLUE);

        int recentJoinedPlayerId = message.conn.connectionId;

        Room room = roomsManager.PickRoomForPlayer(recentJoinedPlayerId);

        playerDataMessage.id = recentJoinedPlayerId;
        playerDataMessage.roomId = room.roomID;

        connections.Add(recentJoinedPlayerId, message.conn);

        NetworkServer.SendToAll(MessageConstants.INITIALIZE_PLAYER_IN_ROOM, playerDataMessage);
    }
}
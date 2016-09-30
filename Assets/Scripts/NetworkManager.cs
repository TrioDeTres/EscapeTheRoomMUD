using System;
using System.Collections;
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
    public const short USE = 1010;
    public const short USE_SUITCASE = 1011;
    public const short USE_HOLE = 1012;
    public const short SAY = 1013;
    public const short SAY_HOLE = 1014;
    public const short WHISPER = 1015;
}

public class NetworkManager : MonoBehaviour
{
    public event Action<int> OnSuitcaseOpened;
    public event Action<int, string, string> OnPlayInteraction;
    public event Action<int, string> OnUseHole;

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
//<<<<<<< HEAD
//                if (playerInRoom.id == 0) continue;
//                NetworkServer.SendToClient(playerInRoom.id, MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenMovedOutSuccessfully(playerData.playerName), MessageColor.LIGHT_BLUE));
//            }

//            foreach (PlayerData playerInRoom in roomsManager.FindRoomInDirection(playerData, cardinalPoint).playersInRoom)
//            {
//                if (playerInRoom.id == 0) continue;
//                NetworkServer.SendToClient(playerInRoom.id, MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenMovedInSuccessfully(playerData.playerName), MessageColor.LIGHT_BLUE));
//=======
                if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != playerData.id)
                {
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, roomsManager.GetMessageWhenMovedOutSuccessfully(playerData.playerName), MessageColor.LIGHT_BLUE));
                }
//>>>>>>> origin/Gabriel
            }
            // only send messages to player in the target room
            foreach (PlayerData playerInRoom in playerData.currentRoom.
                adjacentRooms[(int)cardinalPoint].targetRoom.playersInRoom)
            {
                if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != playerData.id)
                {
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE, new NetworkConsoleMessage(playerData.id, 
                        "Server says: " + playerData.playerName + " has joined the room.", MessageColor.LIGHT_BLUE));
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
//<<<<<<< HEAD
//                    if (playerInRoom.id == 0) continue;
//                    NetworkServer.SendToClient(playerInRoom.id, 
//                                                MessageConstants.MESSAGE, 
//                                                new NetworkConsoleMessage(__playerData.id, 
//                                                                            itemsManager.GetMessageWhenSomeonePickedItem(__playerData.playerName, 
//                                                                                                                        __item.GetFullName()), 
//                                                MessageColor.LIGHT_BLUE));
//=======
                    if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != __playerData.id)
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                        new NetworkConsoleMessage(__playerData.id,
                        itemsManager.GetMessageWhenSomeonePickedItem(__playerData.playerName, __item.GetFullName()),
                        MessageColor.LIGHT_BLUE));
//>>>>>>> origin/Gabriel
                }
            }
            //Item not pickable
            else
            {
                NetworkServer.SendToClient(__playerData.id, 
                                           MessageConstants.MESSAGE,
                                           new NetworkConsoleMessage(__playerData.id, itemsManager.GetMessageWhenItemCantBePicked(),
                                           MessageColor.RED));
            }
        }
        //Room don't have item
        else
        {
            NetworkServer.SendToClient(__playerData.id, 
                                       MessageConstants.MESSAGE, 
                                       new NetworkConsoleMessage(__playerData.id,  roomsManager.GetMessageWhenRoomDontHaveItem(), 
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
//<<<<<<< HEAD
//                if (playerInRoom.id == 0) continue;
//                NetworkServer.SendToClient(playerInRoom.id, 
//                                            MessageConstants.MESSAGE, 
//                                            new NetworkConsoleMessage(__playerData.id, itemsManager.GetMessageWhenSomeoneDroppedItem(__playerData.playerName, 
//                                                                                                                                    __item.GetFullName()),
//                                            MessageColor.LIGHT_BLUE));
//=======
                if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != __playerData.id)
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                        new NetworkConsoleMessage(__playerData.id,
                         itemsManager.GetMessageWhenSomeoneDroppedItem(__playerData.playerName, __item.GetFullName()),
                        MessageColor.LIGHT_BLUE));
//>>>>>>> origin/Gabriel
            }
        }
        //Player don't have item
        else
            NetworkServer.SendToClient(__playerData.id, 
                                       MessageConstants.MESSAGE,
                                       new NetworkConsoleMessage(__playerData.id, playersManager.GetMessageWhenPlayerDontHaveItem(),
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
    // USE command
    public void AskServerToUseItem(PlayerData p_playerData, string p_source, string p_target)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_source, p_target});
        SendMessageToServer(MessageConstants.USE, message);
    }
    private void TryToUseItemOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        NetworkConnection clientConnection = message.conn;

        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __sourceName = defaultMessage.inputs[1];
        string __targetName = defaultMessage.inputs[2];
        //Player don't have the item
        if (!__playerData.HasItem(__sourceName))
        {
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                playersManager.GetMessageWhenPlayerDontHaveItem(),
                MessageColor.RED));
        }
        //There is no target item on the room
        else if (!__playerData.currentRoom.HasItem(__targetName))
        {
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                roomsManager.GetMessageWhenRoomDontHaveTargetItem(),
                MessageColor.RED));
        }
        //The items exist
        else
        {
            Item __source = __playerData.GetItem(__sourceName);
            Item __target = __playerData.currentRoom.GetItem(__targetName);

            //Source can't be used
            if (!__source.isUsable)
                clientConnection.Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    itemsManager.GetMessageWhenItemCantBeUsed(),
                    MessageColor.RED));
            //No interaction between items
            else if (!__target.HasInteractionWithItem(__source))
                clientConnection.Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    itemsManager.GetMessageWhenNoInteraction(),
                    MessageColor.RED));
            //Interaction not active
            else if (!__target.GetInteraction(__source).active)
                clientConnection.Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    itemsManager.GetMessageWhenInteractionNotActive(),
                    MessageColor.RED));
            else
            {
                NetworkServer.SendToAll(MessageConstants.USE, new NetworkDefaultMessage(new[] { __playerData.id.ToString(), __sourceName, __targetName }));
                //Only send messages to player in same room
                foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
                {
                    if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != __playerData.id)
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                            new NetworkConsoleMessage(__playerData.id,
                            itemsManager.GetMessageWhenSomeoneUsedItem(playerInRoom.playerName, __sourceName, __targetName),
                            MessageColor.LIGHT_BLUE));
                }
            }
        }
    }
    private void UseItemOnClient(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
        OnPlayInteraction(networkPlayerId, defaultMessage.inputs[1], defaultMessage.inputs[2]);
    }
    ///////////////////////////////////
    // USE_HOLE command
    public void AskServerToUseHole(PlayerData p_playerData, string p_source)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_source});
        SendMessageToServer(MessageConstants.USE_HOLE, message);
    }
    private void TryToUseHoleOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        NetworkConnection clientConnection = message.conn;

        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __sourceName = defaultMessage.inputs[1];

        //Player don't have the item
        if (!__playerData.HasItem(__sourceName))
        {
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                playersManager.GetMessageWhenPlayerDontHaveItem(),
                MessageColor.RED));
        }
        //There is no target item on the room
        else if (__playerData.currentRoom.roomID != 0 && __playerData.currentRoom.roomID != 4)
        {
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                "Server says: There is no hole in the room",
                MessageColor.RED));
        }
        //The items exist
        else
        {
            Item __source = __playerData.GetItem(__sourceName);

            //Source can't be used
            if (!__source.isTransferable)
                clientConnection.Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    itemsManager.GetMessageWhenItemCantBeTransfered(),
                    MessageColor.RED));
            else
            {
                NetworkServer.SendToAll(MessageConstants.USE_HOLE, new NetworkDefaultMessage(new[] { __playerData.id.ToString(), __sourceName}));
                //Only send messages to player in same room
                foreach (PlayerData playerInRoom in playersManager.players)
                {
                    if (playerInRoom.currentRoom.roomID == 0 || playerInRoom.currentRoom.roomID == 4)
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                            new NetworkConsoleMessage(__playerData.id,
                            itemsManager.GetMessageWhenSomeoneSendItem(playerInRoom.playerName, __sourceName),
                            MessageColor.LIGHT_BLUE));
                }
            }
        }
    }
    private void UseHoleOnClient(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
        OnUseHole(networkPlayerId, defaultMessage.inputs[1]);
    }
    ///////////////////////////////////
    // USE_SUITCASE command
    public void AskServerToUseSuitcase(PlayerData p_playerData, string p_password)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_password });
        SendMessageToServer(MessageConstants.USE_SUITCASE, message);
    }
    private void TryToUseSuitcaseOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        NetworkConnection clientConnection = message.conn;

        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __password = defaultMessage.inputs[1];
        long __result = 0;

        //Player is not on the Office
        if (__playerData.currentRoom != roomsManager.rooms[1])
        {
            clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                itemsManager.GetMessageWhenItemNotFound(),
                MessageColor.RED));
        }
        //Valid password
        else if (long.TryParse(__password, out __result))
        {
            //Right password
            if (__result == 895402763152)
            {
                NetworkServer.SendToAll(MessageConstants.USE_SUITCASE, new NetworkDefaultMessage(new[] { __playerData.id.ToString() }));
                //Only send messages to player in same room
                foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
                {
                    if (connections.ContainsKey(playerInRoom.id) && playerInRoom.id != __playerData.id)
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                            new NetworkConsoleMessage(__playerData.id,
                            itemsManager.GetMessageWhenSomeoneUsedSuitcase(playerInRoom.playerName),
                            MessageColor.LIGHT_BLUE));
                }
            }
            //Wrong password
            else
            {
                clientConnection.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                itemsManager.GetMessageWhenWrondPassword(),
                MessageColor.RED));
            }
        }
        //Invalid password
        else
            clientConnection.Send(MessageConstants.MESSAGE,
                 new NetworkConsoleMessage(__playerData.id,
                 itemsManager.GetMessageWhenInvalidPassword(),
                 MessageColor.RED));
    }
    private void UseSuitcaseOnClient(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        int networkPlayerId = int.Parse(defaultMessage.inputs[0]);
        OnSuitcaseOpened(networkPlayerId);
    }
    ///////////////////////////////////
    // SAY command
    public void AskServerToSay(PlayerData p_playerData, string p_message)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_message });
        SendMessageToServer(MessageConstants.SAY, message);
    }
    private void TryToSayOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __message = defaultMessage.inputs[1];

        foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
        {
            if (playerInRoom.id != __playerData.id)
            {
                if (connections.ContainsKey(playerInRoom.id))
                    connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                    new NetworkConsoleMessage(__playerData.id,
                    __playerData.playerName + " says: " + __message,
                    MessageColor.WHITE));
            }
        }
    }
    ///////////////////////////////////
    // SAY_HOLE command
    public void AskServerToSayHole(PlayerData p_playerData, string p_message)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_message });
        SendMessageToServer(MessageConstants.SAY_HOLE, message);
    }
    private void TryToSayHoleOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __message = defaultMessage.inputs[1];

        if (__playerData.currentRoom.roomID != 0 && __playerData.currentRoom.roomID != 4)
            message.conn.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                "There is no hole in the room.",
                MessageColor.RED));
        else
        {
            int __targetRoomId = 0;
            if (__playerData.currentRoom.roomID == 0)
                __targetRoomId = 4;
            foreach (PlayerData playerInRoom in roomsManager.FindRoomById(__targetRoomId).playersInRoom)
            {
                if (playerInRoom.id != __playerData.id)
                {
                    if (connections.ContainsKey(playerInRoom.id))
                        connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                        new NetworkConsoleMessage(__playerData.id,
                        __playerData.playerName + " says from the hole: " + __message,
                        MessageColor.WHITE));
                }
            }
        }
    }
    ///////////////////////////////////
    // WHISPER command
    public void AskServerToWhisper(PlayerData p_playerData, string p_target, string p_message)
    {
        NetworkDefaultMessage message = new NetworkDefaultMessage(new[] { p_playerData.id.ToString(), p_target ,p_message });
        SendMessageToServer(MessageConstants.WHISPER, message);
    }
    private void TryToWhisperOnServer(NetworkMessage message)
    {
        NetworkDefaultMessage defaultMessage = message.ReadMessage<NetworkDefaultMessage>();
        PlayerData __playerData = playersManager.FindPlayerById(int.Parse(defaultMessage.inputs[0]));
        string __target = defaultMessage.inputs[1];
        string __message = defaultMessage.inputs[2];

        bool __messageSend = false;
        foreach (PlayerData playerInRoom in __playerData.currentRoom.playersInRoom)
        {
            if (connections.ContainsKey(playerInRoom.id) && playerInRoom.playerName == __target)
            {
                connections[playerInRoom.id].Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                __playerData.playerName + " whispers: " + __message,
                MessageColor.GREEN));
                __messageSend = true;
                message.conn.Send(MessageConstants.MESSAGE,
                new NetworkConsoleMessage(__playerData.id,
                "You whispers to " + __target + ": " + __message,
                MessageColor.GREEN));
                __messageSend = true;
            }
        }
        if (!__messageSend)
            message.conn.Send(MessageConstants.MESSAGE,
                 new NetworkConsoleMessage(__playerData.id,
                 "There is nobody in the room with this name.",
                 MessageColor.RED));
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

    private void PlayerInLobbyOnclient(NetworkMessage message)
    {
        isPlayerInLobby = true;
    }

    private void PlayerReadyOnclient(NetworkMessage message)
    {
        isPlayerInLobby = false;
        playersManager.activePlayer.ready = true;
    }

    private void ServerMessageOnClient(NetworkMessage message)
    {
        NetworkConsoleMessage networkConsoleMessage = message.ReadMessage<NetworkConsoleMessage>();
        UIManager.CreateMessage(networkConsoleMessage.message, networkConsoleMessage.color);
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

    public void SendPlayerReadyToServer(PlayerData playerData)
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
        networkClient.RegisterHandler(MessageConstants.JOIN_IN_LOBBY, PlayerInLobbyOnclient);
        networkClient.RegisterHandler(MessageConstants.PLAYER_READY, PlayerReadyOnclient);
        networkClient.RegisterHandler(MessageConstants.GET, GetItemOnClient);
        networkClient.RegisterHandler(MessageConstants.DROP, DropItemOnClient);
        networkClient.RegisterHandler(MessageConstants.USE, UseItemOnClient);
        networkClient.RegisterHandler(MessageConstants.USE_SUITCASE, UseSuitcaseOnClient);
        networkClient.RegisterHandler(MessageConstants.USE_HOLE, UseHoleOnClient);
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
        NetworkServer.RegisterHandler(MessageConstants.PLAYER_READY, PlayerReadyOnServer);
        NetworkServer.RegisterHandler(MessageConstants.GET, TryToGetItemOnServer);
        NetworkServer.RegisterHandler(MessageConstants.DROP, TryToDropItemOnServer);
        NetworkServer.RegisterHandler(MessageConstants.USE, TryToUseItemOnServer);
        NetworkServer.RegisterHandler(MessageConstants.USE_SUITCASE, TryToUseSuitcaseOnServer);
        NetworkServer.RegisterHandler(MessageConstants.USE_HOLE, TryToUseHoleOnServer);
        NetworkServer.RegisterHandler(MessageConstants.SAY, TryToSayOnServer);
        NetworkServer.RegisterHandler(MessageConstants.SAY_HOLE, TryToSayHoleOnServer);
        NetworkServer.RegisterHandler(MessageConstants.WHISPER, TryToWhisperOnServer);
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

    private void PlayerReadyOnServer(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        PlayerData playerData = playersManager.FindPlayerById(playerDataMessage.id);

        playerData.ready = true;

        LobbyManager lobbyManager = LobbyManager.Instance;

        if (lobbyManager.IsEveryoneReady())
        {
            lobbyManager.ClearState();

            NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Servers says: Enough players in lobby, game will start soon.", MessageColor.BLUE));

            StartCoroutine(GameAboutToStartMessage());
        }

        NetworkServer.SendToAll(MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Servers says: " + playerData.playerName + " is ready.", MessageColor.BLUE));
    }

    private void PlayerJoinedInLobbyOnServer(NetworkMessage message)
    {
        NetworkPlayerData playerDataMessage = message.ReadMessage<NetworkPlayerData>();

        PlayerData playerData = playersManager.FindPlayerById(playerDataMessage.id);

        if (!LobbyManager.Instance.isLobbyActive) {
            NetworkServer.SendToClient(playerData.id, MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Server says: Game already started, have fun!", MessageColor.BLUE));
            NetworkServer.SendToClient(playerData.id, MessageConstants.PLAYER_READY, NetworkEmptyMessage.EMPTY);
        }
        else
        {
            LobbyManager.Instance.PlayersWaiting.Add(playerData);
            NetworkServer.SendToClient(playerData.id, MessageConstants.JOIN_IN_LOBBY, NetworkEmptyMessage.EMPTY);
            NetworkServer.SendToClient(playerData.id, MessageConstants.MESSAGE, new NetworkConsoleMessage(0, "Server says: Welcome to EscapeTheRoom lobby. Waiting for others players to start game. When prepared type 'ready' or 'r'.", MessageColor.BLUE));
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

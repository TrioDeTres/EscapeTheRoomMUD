using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net;

public class CommandManager : MonoBehaviour
{
    public event Action<CardinalPoint>          OnTryToMoveToRoom;
    public event Action<string>                 OnTryToShowInventory;
    public event Action<string, string>         OnTryToUseItem;
    public event Action<string>                 OnTryToUseHole;
    public event Action<string>                 OnTryToUseSuitcase;
    public event Action<string>                 OnTryToGetItem;
    public event Action<string>                 OnTryToDropItem;
    public event Action                         OnTryToLookRoom;
    public event Action<int>                    OnTryToStartServer;
    public event Action                         OnTryToStopServer;
    public event Action<string, int>            OnTryToConnectOnServer;
    public event Action<string>                 OnTryToLookItem;
    public event Action<int, string>            OnSendMessageToPlayers;
    public event Action<string>                 OnSendPlayerNameToServer;
    public event Action                         OnSendPlayerReadyToServer;

    public PlayersManager playersManager;

    public void ParseMessage(string p_msg)
    {
        PlayCommand(p_msg.Split(' ').Select(w => w.Trim().ToLower()).ToList());        
    }

    public void PlayCommand(List<string> args)
    {
        if (args.Count == 0)
            return;

        if (!NetworkManager.IsPlayerNameReady() && (string.Equals("ready", args[0]) || string.Equals("r", args[0])))
        {
            UIManager.CreateMessage("Server says: Choose a different name.", MessageColor.WHITE);
            return;
        }

        if (NetworkManager.IsClientInLobby() && NetworkManager.IsPlayerNameReady() && !string.Equals("ready", args[0]) && !string.Equals("r", args[0]))
        {
            string fullText = string.Join(" ", args.ToArray());

            UIManager.CreateMessage("You said: " + fullText, MessageColor.WHITE);
            OnSendMessageToPlayers(playersManager.activePlayer.id, playersManager.activePlayer.playerName + " says: " + fullText);
            return;
        }

        // commands that partially affect network
        switch (args[0])
        {
            case "ready":
            case "r":
                OnSendPlayerReadyToServer();
                break;

            case "connect":
            case "n":
                const int defaultPort = 2300;

                if (NetworkManager.IsClientConnected())
                {
                    UIManager.CreateMessage("Client already connected.", MessageColor.RED);
                    return;
                }

                if (args.Count == 3)
                {
                    int port = defaultPort;

                    if (!int.TryParse(args[2], out port) || !Util.IsAddresValidIPV4(args[1]))
                    {
                        UIManager.CreateMessage("Could not connect in server " + args[1] + " on port " + args[2] + ".", MessageColor.RED);
                        return;
                    }

                    OnTryToConnectOnServer(args[1], port);

                    if (NetworkManager.IsClientConnected()) { 
                        UIManager.CreateMessage("Connecting to " + args[1] + " on port " + args[2] + ".", MessageColor.YELLOW);
                    }

                    return;
                }
                else
                {
                    OnTryToConnectOnServer(IPAddress.Loopback.ToString(), 2300);

                    if (NetworkManager.IsClientConnected())
                        UIManager.CreateMessage("Connecting under local server on default port (2300).", MessageColor.YELLOW);

                    return;
                }

            case "start":
            case "t":
                if (NetworkManager.IsLocalServerStarted())
                {
                    UIManager.CreateMessage("Server already started.", MessageColor.RED);
                    return;
                }

                if (args.Count == 2)
                {
                    OnTryToStartServer(int.Parse(args[1]));

                    if (NetworkManager.IsLocalServerStarted())
                        UIManager.CreateMessage("You successfully started server on port " + args[1] + ".", MessageColor.YELLOW);
                }
                else
                {
                    OnTryToStartServer(2300);

                    if (NetworkManager.IsLocalServerStarted())
                        UIManager.CreateMessage("You successfully started server on default port (2300).", MessageColor.YELLOW);
                }

                return;

            case "stop":
            case "p":
                if (NetworkManager.IsLocalServerStarted())
                {
                    OnTryToStopServer();
                    UIManager.CreateMessage("Server shutting down.", MessageColor.YELLOW);
                }
                else
                {
                    UIManager.CreateMessage("Server isn't running.", MessageColor.RED);
                }

                return;

            case "help":
            case "h":
                UIManager.CreateDefautMessage(DefaultMessageType.HELP_CMD_HELP_TEXT);
                return;

            case "clear":
            case "c":
                UIManager.ClearMessages();
                return;
        }

        // cannot proceed without being connected to server
        if (!NetworkManager.IsClientConnected() && !NetworkManager.IsLocalServerStarted())
        {
            UIManager.CreateMessage("You aren't connected to a server. Type help for instructions.", MessageColor.RED);
            return;
        }

        // complete handshake responding with player name
        if (!NetworkManager.IsPlayerNameReady())
        {
            OnSendPlayerNameToServer(args[0]);
            return;
        }

        switch (args[0])
        {
            case "look":
            case "l":
                if (args.Count == 1 || args[1] == "room")
                    OnTryToLookRoom();
                else
                    OnTryToLookItem(args[1]);
                return;
            //--------------------
            //Room commands
            case "move":
            case "m":
                if (args.Count == 1)
                    UIManager.CreateDefautMessage(DefaultMessageType.WRONG_DIRECTION);
                else
                    MoveCommand(args[1]);
                break;
            
            //--------------------
            //Items commands
            case "inventory":
            case "i":
                if (args.Count == 1)
                    OnTryToShowInventory("");
                else
                    OnTryToShowInventory(args[1]);
                break;
            case "get":
            case "g":
                if (args.Count == 1)
                    UIManager.CreateMessage("This command requires an item name.", MessageColor.RED);
                else
                    OnTryToGetItem(args[1]);
                break;
            case "drop":
            case "d":
                if (args.Count == 1)
                    UIManager.CreateMessage("This command requires an item name.", MessageColor.RED);
                else
                    OnTryToDropItem(args[1]);
                break;
            case "use":
            case "u":
                if (args.Count <= 2)
                    UIManager.CreateMessage("This command requires an item name and a target.", 
                        MessageColor.RED);
                else if (args[1].ToLower() == "hole")
                    OnTryToUseHole(args[2]);
                else if (args[2].ToLower() == "suitcase")
                    OnTryToUseSuitcase(args[1]);
                else
                    OnTryToUseItem(args[1], args[2]);
                break;
            //--------------------
            //Chat commands
            case "say":
            case "s":
                Debug.Log("Say something(args[1]) to all players");
                break;
            case "whisper":
            case "w":
                Debug.Log("Whisper something(args[2]) to player (args[1])");
                break;
        }
    }

    public void MoveCommand(string p_direction)
    {
        switch (p_direction.ToLower())
        {
            case "north":
            case "n":
                OnTryToMoveToRoom(CardinalPoint.NORTH);
                break;
            case "south":
            case "s":
                OnTryToMoveToRoom(CardinalPoint.SOUTH);
                break;
            case "west":
            case "w":
                OnTryToMoveToRoom(CardinalPoint.WEST);
                break;
            case "east":
            case "e":
                OnTryToMoveToRoom(CardinalPoint.EAST);
                break;

            default:
                UIManager.CreateDefautMessage(DefaultMessageType.WRONG_DIRECTION);
                break;
        }
    }
}

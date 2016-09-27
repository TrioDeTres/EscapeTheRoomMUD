using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts;

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
    public event Action<string, int, string>    OnTryToConnectOnServer;
    public event Action<string>                 OnTryToLookItem;

    public void ParseMessage(string p_msg)
    {
        PlayCommand(p_msg.Split(' ').Select(w => w.Trim().ToLower()).ToList());        
    }

    public void PlayCommand(List<string> p_params)
    {
        if (p_params.Count == 0)
            return;

        // commands that partially affect network
        switch (p_params[0])
        {
            case "connect":
            case "n":
                if (NetworkManager.IsClientConnected())
                {
                    UIManager.CreateMessage("Client already connected.", MessageColor.RED);
                    return;
                }

                if (p_params.Count == 4)
                {
                    OnTryToConnectOnServer(p_params[1], int.Parse(p_params[2]), p_params[3]);

                    if (NetworkManager.IsClientConnected())
                        UIManager.CreateMessage("Connecting under local server on port " + p_params[2] + ".", MessageColor.YELLOW);
                }
                else if (p_params.Count == 2)
                {
                    OnTryToConnectOnServer("127.0.0.1", 2300, p_params[1]);

                    if (NetworkManager.IsClientConnected())
                        UIManager.CreateMessage("Connecting under local server on default port (2300).", MessageColor.YELLOW);
                }
                else
                {
                    OnTryToConnectOnServer("127.0.0.1", 2300, null);

                    if (NetworkManager.IsClientConnected())
                        UIManager.CreateMessage("Connecting under local server on default port (2300).", MessageColor.YELLOW);
                }

                return;
            case "start":
            case "t":
                if (NetworkManager.IsLocalServerStarted())
                {
                    UIManager.CreateMessage("Server already started.", MessageColor.RED);
                    return;
                }

                if (p_params.Count == 2)
                {
                    OnTryToStartServer(int.Parse(p_params[1]));

                    if (NetworkManager.IsLocalServerStarted())
                        UIManager.CreateMessage("You successfully started server on port " + p_params[1] + ".", MessageColor.YELLOW);
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
            case "look":
            case "l":
                if (p_params.Count == 1 || p_params[1] == "room")
                    OnTryToLookRoom();
                else
                    OnTryToLookItem(p_params[1]);
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
            NetworkManager.SendPlayerNameToServer(p_params[0]);
            return;
        }

        switch (p_params[0])
        {
            //--------------------
            //Room commands
            case "move":
            case "m":
                if (p_params.Count == 1)
                    UIManager.CreateDefautMessage(DefaultMessageType.WRONG_DIRECTION);
                else
                    MoveCommand(p_params[1]);
                break;
            
            //--------------------
            //Items commands
            case "inventory":
            case "i":
                if (p_params.Count == 1)
                    OnTryToShowInventory("");
                else
                    OnTryToShowInventory(p_params[1]);
                break;
            case "get":
            case "g":
                if (p_params.Count == 1)
                    UIManager.CreateMessage("This command requires an item name.", MessageColor.RED);
                else
                    OnTryToGetItem(p_params[1]);
                break;
            case "drop":
            case "d":
                if (p_params.Count == 1)
                    UIManager.CreateMessage("This command requires an item name.", MessageColor.RED);
                else
                    OnTryToDropItem(p_params[1]);
                break;
            case "use":
            case "u":
                if (p_params.Count <= 2)
                    UIManager.CreateMessage("This command requires an item name and a target.", 
                        MessageColor.RED);
                else if (p_params[1].ToLower() == "hole")
                    OnTryToUseHole(p_params[2]);
                else if (p_params[2].ToLower() == "suitcase")
                    OnTryToUseSuitcase(p_params[1]);
                else
                    OnTryToUseItem(p_params[1], p_params[2]);
                break;
            //--------------------
            //Chat commands
            case "say":
            case "s":
                Debug.Log("Say something(p_params[1]) to all players");
                break;
            case "whisper":
            case "w":
                Debug.Log("Whisper something(p_params[2]) to player (p_params[1])");
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CommandManager : MonoBehaviour
{
    public event Action<CardinalPoint>  OnTryToMoveToRoom;
    public event Action<string>         OnTryToShowInventory;
    public event Action<string>         OnTryToGetItem;
    public event Action<string>         OnTryToDropItem;
    public event Action                 OnTryToLookRoom;
    public event Action<string>         OnTryToLookItem;

    public event Action<DefaultMessageType, List<string>> OnMessageError;

    public void ParseMessage(string p_msg)
    {
        PlayCommand(p_msg.Split(' ').Select(w => w.Trim().ToLower()).ToList());        
    }

    public void PlayCommand(List<string> p_params)
    {
        if (p_params.Count == 0)
            return;
        switch (p_params[0].ToLower())
        {
            case "help":
            case "h":
                UIManager.CreateDefautMessage(DefaultMessageType.HELP_CMD_HELP_TEXT);
                break;
            case "clear":
            case "c":
                UIManager.ClearMessages();
                break;
            //--------------------
            //Room commands
            case "move":
            case "m":
                if (p_params.Count == 1)
                    UIManager.CreateDefautMessage(DefaultMessageType.MOVE_CMD_WRONG_DIRECTION);
                else
                    MoveCommand(p_params[1]);
                break;
            case "look":
            case "l":
                if (p_params.Count == 2)
                    OnTryToLookItem(p_params[1]);
                else
                    OnTryToLookRoom();
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
                Debug.Log("Use the item(p_params[1]) from room/inventory on target(p_params[2])");
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
                OnMessageError(DefaultMessageType.MOVE_CMD_WRONG_DIRECTION, new List<string>());
                break;
        }
    }
}

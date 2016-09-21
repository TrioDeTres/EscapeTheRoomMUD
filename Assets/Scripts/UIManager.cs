﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DefaultMessageType
{
    HELP_CMD_HELP_TEXT,
    MOVE_CMD_WRONG_DIRECTION,
    MOVE_CMD_NO_ROOM_IN_DIRECTION,
    MOVE_CMD_LOCKED_ROOM,
    INVENTORY_CMD_NO_PLAYER_IN_GAME,
    INVENTORY_CMD_NO_PLAYER_IN_ROOM,
    INVENTORY_CMD_EMPTY_INVENTORY,
    INVENTORY_CMD_SHOW_INVENTORY,
    GET_CMD_GET_ITEM,
    DROP_CMD_DROP_ITEM,
    ITEM_NOT_FOUND
}
public enum MessageColor
{
    WHITE,
    RED,
    BLUE,
    YELLOW
}
public class UIManager : MonoBehaviour
{
    public event Action<string> OnExecuteMessage;

    private static UIManager uiManager;

    public InputField   inputField;
    public Button       sendButton;
    public Scrollbar    verticalScrollBar;

    public GameObject   uiMessagePrefab;
    public GameObject   messagesContainer;

    public List<Text>   texts;

    public List<GameObject> uiDefaultMessagesPrefabs;

    void Awake()
    {
        uiManager = this;
    }
    void Start()
    {
        inputField.ActivateInputField();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            ExecuteMassage();
        //if (Input.GetKeyDown(KeyCode.A))
        //    CreateUIMessage("lala");
    }

    public void ExecuteMassage()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;
        if (OnExecuteMessage != null)
            OnExecuteMessage(inputField.text);
        inputField.text = "";
    }

    public static void ClearMessages()
    {
        uiManager.ClearUIMessages();
    }
    public static void CreateMessage(string p_message, MessageColor p_color = MessageColor.WHITE)
    {
        uiManager.CreateUIMessage(p_message, p_color);
    }
    public static void CreateDefautMessage(DefaultMessageType p_type)
    {
        uiManager.CreateDefautUIMessage(p_type, new List<string>());
    }
    public static void CreateDefautMessage(DefaultMessageType p_type, List<string> p_params)
    {
        uiManager.CreateDefautUIMessage(p_type, p_params);
    }
    private void ClearUIMessages()
    {
        for (int i = 0; i < texts.Count; i++)
            Destroy(texts[i].gameObject);
        texts.Clear();
    }
    private void CreateUIMessage(string p_message, MessageColor p_color)
    {
        GameObject __go = Instantiate(uiMessagePrefab);
        __go.transform.SetParent(messagesContainer.transform);

        Text __text = __go.GetComponent<Text>();
        __text.text = p_message;
        __text.color = Util.MessageColorToRGB(p_color);
        texts.Add(__text);

        inputField.ActivateInputField();
        StartCoroutine(VerticalScrollBarDelay(true));
    }
    private void CreateDefautUIMessage(DefaultMessageType p_type, List<string> p_params)
    {
        GameObject __go = Instantiate(uiDefaultMessagesPrefabs[(int)p_type]);
        __go.transform.SetParent(messagesContainer.transform);

        Text __text = __go.GetComponent<Text>();
        texts.Add(__text);
        bool __recalculate = false;

        switch(p_type)
        {
            case DefaultMessageType.MOVE_CMD_LOCKED_ROOM:
                __text.text = p_params[0];
                __recalculate = true;
                break;
            case DefaultMessageType.INVENTORY_CMD_NO_PLAYER_IN_GAME:
            case DefaultMessageType.INVENTORY_CMD_NO_PLAYER_IN_ROOM:
            case DefaultMessageType.INVENTORY_CMD_SHOW_INVENTORY:
            case DefaultMessageType.GET_CMD_GET_ITEM:
            case DefaultMessageType.DROP_CMD_DROP_ITEM:
            case DefaultMessageType.ITEM_NOT_FOUND:
                __text.text += string.Join(string.Empty, p_params.ToArray());
                __recalculate = true;
                break;
        }
        inputField.ActivateInputField();
        StartCoroutine(VerticalScrollBarDelay(__recalculate));
    }
    IEnumerator VerticalScrollBarDelay(bool p_recalculateSize)
    {
        yield return new WaitForSeconds(0.15f);
        if (p_recalculateSize)
        {
            texts[texts.Count - 1].GetComponent<LayoutElement>().preferredHeight *=
                texts[texts.Count - 1].cachedTextGenerator.lines.Count;
            verticalScrollBar.value = 0f;
        }
        else
            verticalScrollBar.value = 0f;
    }
}

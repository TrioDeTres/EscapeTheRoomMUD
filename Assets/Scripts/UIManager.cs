using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DefaultMessageType
{
    HELP_CMD_HELP_TEXT,
    WRONG_DIRECTION,
    SHOW_INVENTORY,
    GET_ITEM,
    DROP_ITEM,
    ITEM_NOT_FOUND
}
public enum MessageColor
{
    WHITE,
    RED,
    LIGHT_BLUE,
    BLUE,
    YELLOW,
    GREEN
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
        inputField.ActivateInputField();
        if (Input.GetKeyDown(KeyCode.Return))
            ExecuteMassage();
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
        inputField.ActivateInputField();
        StartCoroutine(VerticalScrollBarDelay(null, false));
    }
    private void CreateUIMessage(string p_message, MessageColor p_color)
    {
        GameObject __go = Instantiate(uiMessagePrefab);
        __go.transform.SetParent(messagesContainer.transform);
        __go.transform.localScale = Vector3.one;

        Text __text = __go.GetComponent<Text>();
        __text.text = p_message;
        __text.color = Util.MessageColorToRGB(p_color);
        texts.Add(__text);

        inputField.ActivateInputField();
        StartCoroutine(VerticalScrollBarDelay(__text, true));
    }
    private void CreateDefautUIMessage(DefaultMessageType p_type, List<string> p_params)
    {
        GameObject __go = Instantiate(uiDefaultMessagesPrefabs[(int)p_type]);
        __go.transform.SetParent(messagesContainer.transform);
        __go.transform.localScale = Vector3.one;

        Text __text = __go.GetComponent<Text>();
        texts.Add(__text);
        bool __recalculate = false;

        switch(p_type)
        {
            case DefaultMessageType.SHOW_INVENTORY:
            case DefaultMessageType.GET_ITEM:
            case DefaultMessageType.DROP_ITEM:
            case DefaultMessageType.ITEM_NOT_FOUND:
                __text.text += string.Join(string.Empty, p_params.ToArray());
                __recalculate = true;
                break;
        }
        inputField.ActivateInputField();
        StartCoroutine(VerticalScrollBarDelay(__text, __recalculate));
    }
    IEnumerator VerticalScrollBarDelay(Text p_text, bool p_recalculateSize)
    {
        yield return new WaitForSeconds(0.15f);

        if (p_recalculateSize)
            p_text.GetComponent<LayoutElement>().preferredHeight *=
                p_text.cachedTextGenerator.lines.Count;

        yield return new WaitForSeconds(0.075f);
        verticalScrollBar.value = 0f;
    }
}

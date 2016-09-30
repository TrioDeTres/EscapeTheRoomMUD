using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public List<Item> items;

    void Awake()
    {
        for (int i = 0; i < items.Count; i++)
            items[i].itemID = i;
    }
    public bool HasItem(string p_itemName)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == p_itemName)
                return true;
        return false;
    }
    public Item GetItem(string p_itemName)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == p_itemName)
                return items[i];
        return null;
    }
    public Item GetItem(int p_itemID)
    {
        if (items.Count <= p_itemID)
            return null;
        return items[p_itemID];
    }

    public string GetMessageWhenItemCantBePicked()
    {
        return "Server says: This item can't be picked up.";
    }
    public string GetMessageWhenItemCantBeUsed()
    {
        return "Server says: This item can't be used.";
    }
    public string GetMessageWhenItemCantBeTransfered()
    {
        return "Server says: This item can't be transferred.";
    }
    public string GetMessageWhenSomeonePickedItem(string p_playerName, string p_itemName)
    {
        return "Server says: " + p_playerName + " picked up the " + p_itemName + ".";
    }
    public string GetMessageWhenSomeoneDroppedItem(string p_playerName, string p_itemName)
    {
        return "Server says: " + p_playerName + " dropped the " + p_itemName + ".";
    }
    public string GetMessageWhenSomeoneUsedItem(string p_playerName, string p_sourceName, string p_targetName)
    {
        return "Server says: " + p_playerName + " used the " + p_sourceName + " on the " + p_targetName + ".";
    }
    public string GetMessageWhenSomeoneSendItem(string p_playerName, string p_sourceName)
    {
        return "Server says: " + p_playerName + " send the " + p_sourceName + " through the hole.";
    }
    public string GetMessageWhenSomeoneUsedSuitcase(string p_playerName)
    {
        return "Server says: " + p_playerName + " opened the suitcase.";
    }
    public string GetMessageWhenItemNotFound()
    {
        return "Server says: Item not found.";
    }
    public string GetMessageWhenNoInteraction()
    {
        return "Server says: These items can't be used together.";
    }
    public string GetMessageWhenInteractionNotActive()
    {
        return "Server says: Nothing happened.";
    }
    public string GetMessageWhenWrondPassword()
    {
        return "Server says: This is the wrong password.";
    }
    public string GetMessageWhenInvalidPassword()
    {
        return "Server says: This is an invalid password.";
    }
}

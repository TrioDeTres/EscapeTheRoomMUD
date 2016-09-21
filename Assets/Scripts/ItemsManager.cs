using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public event Action<string, MessageColor> OnSendMessage;
    public event Action<DefaultMessageType, List<string>> OnMessageError;

    public RoomsManager roomsManager;
    public List<Item> items;

    public void TryToLookItem(string p_itemName)
    {
        Item item = items.FirstOrDefault(i => string.Equals(i.itemName.ToLower(), p_itemName));

        if (item == null)
            OnMessageError(DefaultMessageType.ITEM_NOT_FOUND, new List<string>() {p_itemName, ". Please try again."});
        else
            OnSendMessage(item.itemDescription, MessageColor.WHITE);
    }
}

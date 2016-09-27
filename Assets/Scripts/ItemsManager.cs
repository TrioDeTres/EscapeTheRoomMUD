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
}

using System.Collections.Generic;
using UnityEngine;[System.Serializable]
public class Item : MonoBehaviour
{
    public string               itemName;
    public int                  itemID;
    [TextArea(5,10)] public string    itemDescription;

    public List<ItemInteraction> interactions;

    public bool isPickable;
    public bool isUsable;
    public bool isTransferable;

    public bool HasInteractionWithItem(Item p_source)
    {
        for (int i = 0; i < interactions.Count; i++)
            if (interactions[i].source == p_source)
                return true;
        return false;
    }
    public ItemInteraction GetInteraction(Item p_source)
    {
        for (int i = 0; i < interactions.Count; i++)
            if (interactions[i].source == p_source)
                return interactions[i];
        return null;
    }

    public string GetFullName()
    {
        if (gameObject.name.StartsWith("Item_"))
            return gameObject.name.Remove(0, 5);
        return gameObject.name;
    }
}

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
}

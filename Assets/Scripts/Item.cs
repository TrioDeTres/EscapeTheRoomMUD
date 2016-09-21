using System.Collections.Generic;
using UnityEngine;[System.Serializable]
public class Item : MonoBehaviour
{
    public string       itemName;
    public string       itemDescription;
    public int          itemID;
    public List<Room>   locations;

    public bool         isUsable;
    public bool         isPickable;
    public bool         isTransferable;
}

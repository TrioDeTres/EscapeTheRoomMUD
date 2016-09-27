using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInteraction
{
    public bool active = true;
    public Item source;
    [TextArea(4, 10)] public string messageWhenActivated;

    public List<InteractionChangeItemDescription>       changeItemDescriptions;
    public List<InteractionMoveItem>                    moveItems;
    public List<InteractionChangeRoomDescription>       changeRoomDescriptions;
    public List<InteractionChangeRoomTransitionLocked>  changeRoomTransition;
}
[System.Serializable]
public class InteractionChangeItemDescription
{
    public Item target;
    [TextArea (4,10)] public string newDescription;
}
[System.Serializable]
public class InteractionChangeRoomDescription
{
    public Room target;
    [TextArea(4, 10)] public string newDescription;
}
[System.Serializable]
public class InteractionMoveItem
{
    public Item target;
    public Room oldRoom;
    public Room newRoom;
}
[System.Serializable]
public class InteractionChangeRoomTransitionLocked
{
    public Room target;
    public CardinalPoint direction;
    public bool isLocked;
}
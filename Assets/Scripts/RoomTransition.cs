using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomTransition
{
    public Room     targetRoom;
    public bool     isLocked;
    [TextArea] public string   messageWhenLocked;
}

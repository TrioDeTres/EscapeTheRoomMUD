using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Util
{
    public static Color MessageColorToRGB(MessageColor p_color)
    {
        if (p_color == MessageColor.WHITE)
            return Color.white;
        else if (p_color == MessageColor.LIGHT_BLUE)
            return new Color(100f/255f, 220f / 255f, 1f);
        else if (p_color == MessageColor.BLUE)
            return new Color(0.25f, 0.25f, 1f);
        else if (p_color == MessageColor.RED)
            return new Color(1f, 0.15f, 0.15f);
        else if (p_color == MessageColor.YELLOW)
            return new Color(1f, 1f, 0.2f);
        return Color.white;
    }

    public static bool IsAddresValidIPV4(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return false;
        }

        string[] splitValues = address.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte parsing;

        return splitValues.All(r => byte.TryParse(r, out parsing));
    }

}

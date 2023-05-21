using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T[] SliceRow<T>(this T[,] array, int row)
    {
        T[] sliced = new T[array.GetLength(1)];

        for (var i = 0; i < array.GetLength(1); i++)
        {
            sliced[i] = array[row, i];
        }

        return sliced;
    }

    public static SlotType OtherType(SlotType type)
    {
        return (type == SlotType.One ? SlotType.Two : SlotType.One);
    }

    public static Color SetAlpha(this Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }
}

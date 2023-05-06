using System.Collections;
using UnityEngine;

public class Colors
{
    public enum ColorNames
    {
        Red,
        Green,
        Blue,
        Yellow,
        Orange,
        Pink,
        Purple,
        Lime,
        Teal,
        Aqua,
        Olive,
        Maroon,
        Fuchsia,
        Cyan,
        LightGreen,
        LightBlue,
        LightPink,
        Mustard,
        Goldenrod,
        Brown,
        Beige
    }

    public static Hashtable colorValues = new Hashtable()
    {
            { ColorNames.Red,       new Color32(255, 0, 0, 255) },
            { ColorNames.Green,     new Color32(0, 255, 0, 255) },
            { ColorNames.Blue,      new Color32(0, 0, 255, 255) },
            { ColorNames.Yellow,    new Color32(255, 255, 0, 255) },
            { ColorNames.Orange,    new Color32(255, 128, 0, 255) },
            { ColorNames.Pink,      new Color32(255, 102, 178, 255) },
            { ColorNames.Purple,    new Color32(128, 0, 255, 255) },
            { ColorNames.Lime,      new Color32(166, 254, 0, 255) },
            { ColorNames.Teal,      new Color32(0, 128, 128, 255) },
            { ColorNames.Aqua,      new Color32(0, 255, 255, 255) },
            { ColorNames.Olive,     new Color32(128, 128, 0, 255) },
            { ColorNames.Maroon,    new Color32(128, 0, 0, 255) },
            { ColorNames.Fuchsia,   new Color32(255, 0, 255, 255) },
            { ColorNames.Cyan,      new Color32(0, 255, 255, 255) },
            { ColorNames.LightGreen,new Color32(128, 255, 128, 255) },
            { ColorNames.LightBlue, new Color32(128, 128, 255, 255) },
            { ColorNames.LightPink, new Color32(255, 204, 204, 255) },
            { ColorNames.Mustard,   new Color32(255, 219, 88, 255) },
            { ColorNames.Goldenrod, new Color32(218, 165, 32, 255) },
            { ColorNames.Brown,     new Color32(165, 42, 42, 255) },
            { ColorNames.Beige,     new Color32(245, 245, 220, 255) }
    };

    public static Color32 ColorValue(ColorNames color)
    {
        return (Color32)colorValues[color];
    }
}

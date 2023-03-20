using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample
{
    public string name;
    public string abreviation;
    public string colorName;
    public Color color;

    public Sample(string name, string abreviation, string colorName, Color color)
    {
        this.name = name;
        this.abreviation = abreviation;
        this.colorName = colorName;
        this.color = color;
    }
}

using UnityEngine;

public class Sample
{
    public string sampleName;
    public string abreviation;
    public string colorName;
    public Color color;

    public Sample(string name, string abreviation, string colorName, Color color)
    {
        this.sampleName = name;
        this.abreviation = abreviation;
        this.colorName = colorName;
        this.color = color;
    }
}

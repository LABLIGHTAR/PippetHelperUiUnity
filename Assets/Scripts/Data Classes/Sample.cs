using UnityEngine;

public class Sample
{
    public string sampleName;
    public string abreviation;
    public string colorName;
    public Color color;
    public string vesselType;

    public Sample(string name, string abreviation, string colorName, Color color, string vesselType)
    {
        this.sampleName = name;
        this.abreviation = abreviation;
        this.colorName = colorName;
        this.color = color;
        this.vesselType = vesselType;
    }

    public override bool Equals(object s)
    {
        if (s is null)
        {
            return false;
        }

        Sample other = s as Sample;

        // Optimization for a common success case.
        if (Object.ReferenceEquals(this, other))
        {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != other.GetType())
        {
            return false;
        }

        return (this.sampleName == other.sampleName && this.abreviation == other.abreviation && this.colorName == other.colorName && this.color == other.color);
    }
}

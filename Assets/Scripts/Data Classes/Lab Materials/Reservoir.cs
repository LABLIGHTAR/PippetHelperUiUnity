using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reservoir : LabMaterial
{
    public Sample sample;

    public Reservoir(int id, string name, Sample newSample) : base(id, name)
    {
        sample = newSample;
    }

    public override string GetNameAsSource(string subID)
    {
        return sample.sampleName;
    }

    public override string GetNameAsTarget(string subID)
    {
        return "reservoir " + id;
    }
}

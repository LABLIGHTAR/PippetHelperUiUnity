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

    public override bool ContainsSample(Sample sampleIn)
    {
        if(sample == sampleIn)
        {
            return true;
        }
        return false;
    }

    public override List<Sample> GetSampleList()
    {
        List<Sample> list = new List<Sample>();
        list.Add(sample);
        return list;
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

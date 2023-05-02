using System.Collections;
using System.Collections.Generic;

public class Reservoir : LabMaterial
{
    public Sample sample;

    public Reservoir(int id, string name) : base(id, name)
    {

    }

    public override void AddNewSample(Sample sampleIn)
    {
        sample = sampleIn;
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

    public override string GetSampleID(Sample sample)
    {
        return "0";
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

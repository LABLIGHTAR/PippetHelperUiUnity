using System.Collections.Generic;
using System.Linq;

public class TubeRack2mL : LabMaterial
{
    public Dictionary<string, Sample> tubes;

    private int subId;

    public TubeRack2mL(int id, string name) : base(id, name)
    {
        tubes = new Dictionary<string, Sample>();
    }

    public override bool ContainsSample(Sample sample)
    {
        if (tubes.ContainsValue(sample))
        {
            return true;
        }
        return false;
    }

    public override string GetSampleID(Sample sample)
    {
        foreach(var item in tubes)
        {
            if(item.Value == sample)
                return item.Key;
        }
        return "";
    }

    public override List<Sample> GetSampleList()
    {
        return tubes.Values.ToList();
    }

    public override void AddNewSample(Sample sample)
    {
        tubes.Add(subId.ToString(), sample);
        subId++;
    }

    public override void RemoveSample(string id)
    {
        tubes.Remove(id);
    }

    public override bool HasSampleSlot()
    {
        var samples = GetSampleList();
        if(samples.Count < 24)
        {
            return true;
        }
        return false;
    }

    public override Dictionary<string, Sample> GetTubes()
    {
        return tubes;
    }

    public override string GetNameAsSource(string matSubID)
    {
        return tubes[matSubID].sampleName;
    }

    public override string GetNameAsTarget(string matSubID)
    {
        return "rack " + id + " tube " + matSubID;
    }
}

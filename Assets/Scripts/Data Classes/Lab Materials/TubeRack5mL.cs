using System.Collections.Generic;

public class TubeRack5mL : LabMaterial
{
    public Dictionary<string, Sample> tubes;

    private int subId;

    public TubeRack5mL(int id, string name) : base(id, name)
    {
        tubes = new Dictionary<string, Sample>();
    }

    public override void AddNewTube(Sample sample)
    {
        tubes.Add(subId.ToString(), sample);
        subId++;
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

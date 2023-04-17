using System.Collections.Generic;

public class Wellplate : LabMaterial
{
    public int numWells;
    public Dictionary<string, Well> wells;

    public Wellplate(int id, string name, int numberOfWells) : base(id, name)
    {
        wells = new Dictionary<string, Well>();
        numWells = numberOfWells;
    }

    public override bool ContainsWell(string wellID)
    {
        return wells.ContainsKey(wellID);
    }

    public override void AddWell(string wellID, Well newWell)
    {
        wells.Add(wellID, newWell);
    }

    public override Well GetWell(string wellID)
    {
        if(!ContainsWell(wellID))
        {
            AddWell(wellID, new Well(wellID, SessionState.CurrentStep.materials.IndexOf(this)));
        }
        return wells[wellID];
    }

    public override Dictionary<string, Well> GetWells()
    {
        return wells;
    }

    public override string GetNameAsSource(string subID)
    {
        return "well " + subID.ToString();
    }

    public override string GetNameAsTarget(string subID)
    {
        return "well " + subID.ToString();
    }
}

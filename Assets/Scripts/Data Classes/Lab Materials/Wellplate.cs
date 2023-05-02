using System.Collections.Generic;
using System.Linq;

public class Wellplate : LabMaterial
{
    public int numWells;
    public Dictionary<string, Well> wells;

    public Wellplate(int id, string name, int numberOfWells, string customName) : base(id, name)
    {
        wells = new Dictionary<string, Well>();
        numWells = numberOfWells;
        this.customName = customName;
    }

    public override bool SetCustomName(string name)
    {
        foreach(LabMaterial material in SessionState.Materials.Where(m => m is Wellplate))
        {
            if (material.customName == name)
                return false;
        }

        customName = name;
        return true;
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

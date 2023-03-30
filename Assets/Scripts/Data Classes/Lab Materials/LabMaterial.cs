using System.Collections.Generic;

public class LabMaterial
{
    public int id;
    public string materialName;

    public LabMaterial(int materialId, string name)
    {
        id = materialId;
        materialName = name;
    }

    public virtual bool ContainsWell(string wellID)
    {
        return false;
    }

    public virtual void AddWell(string wellID, Well newWell)
    {
        return;
    }

    public virtual Well GetWell(string wellID)
    {
        return null;
    }

    public virtual Dictionary<string, Well> GetWells()
    {
        return null;
    }

    public virtual bool ContainsSample(Sample sample)
    {
        return false;
    }

    public virtual List<Sample> GetSampleList()
    {
        return null;
    }

    public virtual void AddNewSample(Sample sample)
    {
        return;
    }

    public virtual bool HasSampleSlot()
    {
        return false;
    }

    public virtual Dictionary<string, Sample> GetTubes()
    {
        return null;
    }

    public virtual string GetNameAsSource(string subID)
    {
        return null;
    }

    public virtual string GetNameAsTarget(string subID) 
    {
        return null;
    }
}

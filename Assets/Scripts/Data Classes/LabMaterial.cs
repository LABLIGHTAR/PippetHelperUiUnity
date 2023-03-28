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
}

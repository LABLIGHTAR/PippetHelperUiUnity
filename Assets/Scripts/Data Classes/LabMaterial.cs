using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabMaterial
{
    public int id;
    public string materialName;
    public int numWells;

    public LabMaterial(int materialId, string name, int numberOfWells)
    {
        id = materialId;
        materialName = name;
        numWells = numberOfWells;
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

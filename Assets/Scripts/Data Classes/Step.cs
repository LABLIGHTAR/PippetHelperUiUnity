using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step
{
    public List<LabMaterial> materials;

    public Step()
    {
        materials = new List<LabMaterial>();

        foreach (LabMaterial material in SessionState.Materials)
        {
            if (material is Wellplate)
            {
                materials.Add(new Wellplate(material.id, material.materialName, material.numWells));
            }
        }
    }
}

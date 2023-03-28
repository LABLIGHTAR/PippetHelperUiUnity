
using System.Collections.Generic;

public class Step
{
    public List<LabMaterial> materials;

    public Step()
    {
        materials = new List<LabMaterial>();

        foreach (Wellplate material in SessionState.Materials)
        {
            if (material is Wellplate)
            {
                materials.Add(new Wellplate(material.id, material.materialName, material.numWells));
            }
        }
    }
}

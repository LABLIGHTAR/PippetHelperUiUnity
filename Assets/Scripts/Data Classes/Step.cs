
using System.Collections.Generic;
using UniRx;

public class Step
{
    public List<LabMaterial> materials;
    public List<LabAction> actions;

    public Step()
    {
        materials = new List<LabMaterial>();
        actions = new List<LabAction>();

        foreach (Wellplate material in SessionState.Materials)
        {
            if (material is Wellplate)
            {
                materials.Add(new Wellplate(material.id, material.materialName, material.numWells));
            }
        }
    }
}

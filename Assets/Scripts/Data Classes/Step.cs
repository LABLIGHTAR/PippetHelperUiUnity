
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

        foreach (var material in SessionState.Materials)
        {
            if (material is Wellplate)
            {
                var wellplate = (Wellplate)material;
                materials.Add(new Wellplate(wellplate.id, wellplate.materialName, wellplate.numWells));
            }
        }
    }
}

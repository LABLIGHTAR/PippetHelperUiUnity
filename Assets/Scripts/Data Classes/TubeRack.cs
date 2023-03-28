using System.Collections.Generic;

public class TubeRack : LabMaterial
{
    public Dictionary<int, Sample> tubes;

    public TubeRack(int id, string name) : base(id, name)
    {
        tubes = new Dictionary<int, Sample>();
    }
}

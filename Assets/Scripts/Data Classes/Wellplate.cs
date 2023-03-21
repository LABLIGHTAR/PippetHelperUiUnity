using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wellplate
{
    public int id;
    public int numWells;
    public Dictionary<string, Well> wells;

    public Wellplate(int id, int numWells)
    {
        this.id = id;
        this.numWells = numWells;
        wells = new Dictionary<string, Well>();
    }
}

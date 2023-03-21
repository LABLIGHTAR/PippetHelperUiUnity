using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step
{
    public List<Wellplate> plates;

    public Step()
    {
        plates = new List<Wellplate>();

        foreach (Wellplate material in SessionState.Materials)
        {
            plates.Add(new Wellplate(material.id, material.numWells));
        }
    }
}

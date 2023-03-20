using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wellplate
{
    public Dictionary<string, Well> wells;

    public Wellplate()
    {
        wells = new Dictionary<string, Well>();
    }
}

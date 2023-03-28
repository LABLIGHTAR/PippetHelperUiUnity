using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabAction
{
    public enum ActionTypes
    { 
        Pipette,
        Transfer,
        Dilution
    }

    public ActionTypes type;

    public struct Source
    {
        public LabMaterial matID;
        public int matSubID;
        public Color color;
        public float volume;
        public string units;
    }

    public struct Target
    {
        public LabMaterial matID;
        public int matSubID;
        public Color color;
    }
}

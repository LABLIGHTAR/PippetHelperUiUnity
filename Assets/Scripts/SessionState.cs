using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;
using static SessionState;

public class SessionState : MonoBehaviour
{
    public static SessionState Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        //initalize state variables
        Steps = new List<WellPlate>();
        SetStep(0);
        Steps.Add(new WellPlate());
        AvailableSamples = new List<Sample>();
        UsedColors = new List<string>();
    }

    //class definitions
    public class Sample
    {
        public string name;
        public string abreviation;
        public string colorName;
        public Color color;

        public Sample(string name, string abreviation, string colorName, Color color)
        {
            this.name = name;
            this.abreviation = abreviation;
            this.colorName = colorName;
            this.color = color;
        }
    }

    public class Well
    {
        public struct SampleGroup
        {
            public int groupId;
            public bool isStart;
            public bool isEnd;
            public Sample Sample;

            public SampleGroup(int groupId, bool isStart, bool isEnd, Sample Sample)
            {
                this.groupId = groupId;
                this.isStart = isStart;
                this.isEnd = isEnd;
                this.Sample = Sample;
            }
        }

        public Dictionary<Sample, float> Samples;
        public List<SampleGroup> groups;

        public Well()
        {
            Samples = new Dictionary<Sample, float>();
            groups = new List<SampleGroup>();
        }
    }

    public class WellPlate
    {
        public Dictionary<string, Well> wells;

        public WellPlate()
        {
            wells = new Dictionary<string, Well>();
        }
    }

    public class Colors
    {
        public enum ColorNames
        {
            Lime,
            Green,
            Olive,
            Brown,
            Aqua,
            Blue,
            Navy,
            Slate,
            Purple,
            Plum,
            Pink,
            Salmon,
            Red,
            Orange,
            Yellow,
            Khaki
        }

        private static Hashtable colorValues = new Hashtable{
             {  ColorNames.Lime,    new Color32( 166 , 254 , 0, 255 ) },
             {  ColorNames.Green,   new Color32( 0 , 254 , 111, 255 ) },
             {  ColorNames.Olive,   new Color32( 85, 107, 47, 255 ) },
             {  ColorNames.Brown,   new Color32( 139, 69, 19, 255 ) },
             {  ColorNames.Aqua,    new Color32( 0 , 201 , 254, 255 ) },
             {  ColorNames.Blue,    new Color32( 0 , 122 , 254, 255 ) },
             {  ColorNames.Navy,    new Color32( 60 , 0 , 254, 255 ) },
             {  ColorNames.Slate,   new Color32( 72, 61, 139, 255 ) },
             {  ColorNames.Purple,  new Color32( 143 , 0 , 254, 255 ) },
             {  ColorNames.Plum,    new Color32( 221, 160, 221, 255 ) },
             {  ColorNames.Pink,    new Color32( 232 , 0 , 254, 255 ) },
             {  ColorNames.Salmon,  new Color32( 255, 160, 122, 255 ) },
             {  ColorNames.Red,     new Color32( 254 , 9 , 0, 255 ) },
             {  ColorNames.Orange,  new Color32( 254 , 161 , 0, 255 ) },
             {  ColorNames.Yellow,  new Color32( 254 , 224 , 0, 255 ) },
             {  ColorNames.Khaki,   new Color32( 240,230,140, 255 ) },
        };

        public static Color32 ColorValue(ColorNames color)
        {
            return (Color32)colorValues[color];
        }
    }

    public class Tool
    {
        public string name;
        public int numChannels;
        public string orientation;
        public float volume;

        public Tool(string name, int numChannels, string orientation, float volume)
        {
            this.name = name;
            this.numChannels = numChannels;
            this.orientation = orientation;
            this.volume = volume;
        }

        public void SetVolume(float value)
        {
            if(volume != value)
            {
                volume = value;
            }
        }
    }

    //state variables
    private static string procedureName;

    private static List<WellPlate> steps;
    private static int step;

    private static List<Sample> availableSamples;
    private static Sample activeSample;

    private static bool formActive;

    private static List<string> usedColors;

    private static Tool activeTool;

    private static Well focusedWell;

    private static int groupId;

    //data streams
    public static Subject<int> stepStream = new Subject<int>();
    public static Subject<int> newStepStream = new Subject<int>();
    public static Subject<Sample> activeSampleStream = new Subject<Sample>();
    public static Subject<Sample> newSampleStream = new Subject<Sample>();
    public static Subject<(string, string)> editedSampleStream = new Subject<(string,string)>();
    public static Subject<string> SampleRemovedStream = new Subject<string>();
    public static Subject<Well> focusedWellStream = new Subject<Well>();
    public static Subject<string> procedureNameStream = new Subject<string>();

    //setters
    public static string ProcedureName
    {
        set
        {
            if(procedureName != value)
            {
                procedureName = value;
                procedureNameStream.OnNext(value);
            }
        }
        get
        {
            return procedureName;
        }
    }

    public static List<WellPlate> Steps
    {
        set
        {
            if (steps != value)
            {
                steps = value;
            }
        }
        get
        {
            return steps;
        }
    }

    public static int Step
    {
        set
        {
            if (step != value)
            {
                step = value;
            }
            stepStream.OnNext(step);
        }
        get
        {
            return step;
        }
    }

    public static List<Sample> AvailableSamples
    {
        set
        {
            if (availableSamples != value)
            {
                availableSamples = value;
            }
        }
        get
        {
            return availableSamples;
        }
    }

    public static Sample ActiveSample
    {
        set
        {
            if (activeSample != value)
            {
                activeSample = value;
            }
            activeSampleStream.OnNext(activeSample);
        }
        get
        {
            return activeSample;
        }
    }

    public static Well FocusedWell
    {
        set
        {
            if (focusedWell != value)
            {
                focusedWell = value;
            }
            focusedWellStream.OnNext(focusedWell);
        }
        get
        {
            return focusedWell;
        }
    }

    public static bool FormActive
    {
        set
        {
            if (formActive != value)
            {
                formActive = value;
            }
        }
        get
        {
            return formActive;
        }
    }

    public static List<string> UsedColors
    {
        set
        {
            if (usedColors != value)
            {
                usedColors = value;
            }
        }
        get
        {
            return usedColors;
        }
    }

    public static Tool ActiveTool
    {
        set
        {
            if (activeTool != value)
            {
                activeTool = value;
            }
        }
        get
        {
            return activeTool;
        }
    }

    public static int GroupId
    {
        set
        {
            if(groupId != value)
            {
                groupId = value;
            }
        }
        get 
        { 
            return groupId; 
        }
    }

    public static void SetStep(int value)
    {
        if(value < 0 || value > Steps.Count)
        {
            return;
        }
        else
        {
            Step = value;
        }
    }

    public static void AddNewStep()
    {
        Steps.Add(new WellPlate());
        Step = Steps.Count - 1;
        newStepStream.OnNext(SessionState.Step);
    }

    public static void RemoveCurrentStep()
    {
        Steps.Remove(Steps[Step]);
    }

    //adds new Sample to the available Samples list
    public static void AddNewSample(string name, string abreviation, string colorName, Color color)
    {
        Sample newSample = new Sample(name, abreviation, colorName, color);
        
        //return if the Sample already exists
        if (AvailableSamples.Exists(x => x.name == name || x.abreviation == abreviation || x.colorName == colorName || x.color == color))
        {
            Debug.LogWarning("Sample already exists");
            return;
        }
        else
        {
            UsedColors.Add(colorName);
            AvailableSamples.Add(newSample);
            newSampleStream.OnNext(newSample);
        }
    }

    //removes sample from available samples list
    public static void RemoveSample(string name)
    {
        Sample forRemoval = AvailableSamples.Where(sample => sample.name == name).FirstOrDefault();

        if (forRemoval != null)
        {
            //set the sample for removal to the active sample
            ActiveSample = forRemoval;

            //remove this sample from all wells
            foreach (var step in Steps)
            {
                foreach (var well in step.wells)
                {
                    if (well.Value.Samples.ContainsKey(forRemoval))
                    {
                        RemoveActiveSampleFromWell(well.Key, step);
                    }
                }
            }
 
            ActiveSample = null;
            //return this samples color to the available colors
            UsedColors.Remove(forRemoval.colorName);
            //remove this sample from the sample list
            AvailableSamples.Remove(forRemoval);
        }
    }

    public static void EditSample(string oldName, string newName, string newAbreviation, string newColorName, Color newColor)
    {
        Sample toEdit = AvailableSamples.Where(sample => sample.name == oldName).FirstOrDefault();
        if (toEdit != null)
        {
            toEdit.name = newName;
            toEdit.abreviation = newAbreviation;
            if(newColor != toEdit.color)
            {
                UsedColors.Remove(toEdit.colorName);
                toEdit.colorName = newColorName;
                toEdit.color = newColor;
                UsedColors.Add(newColorName);
            }
            editedSampleStream.OnNext((oldName, newName));
        }
    }

    public static void SetActiveSample(Sample selected)
    {
        if (!AvailableSamples.Contains(selected))
        {
            Debug.LogWarning("Selected Sample is not available");
            return;
        }
        else
        {
            ActiveSample = selected;
        }
    }

    public static void SetFocusedWell(string wellId)
    {
        if (!Steps[Step].wells.ContainsKey(wellId))
        {
            //if the well does not exist create it
            Steps[Step].wells.Add(wellId, new Well());
            FocusedWell = Steps[Step].wells[wellId];
        }
        else
        {
            FocusedWell = Steps[Step].wells[wellId];
        }
    }

    public static bool AddActiveSampleToWell(string wellName, bool inGroup, bool isStart, bool isEnd)
    {
        if(Steps[Step].wells.ContainsKey(wellName))
        {
            if(!Steps[Step].wells[wellName].Samples.ContainsKey(ActiveSample))
            {
                //if the well exists and does not already have the active Sample add it
                Steps[Step].wells[wellName].Samples.Add(ActiveSample, ActiveTool.volume);
                //if this Sample is grouped add it to the group list
                if(inGroup)
                {
                    Steps[Step].wells[wellName].groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
                    //if this is the last well in the group increment the group id for the next group
                    if(isEnd)
                    {
                        GroupId++; 
                    }
                }
                return true;
            }
            else
            {
                Debug.LogWarning("Well already contains the active Sample");
                return false;
            }
        }
        else
        {
            //if the well does not exist create it
            Steps[Step].wells.Add(wellName, new Well());
            //add the active Sample to the new well
            Steps[Step].wells[wellName].Samples.Add(ActiveSample, ActiveTool.volume);
            //if this Sample is grouped add it to the group list
            if (inGroup)
            {
                Steps[Step].wells[wellName].groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
                //if this is the last well in the group increment the group id for the next group
                if (isEnd)
                {
                    GroupId++;
                }
            }
            return true;
        }
    }

    public static bool RemoveActiveSampleFromWell(string wellName, WellPlate removalStep)
    {
        if (removalStep.wells.ContainsKey(wellName))
        {
            if (removalStep.wells[wellName].Samples.ContainsKey(ActiveSample))
            {
                //if the well exists and already has the active Sample remove it
                removalStep.wells[wellName].Samples.Remove(ActiveSample);
                SampleRemovedStream.OnNext(wellName);

                //if the Sample being removed is part of a group remove the group everywhere
                if (removalStep.wells[wellName].groups != null)
                {
                    foreach(Well.SampleGroup group in removalStep.wells[wellName].groups)
                    {
                        if(group.Sample == ActiveSample)
                        {
                            int IdForRemoval = group.groupId;
                            //go through each well and remove all Samples in this group
                            RemoveAllSamplesInGroup(IdForRemoval);
                            //break since the active Sample cannot be in a well mroe than once
                            break;
                        }
                    }
                }
                return true;
            }
            else
            {
                Debug.LogWarning("Well does not contain the active Sample");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Well is empty");
            return false;
        }
    }

    static void RemoveAllSamplesInGroup(int removalID)
    {
        List<Well.SampleGroup> groupsToRemove = new List<Well.SampleGroup>();
        //iterate through all wells
        foreach (var well in Steps[Step].wells)
        {
            //iterate through each well group
            foreach(Well.SampleGroup group in well.Value.groups)
            {
                if(group.groupId == removalID)
                {
                    //add the group to a list for removal (cannot modify list in foreach loop)
                    groupsToRemove.Add(group);
                    //remove the active Sample
                    well.Value.Samples.Remove(ActiveSample);
                    //notify well
                    SampleRemovedStream.OnNext(well.Key);
                }
            }
            //remove groups
            well.Value.groups.RemoveAll(item => groupsToRemove.Contains(item));
            groupsToRemove.Clear();
        }
    }
}

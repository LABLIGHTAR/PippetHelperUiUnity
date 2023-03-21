using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;

public class SessionState : MonoBehaviour
{
    public static SessionState Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        //initalize state variables
        Materials = new List<Wellplate>();
        Steps = new List<Step>();
        AvailableSamples = new List<Sample>();
        UsedColors = new List<string>();
    }

    void Start()
    {
        MaterialViewController.materialsSelectedStream.Subscribe(_ => AddNewStep());
    }

    //state variables
    private static string procedureName;
    private static List<Wellplate> materials;

    private static List<Step> steps;
    private static int activeStep;

    private static List<Sample> availableSamples;
    private static Sample activeSample;

    private static bool formActive;
    private static bool selectionActive;

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

    //getters and setters
    #region
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

    public static List<Wellplate> Materials
    {
        set
        {
            if(materials != value)
            {
                materials = value;
            }
        }
        get
        {
            return materials;
        }
    }

    public static List<Step> Steps
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

    public static int ActiveStep
    {
        set
        {
            if (activeStep != value)
            {
                activeStep = value;
            }
            stepStream.OnNext(activeStep);
        }
        get
        {
            return activeStep;
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

    public static bool SelectionActive
    {
        set
        {
            if (selectionActive != value)
            {
                selectionActive = value;
            }
        }
        get
        {
            return selectionActive;
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

    public static void SetActiveStep(int value)
    {
        if(value < 0 || value > Steps.Count)
        {
            return;
        }
        else
        {
            ActiveStep = value;
        }
    }
    #endregion

    //adds new step to protocol and navigates ui to new step
    public static void AddNewStep()
    {
        Steps.Add(new Step());
        ActiveStep = Steps.Count - 1;
        newStepStream.OnNext(SessionState.ActiveStep);
    }

    //deletes the active step
    public static void RemoveCurrentStep()
    {
        Steps.Remove(Steps[ActiveStep]);
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

            //remove this sample from all wells in every plate
            foreach (var step in Steps)
            {
                foreach (var plate in step.plates)
                {
                    foreach (var well in plate.wells)
                    {
                        if (well.Value.Samples.ContainsKey(forRemoval))
                        {
                            RemoveActiveSampleFromWell(well.Key, well.Value.plateId, step);
                        }
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

    //edits a sample in the available sample list
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

    //sets the active sample
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

    //sets the focused well
    public static void SetFocusedWell(string wellId, int plateId)
    {
        if (!Steps[ActiveStep].plates[plateId].wells.ContainsKey(wellId))
        {
            //if the well does not exist create it
            Steps[ActiveStep].plates[plateId].wells.Add(wellId, new Well(wellId, plateId));
            FocusedWell = Steps[ActiveStep].plates[plateId].wells[wellId];
        }
        else
        {
            FocusedWell = Steps[ActiveStep].plates[plateId].wells[wellId];
        }
    }

    //adds active sample to passed well
    public static bool AddActiveSampleToWell(string wellName, int plateId, bool inGroup, bool isStart, bool isEnd)
    {
        if(Steps[ActiveStep].plates[plateId].wells.ContainsKey(wellName))
        {
            if(!Steps[ActiveStep].plates[plateId].wells[wellName].Samples.ContainsKey(ActiveSample))
            {
                //if the well exists and does not already have the active Sample add it
                Steps[ActiveStep].plates[plateId].wells[wellName].Samples.Add(ActiveSample, ActiveTool.volume);
                //if this Sample is grouped add it to the group list
                if(inGroup)
                {
                    Steps[ActiveStep].plates[plateId].wells[wellName].groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
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
            Steps[ActiveStep].plates[plateId].wells.Add(wellName, new Well(wellName, plateId));
            //add the active Sample to the new well
            Steps[ActiveStep].plates[plateId].wells[wellName].Samples.Add(ActiveSample, ActiveTool.volume);
            //if this Sample is grouped add it to the group list
            if (inGroup)
            {
                Steps[ActiveStep].plates[plateId].wells[wellName].groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
                //if this is the last well in the group increment the group id for the next group
                if (isEnd)
                {
                    GroupId++;
                }
            }
            return true;
        }
    }

    //removes active sample from passed well at passed step
    public static bool RemoveActiveSampleFromWell(string wellName, int plateId, Step removalStep)
    {
        if (removalStep.plates[plateId].wells.ContainsKey(wellName))
        {
            if (ActiveSample != null && removalStep.plates[plateId].wells[wellName].Samples.ContainsKey(ActiveSample))
            {
                //if the well exists and already has the active Sample remove it
                removalStep.plates[plateId].wells[wellName].Samples.Remove(ActiveSample);
                SampleRemovedStream.OnNext(wellName);

                //if the Sample being removed is part of a group remove the group everywhere
                if (removalStep.plates[plateId].wells[wellName].groups != null)
                {
                    foreach(Well.SampleGroup group in removalStep.plates[plateId].wells[wellName].groups)
                    {
                        if(group.Sample == ActiveSample)
                        {
                            int IdForRemoval = group.groupId;
                            //go through each well and remove all Samples in this group
                            RemoveAllSamplesInGroup(IdForRemoval, plateId);
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

    //removes all samples in the passed sample group id
    static void RemoveAllSamplesInGroup(int removalID, int plateId)
    {
        List<Well.SampleGroup> groupsToRemove = new List<Well.SampleGroup>();
        //iterate through all wells
        foreach (var well in Steps[ActiveStep].plates[plateId].wells)
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

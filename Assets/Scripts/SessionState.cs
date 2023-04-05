using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.UIElements;

public class SessionState : MonoBehaviour
{
    public static SessionState Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        //initalize state variables
        Materials = new List<LabMaterial>();
        Steps = new List<Step>();
        AvailableSamples = new List<Sample>();
        UsedColors = new List<string>();
    }

    void Start()
    {
        //after materials are chosen create step 1
        MaterialViewController.materialsSelectedStream.Subscribe(_ => AddNewStep());
    }

    //state variables
    private static string procedureName;
    private static List<LabMaterial> materials;

    private static List<Step> steps;
    private static int activeStep;
    private static Step currentStep;

    private static List<Sample> availableSamples;
    private static Sample activeSample;

    private static bool formActive;
    private static bool selectionActive;

    private static List<string> usedColors;

    private static Tool activeTool;
    private static LabAction.ActionType activeActionType;

    private static Well focusedWell;
    private static Well selectedWell;

    private static int groupId;

    //data streams
    public static Subject<string> procedureNameStream = new Subject<string>();
    public static Subject<int> stepStream = new Subject<int>();
    public static Subject<int> newStepStream = new Subject<int>();
    
    public static Subject<Sample> activeSampleStream = new Subject<Sample>();
    public static Subject<Sample> newSampleStream = new Subject<Sample>();
    public static Subject<(string, string)> editedSampleStream = new Subject<(string,string)>();
    public static Subject<string> SampleRemovedStream = new Subject<string>();
    
    public static Subject<Well> focusedWellStream = new Subject<Well>();
    public static Subject<Well> selectedWellStream = new Subject<Well>();


    public static Subject<LabAction> actionAddedStream = new Subject<LabAction>();
    public static Subject<LabAction> actionRemovedStream = new Subject<LabAction>();
    public static Subject<LabAction.ActionType> actionTypeStream = new Subject<LabAction.ActionType>();

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

    public static List<LabMaterial> Materials
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

    public static Step CurrentStep
    {
        get
        {
            return Steps[ActiveStep];
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
            List<Sample> sampleList = new List<Sample>();
            foreach(var material in Materials)
            {
                if(material.GetSampleList() != null)
                {
                    sampleList.AddRange(material.GetSampleList());
                }
            }
            return sampleList;
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

    public static Well SelectedWell
    {
        set
        {
            if (selectedWell != value)
            {
                selectedWell = value;
            }
            selectedWellStream.OnNext(selectedWell);
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

    public static LabAction.ActionType ActiveActionType
    {
        set
        {
            if (activeActionType != value)
            {
                activeActionType = value;
                actionTypeStream.OnNext(activeActionType);
            }
        }
        get
        {
            return activeActionType;
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
        newStepStream.OnNext(ActiveStep);
    }

    //deletes the active step
    public static void RemoveCurrentStep()
    {
        Steps.Remove(CurrentStep);
    }

    //adds new Sample to the available Samples list
    public static void AddNewSample(string name, string abreviation, string colorName, Color color, string vesselType)
    {
        Sample newSample = new Sample(name, abreviation, colorName, color);

        //return if the Sample already exists
        if (AvailableSamples.Contains(newSample))
        {
            Debug.LogWarning("Sample already exists");
            return;
        }

        UsedColors.Add(colorName);
        AddSampleToMaterialsList(newSample, vesselType);
        newSampleStream.OnNext(newSample);
    }

    static void AddSampleToMaterialsList(Sample newSample, string vesselType)
    {
        //add sample to materials list
        if (vesselType == "5mL Tube")
        {
            var tubeRack = Materials.Where(mat => mat is TubeRack5mL).FirstOrDefault();
            if (tubeRack != null && tubeRack.HasSampleSlot())
            {
                tubeRack.AddNewSample(newSample);
            }
            else
            {
                TubeRack5mL newRack = new TubeRack5mL(Materials.Count, "tuberack5ml");
                newRack.AddNewSample(newSample);
                Materials.Add(newRack);
            }
        }
        else if (vesselType == "Reservoir")
        {
            var reservoir = new Reservoir(Materials.Count, "reservoir");
            reservoir.AddNewSample(newSample);
            materials.Add(reservoir);
        }
    }

    //removes sample from available samples list
    public static void RemoveSample(string name)
    {
        Sample forRemoval = AvailableSamples.Where(sample => sample.sampleName == name).FirstOrDefault();

        if (forRemoval != null)
        {
            //set the sample for removal to the active sample
            ActiveSample = forRemoval;

            //remove this sample from all wells in every plate
            foreach (var step in Steps)
            {
                foreach (var plate in step.materials)
                {
                    foreach (var well in plate.GetWells())
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
            //remove this sample from the materials list
            RemoveSampleFromMateralsList(forRemoval);
        }
    }

    static void RemoveSampleFromMateralsList(Sample forRemoval)
    {
        //remove sample from materials list
        foreach (var material in Materials)
        {
            if (material is TubeRack5mL)
            {
                foreach (var tube in material.GetTubes())
                {
                    if (tube.Value == forRemoval)
                    {
                        material.GetTubes().Remove(tube.Key);
                        break;
                    }
                }
            }
            else if (material is Reservoir && material.ContainsSample(forRemoval))
            {
                Materials.Remove(material);
            }
        }
    }

    //edits a sample in the available sample list
    public static void EditSample(string oldName, string newName, string newAbreviation, string newColorName, Color newColor)
    {
        Sample toEdit = AvailableSamples.Where(sample => sample.sampleName == oldName).FirstOrDefault();
        if (toEdit != null)
        {
            toEdit.sampleName = newName;
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
        if(AvailableSamples.Contains(selected))
        {
            ActiveSample = selected;
            return;
        }
        Debug.LogWarning("Selected sample is not available");
    }

    //sets the focused well
    public static void SetFocusedWell(string wellId, int plateId)
    {
        if (!CurrentStep.materials[plateId].ContainsWell(wellId))
        {
            //if the well does not exist create it
            CurrentStep.materials[plateId].AddWell(wellId, new Well(wellId, plateId));
            FocusedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
        else
        {
            FocusedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
    }

    //sets the focused well
    public static void SetSelectedWell(string wellId, int plateId)
    {
        if (!CurrentStep.materials[plateId].ContainsWell(wellId))
        {
            //if the well does not exist create it
            CurrentStep.materials[plateId].AddWell(wellId, new Well(wellId, plateId));
            SelectedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
        else
        {
            SelectedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
    }

    //adds active sample to passed well
    public static bool AddActiveSampleToWell(string wellName, int plateId, bool inGroup, bool isStart, bool isEnd)
    {
        if(ActiveActionType == LabAction.ActionType.pipette)
        {
            if (CurrentStep.materials[plateId].ContainsWell(wellName))
            {
                if (!CurrentStep.materials[plateId].GetWell(wellName).Samples.ContainsKey(ActiveSample))
                {
                    //if the well exists and does not already have the active Sample add it
                    CurrentStep.materials[plateId].GetWell(wellName).Samples.Add(ActiveSample, ActiveTool.volume);
                    //if this Sample is grouped add it to the group list
                    if (inGroup)
                    {
                        CurrentStep.materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
                        //if this is the last well in the group increment the group id for the next group
                        if (isEnd)
                        {
                            //Add group action
                            string multichannelTargetID = "";
                            foreach (var well in CurrentStep.materials[plateId].GetWells())
                            {
                                if (well.Value.IsStartOfGroup(GroupId))
                                {
                                    multichannelTargetID = well.Value.id;
                                }
                            }
                            multichannelTargetID = multichannelTargetID + "-" + wellName;
                            AddPipetteAction(plateId.ToString(), multichannelTargetID);

                            GroupId++;
                        }
                    }
                    else
                    {
                        //add single action
                        AddPipetteAction(plateId.ToString(), wellName);
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
                CurrentStep.materials[plateId].AddWell(wellName, new Well(wellName, plateId));
                //add the active Sample to the new well
                CurrentStep.materials[plateId].GetWell(wellName).Samples.Add(ActiveSample, ActiveTool.volume);
                //if this Sample is grouped add it to the group list
                if (inGroup)
                {
                    CurrentStep.materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, ActiveSample));
                    //if this is the last well in the group increment the group id for the next group
                    if (isEnd)
                    {
                        //Add group action
                        string multichannelTargetID = "";
                        foreach (var well in CurrentStep.materials[plateId].GetWells())
                        {
                            if (well.Value.IsStartOfGroup(GroupId))
                            {
                                multichannelTargetID = well.Value.id;
                            }
                        }
                        multichannelTargetID = multichannelTargetID + "-" + wellName;
                        AddPipetteAction(plateId.ToString(), multichannelTargetID);

                        GroupId++;
                    }
                }
                else
                {
                    //add single action
                    AddPipetteAction(plateId.ToString(), wellName);
                }
                return true;
            }
        }
        return false;
    }

    //removes active sample from passed well at passed step
    public static bool RemoveActiveSampleFromWell(string wellName, int plateId, Step removalStep)
    {
        if(ActiveActionType == LabAction.ActionType.pipette)
        {
            if (removalStep.materials[plateId].ContainsWell(wellName))
            {
                if (ActiveSample != null && removalStep.materials[plateId].GetWell(wellName).Samples.ContainsKey(ActiveSample))
                {
                    //if the well exists and has the active Sample remove it
                    removalStep.materials[plateId].GetWell(wellName).Samples.Remove(ActiveSample);
                    //remove the associated action
                    LabAction removalAction = removalStep.actions.Where(a => a.source.color == ActiveSample.color && a.target.matID == plateId.ToString() && a.target.matSubID == wellName).FirstOrDefault();
                    if (removalAction != null)
                    {
                        removalStep.actions.Remove(removalAction);
                        actionRemovedStream.OnNext(removalAction);
                    }

                    SampleRemovedStream.OnNext(wellName);

                    //if the Sample being removed is part of a group remove the group everywhere
                    if (removalStep.materials[plateId].GetWell(wellName).groups != null)
                    {
                        foreach (Well.SampleGroup group in removalStep.materials[plateId].GetWell(wellName).groups)
                        {
                            if (group.Sample == ActiveSample)
                            {
                                int IdForRemoval = group.groupId;

                                //remove group action
                                string multichannelTargetID = "";
                                string groupStart = "";
                                string groupEnd = "";
                                foreach (var well in removalStep.materials[plateId].GetWells())
                                {
                                    if (well.Value.IsStartOfGroup(IdForRemoval))
                                    {
                                        groupStart = well.Value.id;
                                    }
                                    else if (well.Value.IsEndOfGroup(IdForRemoval))
                                    {
                                        groupEnd = well.Value.id;
                                    }
                                }
                                multichannelTargetID = groupStart + "-" + groupEnd;
                                Debug.Log(multichannelTargetID);
                                removalAction = removalStep.actions.Where(a => a.source.color == ActiveSample.color && a.target.matID == plateId.ToString() && a.target.matSubID == multichannelTargetID).FirstOrDefault();
                                if (removalAction != null)
                                {
                                    Debug.Log("Removing Action");
                                    removalStep.actions.Remove(removalAction);
                                    actionRemovedStream.OnNext(removalAction);
                                }

                                //go through each well and remove all Samples in this group
                                RemoveAllSamplesInGroup(IdForRemoval, plateId);
                                //break since the active Sample cannot be in a well more than once
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
        return false;
    }

    //removes all samples in the passed sample group id
    static void RemoveAllSamplesInGroup(int removalID, int plateId)
    {
        List<Well.SampleGroup> groupsToRemove = new List<Well.SampleGroup>();
        //iterate through all wells
        foreach (var well in CurrentStep.materials[plateId].GetWells())
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

    public static void AddActionToCurrentStep(LabAction.ActionType action, LabAction.Source source, LabAction.Target target)
    {
        var newAction = new LabAction(action, source, target);
        CurrentStep.actions.Add(newAction);
        actionAddedStream.OnNext(newAction);
    }

    static void AddPipetteAction(string plateID, string wellID)
    {
        //add action to session state
        string sourceID = "";
        string sourceSubID = "";
        foreach (var material in Materials)
        {
            var samples = material.GetSampleList();
            if (samples != null)
            {
                foreach (var sample in samples)
                {
                    if (sample == ActiveSample)
                    {
                        sourceID = material.id.ToString();
                        sourceSubID = samples.IndexOf(sample).ToString();
                        break;
                    }
                }
            }
        }
        var source = new LabAction.Source(sourceID, sourceSubID, ActiveSample.color, ActiveSample.colorName, ActiveTool.volume, "μL");
        var target = new LabAction.Target(plateID, wellID, ActiveSample.color, ActiveSample.colorName);
        AddActionToCurrentStep(LabAction.ActionType.pipette, source, target);
    }

    public static void AddTransferAction(Well sourceWell, Well targetWell, float volume)
    {
        var source = new LabAction.Source(sourceWell.plateId.ToString(), sourceWell.id, Color.red, "Red", volume, "μL");
        var target = new LabAction.Target(targetWell.plateId.ToString(), targetWell.id, Color.green, "Green");
        AddActionToCurrentStep(LabAction.ActionType.transfer, source, target);
    }

    public static void AddDilutionAction(Well sourceWell, Well targetWell, float dilutionFactor)
    {
        var source = new LabAction.Source(sourceWell.plateId.ToString(), sourceWell.id, Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetWell.plateId.ToString(), targetWell.id, Color.green, "Green");
        AddActionToCurrentStep(LabAction.ActionType.dilution, source, target);
    }

    public static void AddDilutionActionStart(Sample sourceSample, Well targetWell, float dilutionFactor)
    {
        var sourceMaterial = Materials.Where(material => material.ContainsSample(sourceSample)).FirstOrDefault();
        var sampleList = sourceMaterial.GetSampleList();
        var sampleID = sampleList.IndexOf(sourceSample);

        var source = new LabAction.Source(sourceMaterial.id.ToString(), sampleID.ToString(), Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetWell.plateId.ToString(), targetWell.id, Color.green, "Green");
        AddActionToCurrentStep(LabAction.ActionType.dilution, source, target);
    }
}

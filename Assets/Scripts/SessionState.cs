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
    private static LabAction.ActionStatus activeActionStatus;
    private static LabAction focusedAction;

    private static Well focusedWell;
    private static List<Well> selectedWells;

    //data streams
    public static Subject<string> procedureNameStream = new Subject<string>();
    public static Subject<int> stepStream = new Subject<int>();
    public static Subject<int> newStepStream = new Subject<int>();
    
    public static Subject<Sample> activeSampleStream = new Subject<Sample>();
    public static Subject<Sample> newSampleStream = new Subject<Sample>();
    public static Subject<(string, string)> editedSampleStream = new Subject<(string,string)>();
    public static Subject<string> SampleRemovedStream = new Subject<string>();
    
    public static Subject<Well> focusedWellStream = new Subject<Well>();
    public static Subject<List<Well>> selectedWellsStream = new Subject<List<Well>>();

    public static Subject<LabAction.ActionType> actionTypeStream = new Subject<LabAction.ActionType>();
    public static Subject<LabAction.ActionStatus> actionStatusStream = new Subject<LabAction.ActionStatus>();
    public static Subject<LabAction> focusedActionStream = new Subject<LabAction>();

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

    public static List<Well> SelectedWells
    {
        set
        {
            if (selectedWells != value)
            {
                selectedWells = value;
            }
            selectedWellsStream.OnNext(selectedWells);
        }
        get
        {
            return selectedWells;
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

    public static LabAction.ActionStatus ActiveActionStatus
    {
        set
        {
            if(activeActionStatus != value)
            {
                activeActionStatus = value;
                actionStatusStream.OnNext(activeActionStatus);
            }
        }
        get
        {
            return activeActionStatus;
        }
    }

    public static LabAction FocusedAction
    {
        set
        {
            if (focusedAction != value)
            {
                focusedAction = value;
                focusedActionStream.OnNext(focusedAction);
            }
        }
        get
        {
            return focusedAction;
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

    public static void AddNewStep()
    {
        Steps.Add(new Step());
        ActiveStep = Steps.Count - 1;
        newStepStream.OnNext(ActiveStep);
    }

    public static void RemoveCurrentStep()
    {
        Steps.Remove(CurrentStep);
    }

    public static void AddNewSample(string name, string abreviation, string colorName, Color color, string vesselType)
    {
        Sample newSample = new Sample(name, abreviation, colorName, color);

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

    public static void RemoveSample(string name)
    {
        Sample forRemoval = AvailableSamples.Where(sample => sample.sampleName == name).FirstOrDefault();

        if (forRemoval != null)
        {
            ActiveSample = forRemoval;

            RemoveSampleFromAllWells(forRemoval);

            ActiveSample = null;

            UsedColors.Remove(forRemoval.colorName);

            RemoveSampleFromMateralsList(forRemoval);
        }
    }

    static void RemoveSampleFromAllWells(Sample forRemoval)
    {
        foreach (Step step in Steps)
        {
            foreach (var plate in step.materials)
            {
                foreach (var well in plate.GetWells())
                {
                    if (well.Value.ContainsSample(forRemoval.color))
                    {
                        step.TryRemoveActiveSampleFromWell(well.Key, well.Value.plateId);
                    }
                }
            }
        }
    }

    static void RemoveSampleFromMateralsList(Sample forRemoval)
    {
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

    public static void SetActiveSample(Sample selected)
    {
        if(AvailableSamples.Contains(selected))
        {
            ActiveSample = selected;
            return;
        }
        Debug.LogWarning("Selected sample is not available");
    }

    public static void SetFocusedWell(string wellId, int plateId)
    {
        if (!CurrentStep.materials[plateId].ContainsWell(wellId))
        {
            CurrentStep.materials[plateId].AddWell(wellId, new Well(wellId, plateId));
            FocusedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
        else
        {
            FocusedWell = CurrentStep.materials[plateId].GetWell(wellId);
        }
    }

    public static void SetSelectedWells(int plateId)
    {
        List<Well> selectedWells = new List<Well>();

        foreach(var well in CurrentStep.materials[plateId].GetWells())
        {
            if(well.Value.selected)
            {
                selectedWells.Add(well.Value);
            }
        }
        SelectedWells = selectedWells;
    }  

    public static void SetFocusedAction(LabAction action)
    {
        FocusedAction = action;
    }
}

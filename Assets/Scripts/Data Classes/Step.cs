using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class Step
{
    public List<LabMaterial> materials;
    public List<LabAction> actions;

    private int GroupId;

    //data streams
    public Subject<LabAction> actionAddedStream = new Subject<LabAction>();
    public Subject<LabAction> actionRemovedStream = new Subject<LabAction>();

    public Step()
    {
        materials = new List<LabMaterial>();
        actions = new List<LabAction>();
        AddWellplates();
    }

    void AddWellplates()
    {
        foreach (var material in SessionState.Materials)
        {
            if (material is Wellplate)
            {
                var wellplate = (Wellplate)material;
                materials.Add(new Wellplate(wellplate.id, wellplate.materialName, wellplate.numWells));
            }
        }
    }

    public bool TryAddActiveSampleToWell(string wellName, int plateId, bool inGroup, bool isStart, bool isEnd)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            if (!materials[plateId].ContainsWell(wellName))
            {
                materials[plateId].AddWell(wellName, new Well(wellName, plateId));
            }

            if (materials[plateId].ContainsWell(wellName) && !materials[plateId].GetWell(wellName).ContainsSample(SessionState.ActiveSample))
            {
                if (inGroup)
                {
                    materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, SessionState.ActiveSample));

                    if (isEnd)
                    {
                        string multichannelTargetID = "";
                        foreach (var well in materials[plateId].GetWells())
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
        return false;
    }

    public bool TryRemoveActiveSampleFromWell(string wellName, int plateId)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            if (materials[plateId].ContainsWell(wellName))
            {
                LabAction removalAction = actions.Where(action => action.source.color == SessionState.ActiveSample.color && action.WellIsTarget(plateId.ToString(), wellName)).FirstOrDefault();
                
                if (SessionState.ActiveSample != null && removalAction != null)
                {
                    RemoveAction(removalAction);

                    SessionState.SampleRemovedStream.OnNext(wellName);

                    if (materials[plateId].GetWell(wellName).groups != null)
                    {
                        RemoveSampleGroup(wellName, plateId);
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

    void RemoveSampleGroup(string wellName, int plateId)
    {
        foreach (Well.SampleGroup group in materials[plateId].GetWell(wellName).groups)
        {
            if (group.Sample == SessionState.ActiveSample)
            {
                int IdForRemoval = group.groupId;

                string multichannelTargetID = "";
                string groupStart = "";
                string groupEnd = "";

                foreach (var well in materials[plateId].GetWells())
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
                LabAction removalAction = actions.Where(a => a.source.color == SessionState.ActiveSample.color && a.target.matID == plateId.ToString() && a.target.matSubID == multichannelTargetID).FirstOrDefault();

                if (removalAction != null)
                {
                    Debug.Log("Removing Action");
                    RemoveAction(removalAction);
                }

                RemoveAllSamplesInGroup(IdForRemoval, plateId);
                break;
            }
        }
    }

    void RemoveAllSamplesInGroup(int removalID, int plateId)
    {
        List<Well.SampleGroup> groupsToRemove = new List<Well.SampleGroup>();

        foreach (var well in materials[plateId].GetWells())
        {
            foreach (Well.SampleGroup group in well.Value.groups)
            {
                if (group.groupId == removalID)
                {
                    groupsToRemove.Add(group);
                    SessionState.SampleRemovedStream.OnNext(well.Key);
                }
            }
            well.Value.groups.RemoveAll(item => groupsToRemove.Contains(item));
            groupsToRemove.Clear();
        }
    }

    public List<LabAction> GetActionsWithTargetWell(Well well)
    {
        List<LabAction> associatedActions = new List<LabAction>();

        foreach(LabAction action in actions)
        {
            if(action.WellIsTarget(well.plateId.ToString(), well.id))
            {
                associatedActions.Add(action);
            }
        }
        
        return associatedActions;
    }

    public List<LabAction> GetActionsWithSourceSample(Sample sample)
    {
        List<LabAction> associatedActions = new List<LabAction>();

        foreach (LabAction action in actions)
        {
            if (action.SampleIsSource(sample))
            {
                associatedActions.Add(action);
            }
        }

        return associatedActions;
    }

    public void AddAction(LabAction.ActionType action, LabAction.Source source, LabAction.Target target)
    {
        var newAction = new LabAction(action, source, target);
        actions.Add(newAction);
        actionAddedStream.OnNext(newAction);
    }

    public void RemoveAction(LabAction action)
    {
        actions.Remove(action);
        actionRemovedStream.OnNext(action);
    }

    void AddPipetteAction(string plateID, string wellID)
    {
        //add action to session state
        string sourceID = "";
        string sourceSubID = "";
        foreach (var material in SessionState.Materials)
        {
            if(material.ContainsSample(SessionState.ActiveSample))
            {
                sourceID = material.id.ToString();
                sourceSubID = material.GetSampleID(SessionState.ActiveSample);
            }
        }
        var source = new LabAction.Source(sourceID, sourceSubID, SessionState.ActiveSample.color, SessionState.ActiveSample.colorName, SessionState.ActiveTool.volume, "μL");
        var target = new LabAction.Target(plateID, wellID, SessionState.ActiveSample.color, SessionState.ActiveSample.colorName);
        AddAction(LabAction.ActionType.pipette, source, target);
    }

    public void AddTransferAction(string sourcePlateId, string sourceWellId, string targetPlateId, string targetWellId, float volume)
    {
        var source = new LabAction.Source(sourcePlateId, sourceWellId, Color.red, "Red", volume, "μL");
        var target = new LabAction.Target(targetPlateId, targetWellId, Color.green, "Green");
        AddAction(LabAction.ActionType.transfer, source, target);
    }

    public void AddDilutionAction(string sourceId, string SourceSubId, string targetId, string targetSubId,float dilutionFactor)
    {
        var source = new LabAction.Source(sourceId, SourceSubId, Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetId, targetSubId, Color.green, "Green");
        AddAction(LabAction.ActionType.dilution, source, target);
    }

    public void AddDilutionActionStart(Sample sourceSample, string targetId, string targetSubId, float dilutionFactor)
    {
        var sourceMaterial = SessionState.Materials.Where(material => material.ContainsSample(sourceSample)).FirstOrDefault();
        var sampleList = sourceMaterial.GetSampleList();
        var sampleID = sampleList.IndexOf(sourceSample);

        var source = new LabAction.Source(sourceMaterial.id.ToString(), sampleID.ToString(), Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetId, targetSubId, Color.green, "Green");
        AddAction(LabAction.ActionType.dilution, source, target);
    }
}

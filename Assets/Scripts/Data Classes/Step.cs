using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class Step
{
    public List<LabAction> actions;

    private int GroupId;

    //data streams
    public Subject<LabAction> actionAddedStream = new Subject<LabAction>();
    public Subject<LabAction> actionRemovedStream = new Subject<LabAction>();

    public Step()
    {
        actions = new List<LabAction>();
    }

    public bool TryAddActiveSampleToWell(string wellName, int plateId, bool inGroup, bool isStart, bool isEnd)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            if (!SessionState.Materials[plateId].ContainsWell(wellName))
            {
                SessionState.Materials[plateId].AddWell(wellName, new Well(wellName, plateId));
            }

            if (SessionState.Materials[plateId].ContainsWell(wellName) && !SessionState.Materials[plateId].GetWell(wellName).ContainsSample(SessionState.ActiveSample))
            {
                if (inGroup)
                {
                    SessionState.Materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, SessionState.ActiveSample));

                    if (isEnd)
                    {
                        string multichannelTargetID = "";
                        foreach (var well in SessionState.Materials[plateId].GetWells())
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
            if (SessionState.Materials[plateId].ContainsWell(wellName))
            {
                LabAction removalAction = actions.Where(action => action.source.color == SessionState.ActiveSample.color && action.WellIsTarget(plateId.ToString(), wellName)).FirstOrDefault();
                
                if (SessionState.ActiveSample != null && removalAction != null)
                {
                    RemoveAction(removalAction);

                    SessionState.SampleRemovedStream.OnNext(wellName);

                    if (SessionState.Materials[plateId].GetWell(wellName).groups != null)
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
        foreach (Well.SampleGroup group in SessionState.Materials[plateId].GetWell(wellName).groups)
        {
            if (group.Sample == SessionState.ActiveSample)
            {
                int IdForRemoval = group.groupId;

                string multichannelTargetID = "";
                string groupStart = "";
                string groupEnd = "";

                foreach (var well in SessionState.Materials[plateId].GetWells())
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

        foreach (var well in SessionState.Materials[plateId].GetWells())
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


    public List<LabAction> GetActionsWithSourceWell(Well well)
    {
        List<LabAction> associatedActions = new List<LabAction>();

        foreach (LabAction action in actions)
        {
            if (action.WellIsSource(well.plateId.ToString(), well.id))
            {
                associatedActions.Add(action);
            }
        }

        return associatedActions;
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
            if (action.SourceIsSample(sample))
            {
                associatedActions.Add(action);
            }
        }

        return associatedActions;
    }

    public void AddAction(LabAction action)
    {
        actions.Add(action);
        actionAddedStream.OnNext(action);
    }

    public void RemoveAction(LabAction action)
    {
        actionRemovedStream.OnNext(action);
        actions.Remove(action);
    }

    public void AddPipetteAction(string plateID, string wellID)
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
        var newAction = new LabAction(SessionState.ActiveStep, LabAction.ActionType.pipette, source, target);
        AddAction(newAction);
    }

    public void AddTransferAction(string sourcePlateId, string sourceWellId, string targetPlateId, string targetWellId, float volume)
    {
        if(SessionState.ActiveStep == 0 && SessionState.CurrentStep.actions.Count() == 0)
        {
            Debug.LogWarning("First action of protocol cannot be a transfer");
            return;
        }

        var source = new LabAction.Source(sourcePlateId, sourceWellId, Color.red, "Red", volume, "μL");
        var target = new LabAction.Target(targetPlateId, targetWellId, Color.green, "Green");
        var newAction = new LabAction(SessionState.ActiveStep, LabAction.ActionType.transfer, source, target);

        //if this is the first action of the step check the source well volume at the final action of the previous step
        if(SessionState.ActiveStep > 0 && SessionState.CurrentStep.actions.Count() == 0)
        {
            Step prevStep = SessionState.Steps[SessionState.ActiveStep - 1];
            foreach (Well sourceWell in newAction.TryGetSourceWells())
            {
                if (prevStep.actions.Count() > 0 && sourceWell.GetVolumeAtAction(prevStep.actions[prevStep.actions.Count() - 1]) < volume)
                {
                    Debug.LogWarning("cannot perform transfer, well " + sourceWell.id + " has insufficent volume.");
                    return;
                }
            }
        }
        else
        {
            foreach (Well sourceWell in newAction.TryGetSourceWells())
            {
                if (SessionState.CurrentStep.actions.Count() > 0 && sourceWell.GetVolumeAtAction(SessionState.CurrentStep.actions[SessionState.CurrentStep.actions.Count() - 1]) < volume)
                {
                    Debug.LogWarning("cannot perform transfer, well " + sourceWell.id + " has insufficent volume.");
                    return;
                }
            }
        }

        AddAction(newAction);
    }

    public void AddDilutionAction(string sourceId, string SourceSubId, string targetId, string targetSubId,float dilutionFactor)
    {
        var source = new LabAction.Source(sourceId, SourceSubId, Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetId, targetSubId, Color.green, "Green");
        var newAction = new LabAction(SessionState.ActiveStep, LabAction.ActionType.dilution, source, target);
        AddAction(newAction);
    }

    public void AddDilutionActionStart(Sample sourceSample, string targetId, string targetSubId, float dilutionFactor)
    {
        var sourceMaterial = SessionState.Materials.Where(material => material.ContainsSample(sourceSample)).FirstOrDefault();
        var sampleList = sourceMaterial.GetSampleList();
        var sampleID = sampleList.IndexOf(sourceSample);

        var source = new LabAction.Source(sourceMaterial.id.ToString(), sampleID.ToString(), Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetId, targetSubId, Color.green, "Green");
        var newAction = new LabAction(SessionState.ActiveStep, LabAction.ActionType.dilution, source, target);
        AddAction(newAction);
    }
}

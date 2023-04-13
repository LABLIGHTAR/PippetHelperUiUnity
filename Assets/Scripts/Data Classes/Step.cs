
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

    public bool AddActiveSampleToWell(string wellName, int plateId, bool inGroup, bool isStart, bool isEnd)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            if (materials[plateId].ContainsWell(wellName))
            {
                if (!materials[plateId].GetWell(wellName).Samples.ContainsKey(SessionState.ActiveSample))
                {
                    //if the well exists and does not already have the active Sample add it
                    materials[plateId].GetWell(wellName).Samples.Add(SessionState.ActiveSample, SessionState.ActiveTool.volume);
                    //if this Sample is grouped add it to the group list
                    if (inGroup)
                    {
                        materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, SessionState.ActiveSample));
                        //if this is the last well in the group increment the group id for the next group
                        if (isEnd)
                        {
                            //Add group action
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
                materials[plateId].AddWell(wellName, new Well(wellName, plateId));
                //add the active Sample to the new well
                materials[plateId].GetWell(wellName).Samples.Add(SessionState.ActiveSample, SessionState.ActiveTool.volume);
                //if this Sample is grouped add it to the group list
                if (inGroup)
                {
                    materials[plateId].GetWell(wellName).groups.Add(new Well.SampleGroup(GroupId, isStart, isEnd, SessionState.ActiveSample));
                    //if this is the last well in the group increment the group id for the next group
                    if (isEnd)
                    {
                        //Add group action
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
                    //add single action
                    AddPipetteAction(plateId.ToString(), wellName);
                }
                return true;
            }
        }
        return false;
    }

    //removes active sample from passed well at passed step
    public bool RemoveActiveSampleFromWell(string wellName, int plateId)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            if (materials[plateId].ContainsWell(wellName))
            {
                if (SessionState.ActiveSample != null && materials[plateId].GetWell(wellName).Samples.ContainsKey(SessionState.ActiveSample))
                {
                    //if the well exists and has the active Sample remove it
                    materials[plateId].GetWell(wellName).Samples.Remove(SessionState.ActiveSample);
                    //remove the associated action
                    LabAction removalAction = actions.Where(a => a.source.color == SessionState.ActiveSample.color && a.target.matID == plateId.ToString() && a.target.matSubID == wellName).FirstOrDefault();
                    if (removalAction != null)
                    {
                        RemoveAction(removalAction);
                    }

                    SessionState.SampleRemovedStream.OnNext(wellName);

                    //if the Sample being removed is part of a group remove the group everywhere
                    if (materials[plateId].GetWell(wellName).groups != null)
                    {
                        foreach (Well.SampleGroup group in materials[plateId].GetWell(wellName).groups)
                        {
                            if (group.Sample == SessionState.ActiveSample)
                            {
                                int IdForRemoval = group.groupId;

                                //remove group action
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
                                Debug.Log(multichannelTargetID);
                                removalAction = actions.Where(a => a.source.color == SessionState.ActiveSample.color && a.target.matID == plateId.ToString() && a.target.matSubID == multichannelTargetID).FirstOrDefault();
                                if (removalAction != null)
                                {
                                    Debug.Log("Removing Action");
                                    RemoveAction(removalAction);
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
    void RemoveAllSamplesInGroup(int removalID, int plateId)
    {
        List<Well.SampleGroup> groupsToRemove = new List<Well.SampleGroup>();
        //iterate through all wells
        foreach (var well in materials[plateId].GetWells())
        {
            //iterate through each well group
            foreach (Well.SampleGroup group in well.Value.groups)
            {
                if (group.groupId == removalID)
                {
                    //add the group to a list for removal (cannot modify list in foreach loop)
                    groupsToRemove.Add(group);
                    //remove the active Sample
                    well.Value.Samples.Remove(SessionState.ActiveSample);
                    //notify well
                    SessionState.SampleRemovedStream.OnNext(well.Key);
                }
            }
            //remove groups
            well.Value.groups.RemoveAll(item => groupsToRemove.Contains(item));
            groupsToRemove.Clear();
        }
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
            var samples = material.GetSampleList();
            if (samples != null)
            {
                foreach (var sample in samples)
                {
                    if (sample == SessionState.ActiveSample)
                    {
                        sourceID = material.id.ToString();
                        sourceSubID = samples.IndexOf(sample).ToString();
                        break;
                    }
                }
            }
        }
        var source = new LabAction.Source(sourceID, sourceSubID, SessionState.ActiveSample.color, SessionState.ActiveSample.colorName, SessionState.ActiveTool.volume, "μL");
        var target = new LabAction.Target(plateID, wellID, SessionState.ActiveSample.color, SessionState.ActiveSample.colorName);
        AddAction(LabAction.ActionType.pipette, source, target);
    }

    public void AddTransferAction(string sourcePlateId, string souceWellId, string targetPlateId, string targetWellId, float volume)
    {
        var source = new LabAction.Source(sourcePlateId, souceWellId, Color.red, "Red", volume, "μL");
        var target = new LabAction.Target(targetPlateId, targetWellId, Color.green, "Green");
        AddAction(LabAction.ActionType.transfer, source, target);
    }

    public void AddDilutionAction(Well sourceWell, Well targetWell, float dilutionFactor)
    {
        var source = new LabAction.Source(sourceWell.plateId.ToString(), sourceWell.id, Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetWell.plateId.ToString(), targetWell.id, Color.green, "Green");
        AddAction(LabAction.ActionType.dilution, source, target);
    }

    public void AddDilutionActionStart(Sample sourceSample, Well targetWell, float dilutionFactor)
    {
        var sourceMaterial = SessionState.Materials.Where(material => material.ContainsSample(sourceSample)).FirstOrDefault();
        var sampleList = sourceMaterial.GetSampleList();
        var sampleID = sampleList.IndexOf(sourceSample);

        var source = new LabAction.Source(sourceMaterial.id.ToString(), sampleID.ToString(), Color.red, "Red", dilutionFactor, "μL");
        var target = new LabAction.Target(targetWell.plateId.ToString(), targetWell.id, Color.green, "Green");
        AddAction(LabAction.ActionType.dilution, source, target);
    }
}

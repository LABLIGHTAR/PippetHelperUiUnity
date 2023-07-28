using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public int plateId;
    public string id;
    public bool selected;
    public List<SampleGroup> groups;

    public Well(string wellId, int parentId)
    {
        id = wellId;
        plateId = parentId;
        groups = new List<SampleGroup>();
    }

    public bool IsStartOfGroup(int groupID)
    {
        foreach (SampleGroup group in groups)
        {
            if (group.groupId == groupID && group.isStart)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsEndOfGroup(int groupID)
    {
        foreach (SampleGroup group in groups)
        {
            if (group.groupId == groupID && group.isEnd)
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsSample(Sample sample)
    {
        if(GetSamples().Contains(sample))
            return true;
        return false;
    }

    /// <summary>
    /// returns all samples in well up to active step
    /// </summary>
    /// <returns></returns>
    public List<Sample> GetSamples()
    {
        List<Sample> samples = new List<Sample>();

        for(int i=0; i<=SessionState.ActiveStep; i++)
        {
            foreach(var action in SessionState.Steps[i].GetActionsWithTargetWell(this))
            {
                //get samples pipetted directly into well
                if (action.type == LabAction.ActionType.pipette)
                {
                    Sample sourceSample = action.TryGetSourceSample();
                    if (sourceSample != null)
                    {
                        samples.Add(sourceSample);
                    }
                }
                //get samples transfered into well
                else if(action.type == LabAction.ActionType.transfer)
                {
                    List<Sample> sourceWellSamples = action.TryGetSourceWellSamples(this);
                    if(sourceWellSamples != null)
                    {
                        samples = samples.Union(sourceWellSamples).ToList();
                    }
                }
            }
        }

        return samples;
    }


    public List<Sample> GetSamplesBeforeAction(LabAction action)
    {
        List<Sample> samples = new List<Sample>();

        for (int i = 0; i <= action.step; i++)
        {
            foreach (var a in SessionState.Steps[i].GetActionsWithTargetWell(this))
            {
                if (i < action.step)
                {
                    samples = samples.Union(GetSamplesFromAction(a)).ToList();
                }
                else
                {
                    if (SessionState.Steps[i].actions.IndexOf(a) < SessionState.Steps[i].actions.IndexOf(action))
                    {
                        samples = samples.Union(GetSamplesFromAction(a)).ToList();
                    }
                }
            }
        }

        return samples;
    }

    private List<Sample> GetSamplesFromAction(LabAction action)
    {
        List<Sample> samples = new List<Sample>();

        //get samples pipetted directly into well
        if (action.type == LabAction.ActionType.pipette)
        {
            Sample sourceSample = action.TryGetSourceSample();
            if (sourceSample != null)
            {
                samples.Add(sourceSample);
            }
        }
        //get samples transfered into well
        else if (action.type == LabAction.ActionType.transfer)
        {
            List<Sample> sourceWellSamples = action.TryGetSourceWellSamples(this);
            if (sourceWellSamples != null)
            {
                samples = samples.Union(sourceWellSamples).ToList();
            }
        }

        return samples;
    }

    public float GetVolumeAtAction(LabAction action)
    {
        float volume = 0f;

        for(int i=0; i<=action.step; i++)
        {
            //add all volumes
            foreach (var a in SessionState.Steps[i].GetActionsWithTargetWell(this))
            {
                if (i < action.step)
                {
                    volume += a.source.volume;
                }
                else
                {
                    if(SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                    {
                        volume += a.source.volume;
                    }
                }
            }
            //subtract volumes
            foreach (var a in SessionState.Steps[i].GetActionsWithSourceWell(this))
            {
                if (i < action.step)
                {
                    volume -= a.source.volume;
                }
                else
                {
                    if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                    {
                        volume -= a.source.volume;
                    }
                }
            }
        }

        return volume;
    }

    public float GetSampleVolumeAtAction(Sample sample, LabAction action)
    {
        float volume = 0f; 

        if(ContainsSample(sample))
        {
            for(int i=0; i<=action.step; i++)
            {
                //when well is target add volume
                foreach(var a in SessionState.Steps[i].GetActionsWithTargetWell(this))
                {
                    if(i < action.step)
                    {
                        volume += AddSampleVolumeAtAction(sample, a);
                    }
                    else
                    {
                        if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                        {
                            volume += AddSampleVolumeAtAction(sample, a);
                        }
                    }
                }
                //when well is source subtract volume
                foreach (var a in SessionState.Steps[i].GetActionsWithSourceWell(this))
                {
                    if (i < action.step)
                    {
                        volume += SubtractSampleVolumeAtAction(sample, a);
                    }
                    else
                    {
                        if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                        {
                            volume += SubtractSampleVolumeAtAction(sample, a);
                        }
                    }
                }
            }
        }

        return volume;
    }

    //if adding sample volume this well is the target of the action
    private float AddSampleVolumeAtAction(Sample sample, LabAction action)
    {
        float volume = 0f;

        if (action.type == LabAction.ActionType.pipette && action.SourceIsSample(sample))
        {
            volume += action.source.volume;
        }
        else if (action.type == LabAction.ActionType.transfer && action.TryGetSourceWellSamples(this).Contains(sample))
        {
            Well sourceWell = action.TryGetSourceWell(this);

            LabAction prevAction = SessionState.TryGetPreviousAction(action);

            if (sourceWell !=null && prevAction != null)
            {
                volume += ((sourceWell.GetSampleVolumeAtAction(sample, prevAction) / sourceWell.GetVolumeAtAction(prevAction)) * action.source.volume);
            }
        }

        return volume;
    }

    //if subtracting sample volume this well is the source of the action
    private float SubtractSampleVolumeAtAction(Sample sample, LabAction action)
    {
        float volume = 0f;

        if (action.type == LabAction.ActionType.transfer)
        {
            LabAction prevAction = SessionState.TryGetPreviousAction(action);

            if (prevAction != null)
            {
                volume -= ((this.GetSampleVolumeAtAction(sample, prevAction) / this.GetVolumeAtAction(prevAction)) * action.source.volume);
            }
        }

        return volume;
    }
}

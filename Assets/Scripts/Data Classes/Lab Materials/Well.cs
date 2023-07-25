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

    public List<Sample> GetSamples()
    {
        List<Sample> samples = new List<Sample>();

        for(int i=0; i<=SessionState.ActiveStep; i++)
        {
            foreach(var action in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
            {
                if (action.type == LabAction.ActionType.pipette)
                {
                    Sample sourceSample = action.TryGetSourceSample();
                    if (sourceSample != null)
                    {
                        samples.Add(sourceSample);
                    }
                }
                else if(action.type == LabAction.ActionType.transfer)
                {
                    List<Sample> sourceWellSamples = action.TryGetSourceWellSamples();
                    if(sourceWellSamples != null)
                    {
                        samples.Union(sourceWellSamples);
                    }
                }
            }
        }

        return samples;
    }


    public List<Sample> GetSamplesBeforeAction(LabAction action)
    {
        List<Sample> samples = new List<Sample>();

        foreach(LabAction a in SessionState.GetAllActionsBefore(action).Where(a => a.WellIsTarget(plateId.ToString(), id) && a.type == LabAction.ActionType.pipette))
        {
            Sample sourceSample = a.TryGetSourceSample();
            if (sourceSample != null)
            {
                samples.Add(sourceSample);
            }
        }

        return samples;
    }

    public float GetVolumeAtAction(LabAction action)
    {
        float volume = 0f;

        for(int i=0; i<=action.step; i++)
        {
            foreach (var a in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
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
                foreach(var a in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
                {
                    if(i < action.step)
                    {
                        if (a.type == LabAction.ActionType.pipette && a.SampleIsSource(sample))
                        {
                            volume += a.source.volume;
                        }
                        else if (a.type == LabAction.ActionType.transfer && a.TryGetSourceWellSamples().Contains(sample))
                        {
                            volume += (a.source.volume / a.TryGetSourceWellSamples().Count());
                        }
                    }
                    else
                    {
                        if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                        {
                            if (a.type == LabAction.ActionType.pipette && a.SampleIsSource(sample))
                            {
                                volume += a.source.volume;
                            }
                            else if (a.type == LabAction.ActionType.transfer && a.TryGetSourceWellSamples().Contains(sample))
                            {
                                volume += (a.source.volume / a.TryGetSourceWellSamples().Count());
                            }
                        }
                    }
                }
            }
        }

        return volume;
    }
}

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

        foreach(LabAction action in SessionState.CurrentStep.actions)
        {
            if (action.WellIsTarget(plateId.ToString(), id) && action.type == LabAction.ActionType.pipette)
            {
                Sample sourceSample = action.TryGetSourceSample();
                if (sourceSample != null)
                {
                    samples.Add(sourceSample);
                }
            }
        }

        return samples;
    }
}

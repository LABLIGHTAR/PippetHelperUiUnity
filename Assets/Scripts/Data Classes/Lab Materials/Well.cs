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

    public bool ContainsSample(Color sampleColor)
    {
        var actionsWithWellAsTarget = SessionState.CurrentStep.GetActionsWithTargetWell(id, plateId.ToString());
        var actionsWithSampleAsSource = SessionState.CurrentStep.GetActionsWithSourceSample(sampleColor);

        if (actionsWithWellAsTarget.Intersect(actionsWithSampleAsSource) != null)
        {
            return true;
        }
        return false;
    }
}

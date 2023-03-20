using System.Collections;
using System.Collections.Generic;
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

    public Dictionary<Sample, float> Samples;
    public List<SampleGroup> groups;

    public Well()
    {
        Samples = new Dictionary<Sample, float>();
        groups = new List<SampleGroup>();
    }
}

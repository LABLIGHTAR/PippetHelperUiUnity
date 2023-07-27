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
            foreach(var action in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
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
            foreach (var a in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
            {
                if (i < action.step)
                {
                    //get samples pipetted directly into well
                    if (a.type == LabAction.ActionType.pipette)
                    {
                        Sample sourceSample = a.TryGetSourceSample();
                        if (sourceSample != null)
                        {
                            samples.Add(sourceSample);
                        }
                    }
                    //get samples transfered into well
                    else if (a.type == LabAction.ActionType.transfer)
                    {
                        List<Sample> sourceWellSamples = a.TryGetSourceWellSamples(this);
                        if (sourceWellSamples != null)
                        {
                            samples = samples.Union(sourceWellSamples).ToList();
                        }
                    }
                }
                else
                {
                    if (SessionState.Steps[i].actions.IndexOf(a) < SessionState.Steps[i].actions.IndexOf(action))
                    {
                        //get samples pipetted directly into well
                        if (a.type == LabAction.ActionType.pipette)
                        {
                            Sample sourceSample = a.TryGetSourceSample();
                            if (sourceSample != null)
                            {
                                samples.Add(sourceSample);
                            }
                        }
                        //get samples transfered into well
                        else if (a.type == LabAction.ActionType.transfer)
                        {
                            List<Sample> sourceWellSamples = a.TryGetSourceWellSamples(this);
                            if (sourceWellSamples != null)
                            {
                                samples = samples.Union(sourceWellSamples).ToList();
                            }
                        }
                    }
                }
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
            //subtract volumes
            foreach (var a in SessionState.Steps[i].actions.Where(a => a.WellIsSource(plateId.ToString(), id)))
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
                //add volumes when well is target
                foreach(var a in SessionState.Steps[i].actions.Where(a => a.WellIsTarget(plateId.ToString(), id)))
                {
                    if(i < action.step)
                    {
                        if (a.type == LabAction.ActionType.pipette && a.SourceIsSample(sample))
                        {
                            volume += a.source.volume;
                        }
                        else if (a.type == LabAction.ActionType.transfer && a.TryGetSourceWellSamples(this).Contains(sample))
                        {
                            Well sourceWell = a.TryGetSourceWell(this);
                            float samplePercent = sourceWell.GetSampleVolumeAtAction(sample, a) / sourceWell.GetVolumeAtAction(a);
                            volume += (a.source.volume * samplePercent);
                        }
                    }
                    else
                    {
                        if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                        {
                            if (a.type == LabAction.ActionType.pipette && a.SourceIsSample(sample))
                            {
                                volume += a.source.volume;
                            }
                            else if (a.type == LabAction.ActionType.transfer && a.TryGetSourceWellSamples(this).Contains(sample))
                            {
                                Well sourceWell = a.TryGetSourceWell(this);
                                float samplePercent = sourceWell.GetSampleVolumeAtAction(sample, a) / sourceWell.GetVolumeAtAction(a);
                                volume += (a.source.volume * samplePercent);
                            }
                        }
                    }
                }
                //subtract volumes when well is source
                foreach (var a in SessionState.Steps[i].actions.Where(a => a.WellIsSource(plateId.ToString(), id)))
                {
                    if (i < action.step)
                    {
                        if (a.type == LabAction.ActionType.transfer)
                        {
                            volume -= (a.source.volume / this.GetSamplesBeforeAction(a).Count());
                        }
                    }
                    else
                    {
                        if (SessionState.Steps[i].actions.IndexOf(a) <= SessionState.Steps[i].actions.IndexOf(action))
                        {
                            if (a.type == LabAction.ActionType.transfer)
                            {
                                volume -= (a.source.volume / this.GetSamplesBeforeAction(a).Count());
                            }
                        }
                    }
                }
            }
        }

        return volume;
    }
}

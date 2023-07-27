using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LabAction
{
    public enum ActionType
    { 
        pipette,
        transfer,
        dilution
    }

    public enum ActionStatus
    {
        selectingSource,
        selectingTarget,
        awaitingSubmission,
        submitted
    }

    public struct Source
    {
        public string matID;
        public string matSubID;
        public Color color;
        public string colorName;
        public float volume;
        public string units;

        public Source(string materialID, string materialSubID, Color sourceColor, string sourceColorName, float sourceVolume, string sourceVolUnits)
        {
            matID = materialID;
            matSubID = materialSubID;
            color = sourceColor;
            colorName = sourceColorName;
            volume = sourceVolume;
            units = sourceVolUnits;
        }
    }

    public struct Target
    {
        public string matID;
        public string matSubID;
        public Color color;
        public string colorName;

        public Target(string materialID, string materialSubID, Color targetColor, string targetColorName)
        {
            matID = materialID;
            matSubID = materialSubID;
            color = targetColor;
            colorName = targetColorName;
        }
    }

    public int step;
    public ActionType type;
    public Source source;
    public Target target;
    public int numChannels;

    public LabAction(int step, ActionType action, Source sourceMat, Target targetMat)
    {
        this.step = step;
        type = action;
        source = sourceMat;
        target = targetMat;
        numChannels = SessionState.ActiveTool.numChannels;
    }

    public string GetActionString()
    {
        string sourceName = SessionState.Materials[int.Parse(source.matID)].GetNameAsSource(source.matSubID);
        string targetName = SessionState.Materials[int.Parse(target.matID)].GetNameAsTarget(target.matSubID);
        string sourcePlateName = SessionState.Materials[int.Parse(source.matID)].customName;
        string targetPlateName = SessionState.Materials[int.Parse(target.matID)].customName;

        if (type == ActionType.pipette)
        {
            return "Load " + source.volume + "μl" + " of " + sourceName + " into " + targetName + " of " + targetPlateName;
        }
        else if (type == ActionType.transfer)
        {
            return "Transfer " + source.volume + "μl" + " from " + sourceName + " of " + sourcePlateName + " into " + targetName + " of " + targetPlateName;
        }
        else if (type == ActionType.dilution)
        {
            return "Perform a serial " + type.ToString() + " with a factor of " + source.volume + " from " + sourceName + " into " + targetName + " of " + targetPlateName;
        }
        else
        {
            Debug.LogWarning("Undefined Action Type");
            return "Undefined Action Type";
        }
    }

    public bool WellIsSource(string plateId, string wellId)
    {
        if (source.matID == plateId)
        {
            if (numChannels > 1)
            {
                return WellIsSourceMultichannel(wellId);
            }
            else if (source.matSubID == wellId)
            {
                return true;
            }
        }
        return false;
    }

    private bool WellIsSourceMultichannel(string wellId)
    {
        string[] range = source.matSubID.Split("-");

        return GetWellsInRange(range).Contains(wellId);
    }

    public bool WellIsTarget(string plateId, string wellId)
    {
        if(target.matID == plateId)
        {
            if(numChannels > 1)
            {
                return WellIsTargetMultichannel(wellId);
            }
            else if(target.matSubID == wellId)
            {
                return true;
            }
        }
        return false;
    }

    private bool WellIsTargetMultichannel(string wellId)
    {
        string[] range = target.matSubID.Split("-");

        return GetWellsInRange(range).Contains(wellId);
    }

    List<string> GetWellsInRange(string[] range)
    {
        string startID = range[0];
        char startChar = startID[0];
        int startNum = int.Parse(startID.Substring(1));

        string endID = range[1];
        char endChar = endID[0];
        int endNum = int.Parse(endID.Substring(1));

        List<string> wellIds = new List<string>();

         //horizontal group
        if (startChar == endChar)
        {
           for(int i=0; i<numChannels; i++)
            {
                var nextWellNum = startNum + i;
                string nextWell = startChar.ToString() + nextWellNum;
                wellIds.Add(nextWell);
            }
        }
        //vertical group
        else
        {
            for (int i = 0; i < numChannels; i++)
            {
                var nextWellChar = ((char)(startChar + i));
                string nextWell = nextWellChar.ToString() + startNum.ToString();
                wellIds.Add(nextWell);
            }
        }

        return wellIds;
    }

    public bool SourceIsWellplate()
    {
        if (SessionState.Materials[int.Parse(source.matID)] is Wellplate)
            return true;
        return false;
    }

    public bool TargetIsWellplate()
    {
        if (SessionState.Materials[int.Parse(target.matID)] is Wellplate)
            return true;
        return false;
    }
    
    public bool SourceIsSample(Sample sample)
    {
        //return (source.color == sample.color);
        return (SessionState.Materials[int.Parse(source.matID)].GetNameAsSource(source.matSubID) == sample.sampleName);
    }

    public Sample TryGetSourceSample()
    {
        return SessionState.Materials[int.Parse(source.matID)].GetSampleList().Where(sample => SourceIsSample(sample)).FirstOrDefault();
    }

    public List<Well> TryGetSourceWells()
    {
        if(SourceIsWellplate())
        {
            List<Well> sourceWells = new List<Well>();

            if(numChannels > 1)
            {
                string[] range = source.matSubID.Split("-");

                foreach (string wellId in GetWellsInRange(range))
                {
                    sourceWells.Add(((Wellplate)SessionState.Materials[int.Parse(source.matID)]).wells[wellId]);
                }
            }
            else
            {
                sourceWells.Add(((Wellplate)SessionState.Materials[int.Parse(source.matID)]).wells[source.matSubID]);
            }
            return sourceWells;
        }
        return null;
    }

    public List<Well> TryGetTargetWells()
    {
        if (TargetIsWellplate())
        {
            List<Well> targetWells = new List<Well>();

            if (numChannels > 1)
            {
                string[] range = target.matSubID.Split("-");

                foreach (string wellId in GetWellsInRange(range))
                {
                    targetWells.Add(((Wellplate)SessionState.Materials[int.Parse(target.matID)]).wells[wellId]);
                }
            }
            else
            {
                targetWells.Add(((Wellplate)SessionState.Materials[int.Parse(target.matID)]).wells[target.matSubID]);
            }
            return targetWells;
        }
        return null;
    }

    public List<Sample> TryGetSourceWellSamples(Well targetWell)
    {
        List<Sample> samples = new List<Sample>();

        Well sourceWell = TryGetSourceWell(targetWell);

        if(sourceWell != null)
        {
            samples = sourceWell.GetSamplesBeforeAction(this);
        }

        return samples;
    }

    public List<Sample> TryGetTargetWellSamples(Well sourceWell)
    {
        List<Sample> samples = new List<Sample>();

        Well targetWell = TryGetSourceWell(sourceWell);

        if (targetWell != null)
        {
            samples = targetWell.GetSamplesBeforeAction(this);
        }

        return samples;
    }

    public Well TryGetSourceWell(Well targetWell)
    {
        int targetWellIndex = TryGetTargetWells().IndexOf(targetWell);
        var sourceWells = TryGetSourceWells();

        if (targetWellIndex > -1 && sourceWells != null)
        {
            return TryGetSourceWells()[targetWellIndex];
        }
        return null;
    }

    public void UpdateSourceSample(Sample sample)
    {
        source.color = sample.color;
        source.colorName = sample.colorName;
    }
}

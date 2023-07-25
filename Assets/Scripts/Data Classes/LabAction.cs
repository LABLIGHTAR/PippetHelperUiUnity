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
                return WellIsSourceMultichannel(plateId, wellId);
            }
            else if (source.matSubID == wellId)
            {
                return true;
            }
        }
        return false;
    }

    private bool WellIsSourceMultichannel(string plateID, string wellId)
    {
        string[] wellIDs = source.matSubID.Split("-");

        string startID = wellIDs[0];
        char startChar = startID[0];
        int startNum = int.Parse(startID.Substring(1));

        string endID = wellIDs[1];
        char endChar = endID[0];
        int endNum = int.Parse(endID.Substring(1));

        char wellChar = wellId[0];
        int wellNum = int.Parse(wellId.Substring(1));

        int numWellsSpanned;
        int offset;

        if (wellId == startID || wellId == endID)
        {
            return true;
        }

        //horizontal group
        else if (startChar == endChar)
        {
            if (wellChar != startChar)
            {
                return false;
            }

            numWellsSpanned = endNum - startNum + 1;

            offset = (numWellsSpanned + 1) / numChannels;

            if (wellNum >= startNum && wellNum <= endNum)
            {
                if (offset > 1 && wellNum % offset == (startNum % 2))
                {
                    return true;
                }
                else if (offset <= 1)
                {
                    return true;
                }
            }
        }

        //vertical group
        else
        {
            if (wellNum != startNum)
            {
                return false;
            }

            numWellsSpanned = (endChar - startChar + 1);

            offset = (numWellsSpanned + 1) / numChannels;

            if (startChar <= wellChar && endChar >= wellChar)
            {
                if (offset > 1 && wellChar % offset == (startChar % 2))
                {
                    return true;
                }
                else if (offset <= 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool WellIsTarget(string plateId, string wellId)
    {
        if(target.matID == plateId)
        {
            if(numChannels > 1)
            {
                return WellIsTargetMultichannel(plateId, wellId);
            }
            else if(target.matSubID == wellId)
            {
                return true;
            }
        }
        return false;
    }

    private bool WellIsTargetMultichannel(string plateID, string wellId)
    {
        string[] wellIDs = target.matSubID.Split("-");
        
        string startID = wellIDs[0];
        char startChar = startID[0];
        int startNum = int.Parse(startID.Substring(1));
       
        string endID = wellIDs[1];
        char endChar = endID[0];
        int endNum = int.Parse(endID.Substring(1));

        char wellChar = wellId[0];
        int wellNum = int.Parse(wellId.Substring(1));

        int numWellsSpanned;
        int offset;

        if (wellId == startID || wellId == endID)
        {
            return true;
        }

        //horizontal group
        else if (startChar == endChar)
        {
            if (wellChar != startChar)
            {
                return false;
            }

            numWellsSpanned = endNum - startNum + 1;

            offset = (numWellsSpanned + 1) / numChannels;

            if(wellNum >= startNum && wellNum <= endNum)
            {
                if (offset > 1 && wellNum % offset == (startNum % 2))
                {
                    return true;
                }
                else if (offset <= 1)
                {
                    return true;
                }
            }
        }

        //vertical group
        else
        {
            if(wellNum !=  startNum)
            {
                return false;
            }

            numWellsSpanned = (endChar - startChar + 1);

            offset = (numWellsSpanned + 1) / numChannels;

            if (startChar <= wellChar && endChar >= wellChar)
            {
                if(offset > 1 && wellChar % offset == (startChar % 2))
                {
                    return true;
                }
                else if(offset <= 1)
                {
                    return true;
                }
            }
        }

        return false;
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
    
    public bool SampleIsSource(Sample sample)
    {
        //return (source.color == sample.color);
        return (SessionState.Materials[int.Parse(source.matID)].GetNameAsSource(source.matSubID) == sample.sampleName);
    }

    public Sample TryGetSourceSample()
    {
        return SessionState.Materials[int.Parse(source.matID)].GetSampleList().Where(sample => SampleIsSource(sample)).FirstOrDefault();
    }

    public Well TryGetSourceWell()
    {
        if(SourceIsWellplate())
        {
            return ((Wellplate)SessionState.Materials[int.Parse(source.matID)]).wells[source.matSubID];
        }
        return null;
    }

    public List<Sample> TryGetSourceWellSamples()
    {
        List<Sample> samples = new List<Sample>();
        Well well = TryGetSourceWell();
        if(well != null)
        {
            samples = well.GetSamplesBeforeAction(this);
        }

        return samples;
    }

    public void UpdateSourceSample(Sample sample)
    {
        source.color = sample.color;
        source.colorName = sample.colorName;
    }
}

using System.Collections;
using System.Collections.Generic;
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

    public ActionType type;
    public Source source;
    public Target target;

    public LabAction(ActionType action, Source sourceMat, Target targetMat)
    {
        type = action;
        source = sourceMat;
        target = targetMat;
    }

    public string GetActionString()
    {
        string sourceName = SessionState.Materials[int.Parse(source.matID)].GetNameAsSource(source.matSubID);
        string targetName = SessionState.Materials[int.Parse(target.matID)].GetNameAsTarget(target.matSubID);

        if (type == ActionType.pipette)
        {
            return "Load " + source.volume + "μl" + " of " + sourceName + " into " + targetName + " of plate " + target.matID;
        }
        else if (type == ActionType.transfer)
        {
            return "Transfer " + source.volume + "μl" + " from " + sourceName + " of plate " + source.matID + " into " + targetName + " of plate " + target.matID;
        }
        else if (type == ActionType.dilution)
        {
            return "Perform a serial " + type.ToString() + " with a factor of " + source.volume + " from " + sourceName + " into " + targetName + " of plate " + target.matID;
        }
        else
        {
            Debug.LogWarning("Undefined Action Type");
            return "Undefined Action Type";
        }
    }
}

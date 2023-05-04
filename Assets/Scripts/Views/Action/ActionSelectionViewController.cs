using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionSelectionViewController : MonoBehaviour
{
    public TMP_Dropdown actionTypeDropdown;
    public TextMeshProUGUI actionTypeText;

    public void SelectActionType()
    {
        switch (actionTypeText.text)
        {
            case ("Load"):
                SessionState.ActiveActionType = LabAction.ActionType.pipette;
                break;
            case ("Transfer"):
                SessionState.ActiveActionType = LabAction.ActionType.transfer;
                break;
            case ("Dilution"):
                SessionState.ActiveActionType = LabAction.ActionType.dilution;
                break;
        }
    }
}

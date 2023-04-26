using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class SelectedWellViewController : MonoBehaviour
{
    public bool isSourceDisplay;
    public List<Well> selectedWells;
    public string plateId;

    public TextMeshProUGUI plateText;
    public TextMeshProUGUI wellText;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.selectedWellsStream.Subscribe(wells =>
        {
            if(isSourceDisplay && SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource)
            {
                UpdateVisualState(wells);
                selectedWells = wells;
            }
            else if(!isSourceDisplay && SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
            {
                UpdateVisualState(wells);
                selectedWells = wells;
            }
        }).AddTo(this);
    }

    public void ClearDisplay()
    {
        selectedWells = null;
        plateText.text = "";
        wellText.text = "";
    }

    void UpdateVisualState(List<Well> wells)
    {
        if(wells.Count == 1)
        {
            wellText.text = wells[0].id;
        }
        else
        {
            wellText.text = wells[0].id + "-" + wells[wells.Count - 1].id;
        }
        plateId = wells[0].plateId.ToString();
        plateText.text = "Plate " + plateId;
    }
}

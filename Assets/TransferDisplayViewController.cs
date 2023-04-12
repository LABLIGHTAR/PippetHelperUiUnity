using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class TransferDisplayViewController : MonoBehaviour
{
    public GameObject sourceWellDisplay;
    public GameObject targetWellDisplay;

    public Button clearSelectionsButton;
    public Button confirmButton;

    public TextMeshProUGUI instructionText;

    private SelectedWellViewController source;
    private SelectedWellViewController target;

    // Start is called before the first frame update
    void Start()
    {
        source = sourceWellDisplay.GetComponent<SelectedWellViewController>();
        source.isSourceDisplay = true;
        target = targetWellDisplay.GetComponent<SelectedWellViewController>();
        target.isSourceDisplay = false;
        ResetUI();

        clearSelectionsButton.onClick.AddListener(ResetUI);
        confirmButton.onClick.AddListener(SubmitTransferAction);
    }

    // Update is called once per frame
    void Update()
    {
        if (source.selectedWells == null)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;
            instructionText.text = "Select a source";
        }
        else if(target.selectedWells == null)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingTarget;
            instructionText.text = "Select a target";
        }
        else if(source.selectedWells != null && target.selectedWells != null)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.awaitingSubmission;
            instructionText.text = "Confirm transfer";
            confirmButton.enabled = true;
        }
    }

    private void ResetUI()
    {
        SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
        SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;
        instructionText.text = "Select a source";
        source.ClearDisplay();
        target.ClearDisplay();
        confirmButton.enabled = false;
    }

    private void SubmitTransferAction()
    {
        if(SessionState.ActiveActionStatus == LabAction.ActionStatus.awaitingSubmission)
        {
            SessionState.AddTransferAction(source.plateId, source.wellText.text, target.plateId, target.wellText.text, SessionState.ActiveTool.volume);
            SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
            //clear UI
            ResetUI();
        }
    }
}

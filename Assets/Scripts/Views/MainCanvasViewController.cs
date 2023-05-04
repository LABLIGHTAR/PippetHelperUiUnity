using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MainCanvasViewController : MonoBehaviour
{
    public GameObject SampleDisplay;
    public Transform ActionDisplay;

    public GameObject TransferDisplayPrefab;
    public GameObject DilutionDisplayPrefab;

    private GameObject TransferDisplay;
    private GameObject DilutionDisplay;

    void Awake()
    {
        SessionState.actionTypeStream.Subscribe(actionType =>
        {
            switch (actionType)
            {
                case (LabAction.ActionType.pipette):
                    if (DilutionDisplay != null)
                    {
                        Destroy(DilutionDisplay);
                    }
                    if (TransferDisplay != null)
                    {
                        Destroy(TransferDisplay);
                    }
                    SampleDisplay.SetActive(true);
                    break;
                case (LabAction.ActionType.transfer):
                    SampleDisplay.SetActive(false);
                    if(DilutionDisplay != null)
                    {
                        Destroy(DilutionDisplay);
                    }    
                    TransferDisplay = Instantiate(TransferDisplayPrefab, ActionDisplay);
                    break;
                case (LabAction.ActionType.dilution):
                    SampleDisplay.SetActive(false);
                    if(TransferDisplay != null)
                    {
                        Destroy(TransferDisplay);
                    }
                    DilutionDisplay = Instantiate(DilutionDisplayPrefab, ActionDisplay);
                    break;
            }
        }).AddTo(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MainCanvasViewController : MonoBehaviour
{
    public GameObject SampleDisplay;
    public GameObject TransferDisplay;
    public GameObject DilutionDisplay;
  
    // Start is called before the first frame update
    void Start()
    {
        SessionState.actionTypeStream.Subscribe(actionType =>
        {
            switch (actionType)
            {
                case (LabAction.ActionType.pipette):
                    TransferDisplay.SetActive(false);
                    DilutionDisplay.SetActive(false);
                    SampleDisplay.SetActive(true);
                    break;
                case (LabAction.ActionType.transfer):
                    SampleDisplay.SetActive(false);
                    DilutionDisplay.SetActive(false);
                    TransferDisplay.SetActive(true);
                    break;
                case (LabAction.ActionType.dilution):
                    SampleDisplay.SetActive(false);
                    TransferDisplay.SetActive(false);
                    DilutionDisplay.SetActive(true);
                    break;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

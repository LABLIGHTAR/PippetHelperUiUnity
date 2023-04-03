using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MainCanvasViewController : MonoBehaviour
{
    public GameObject SampleDisplay;
    public GameObject TransferDisplay;
  
    // Start is called before the first frame update
    void Start()
    {
        SessionState.actionTypeStream.Subscribe(actionType =>
        {
            switch (actionType)
            {
                case (LabAction.ActionType.pipette):
                    TransferDisplay.SetActive(false);
                    SampleDisplay.SetActive(true);
                    break;
                case (LabAction.ActionType.transfer):
                    SampleDisplay.SetActive(false);
                    TransferDisplay.SetActive(true);
                    break;
                case (LabAction.ActionType.dilution):
                    break;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

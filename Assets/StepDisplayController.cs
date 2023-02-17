using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StepDisplayController : MonoBehaviour
{
    private int currentStep = 1;
    private TextMeshProUGUI stepDisplayText;
    // Start is called before the first frame update
    void Start()
    {
        stepDisplayText = this.GetComponent<TextMeshProUGUI>();
        stepDisplayText.text = "Step " + currentStep + "/" + SessionState.steps.Count + 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentStep != SessionState.step + 1)
        {
            currentStep = SessionState.step + 1;
            stepDisplayText.text = "Step " + "/" + SessionState.steps.Count + 1;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class StepViewController : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button newStepButton;
    public Transform stepDisplay;

    private TextMeshProUGUI stepDisplayText;
    // Start is called before the first frame update
    void Start()
    {
        stepDisplayText = stepDisplay.GetComponent<TextMeshProUGUI>();

        //add button events
        previousButton.onClick.AddListener(delegate
        {
            SessionState.SetStep(SessionState.Step - 1);
        });

        nextButton.onClick.AddListener(delegate
        {
            if (SessionState.Step + 1 < SessionState.Steps.Count)
            {
                SessionState.SetStep(SessionState.Step + 1);
            }
        });

        newStepButton.onClick.AddListener(delegate
        {
            SessionState.AddNewStep();
            SessionState.SetStep(SessionState.Step + 1);
        });

        //subscribe to datastream
        SessionState.stepStream.Subscribe(_ => UpdateVisualState());

        UpdateVisualState();
    }

    void UpdateVisualState()
    {
        stepDisplayText.text = "Step " + (SessionState.Step + 1) + "/" + SessionState.Steps.Count;

        if (SessionState.Step == 0)
        {
            previousButton.enabled = false;
        }
        else
        {
            previousButton.enabled = true;
        }
        if (SessionState.Step == SessionState.Steps.Count)
        {
            nextButton.enabled = false;
        }
        else
        {
            nextButton.enabled = true;
        }
    }
}

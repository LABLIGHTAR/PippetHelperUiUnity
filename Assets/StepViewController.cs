using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepViewController : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button newStepButton;
    public Transform stepDisplay;

    private int currentStep = 1;
    private int numberOfSteps = 1;
    private TextMeshProUGUI stepDisplayText;
    // Start is called before the first frame update
    void Start()
    {
        stepDisplayText = stepDisplay.GetComponent<TextMeshProUGUI>();
        Debug.Log("Number of steps: " + SessionState.steps.Count);
        stepDisplayText.text = "Step " + currentStep + "/" + SessionState.steps.Count;
        previousButton.enabled = false;

        //add button events
        previousButton.onClick.AddListener(delegate
        {
            SessionState.step = SessionState.step - 1;
            Debug.Log(SessionState.steps[SessionState.step].wells["A1"].liquids[0].name);
        });

        nextButton.onClick.AddListener(delegate
        {
            if (SessionState.step + 1 < SessionState.steps.Count)
            {
                SessionState.step = SessionState.step + 1;
                Debug.Log(SessionState.steps[SessionState.step].wells["A1"].liquids[0].name);
            }
        });

        newStepButton.onClick.AddListener(delegate
        {
            SessionState.AddNewStep();
            SessionState.step = SessionState.step + 1;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (currentStep != SessionState.step + 1 || numberOfSteps != SessionState.steps.Count)
        {
            currentStep = SessionState.step + 1;
            numberOfSteps = SessionState.steps.Count;
            stepDisplayText.text = "Step " + currentStep + "/" + SessionState.steps.Count;

            if (SessionState.step == 0)
            {
                previousButton.enabled = false;
            }
            else
            {
                previousButton.enabled = true;
            }
            if(SessionState.step == SessionState.steps.Count)
            {
                nextButton.enabled = false;
            }
            else
            {
                nextButton.enabled = true;
            }
        }
    }
}

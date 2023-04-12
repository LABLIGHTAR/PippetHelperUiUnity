using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class StepViewController : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button newStepButton;
    public Button removeStepButton;
    public Transform stepDisplay;

    private TextMeshProUGUI stepDisplayText;
    // Start is called before the first frame update
    void Start()
    {
        stepDisplayText = stepDisplay.GetComponent<TextMeshProUGUI>();

        //add button events
        previousButton.onClick.AddListener(delegate
        {
            SessionState.SetActiveStep(SessionState.ActiveStep - 1);
        });

        nextButton.onClick.AddListener(delegate
        {
            if (SessionState.ActiveStep + 1 < SessionState.Steps.Count)
            {
                SessionState.SetActiveStep(SessionState.ActiveStep + 1);
            }
        });

        newStepButton.onClick.AddListener(delegate
        {
            SessionState.AddNewStep();
        });

        removeStepButton.onClick.AddListener(delegate { 
            if(SessionState.Steps.Count > 1) {
                SessionState.RemoveCurrentStep();
                if(SessionState.ActiveStep > 0)
                {
                    SessionState.SetActiveStep(SessionState.ActiveStep - 1);
                }
                else
                {
                    SessionState.SetActiveStep(0);
                }
            }
        });

        //subscribe to datastream
        SessionState.stepStream.Subscribe(_ => UpdateVisualState()).AddTo(this);

        UpdateVisualState();
    }

    void UpdateVisualState()
    {
        stepDisplayText.text = "Step " + (SessionState.ActiveStep + 1) + "/" + SessionState.Steps.Count;

        if (SessionState.ActiveStep == 0)
        {
            previousButton.enabled = false;
        }
        else
        {
            previousButton.enabled = true;
        }
        if (SessionState.ActiveStep == SessionState.Steps.Count)
        {
            nextButton.enabled = false;
        }
        else
        {
            nextButton.enabled = true;
        }
    }
}

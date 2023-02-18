using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WellViewController : MonoBehaviour
{
    public string name;
    public List<SpriteRenderer> liquidIndicators;

    private int liquidCount;
    // Start is called before the first frame update
    void Start()
    {
        SessionState.stepStream.Subscribe(_ => LoadVisualState());

        SessionState.clickStream.Subscribe(selectedObject =>
        {
            if(selectedObject.name == name && !SessionState.FormActive)
            {
                SessionState.AddActiveLiquidToWell(name);
                UpdateVisualState();
            }
        });
    }

    //called when well is clicked
    void UpdateVisualState()
    {
        Debug.Log("Updating visual state");
        if (SessionState.Steps != null & SessionState.Steps[SessionState.Step] != null)
        {
            var currentStep = SessionState.Steps[SessionState.Step];
            //check if this well has liquids in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                if (currentStep.wells[name].liquids.Count != liquidCount)
                {
                    liquidCount = currentStep.wells[name].liquids.Count;
                    for (int i = 0; i < liquidCount; i++)
                    {
                        liquidIndicators[i].gameObject.SetActive(true);
                        liquidIndicators[i].color = currentStep.wells[name].liquids[i].color;
                    }
                }
            }
        }
    }

    //called when step is changed
    void LoadVisualState()
    {
        Debug.Log("Loading visual state");
        if (SessionState.Steps != null & SessionState.Steps[SessionState.Step] != null)
        {
            liquidCount = 0;
            var currentStep = SessionState.Steps[SessionState.Step];
            //check if this well has liquids in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                liquidCount = currentStep.wells[name].liquids.Count;
                for (int i = 0; i < liquidCount; i++)
                {
                    liquidIndicators[i].gameObject.SetActive(true);
                    liquidIndicators[i].color = currentStep.wells[name].liquids[i].color;
                }
            }
            else
            {
                foreach (SpriteRenderer sr in liquidIndicators)
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }
    }
}

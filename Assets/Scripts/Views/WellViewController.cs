using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WellViewController : MonoBehaviour
{
    public string name;
    public List<SpriteRenderer> liquidIndicators;

    private int liquidCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SessionState.steps != null & SessionState.step != null & SessionState.steps[SessionState.step] != null)
        {
            var currentStep = SessionState.steps[SessionState.step];
            //check if this well has liquids in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                if(currentStep.wells[name].liquids.Count != liquidCount)
                {
                    liquidCount = currentStep.wells[name].liquids.Count;
                    for (int i = 0; i < liquidCount; i++)
                    {
                        liquidIndicators[i].gameObject.SetActive(true);
                        liquidIndicators[i].color = currentStep.wells[name].liquids[i].color;
                    }
                }
            }
            else
            {
                foreach(SpriteRenderer sr in liquidIndicators)
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }
    }
}

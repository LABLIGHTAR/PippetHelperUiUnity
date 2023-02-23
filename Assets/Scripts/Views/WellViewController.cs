using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WellViewController : MonoBehaviour
{
    public string name;
    public List<SpriteRenderer> liquidIndicators;
    public WellViewController NextInRow;
    public WellViewController NextInCol;

    private int liquidCount;
    // Start is called before the first frame update
    void Start()
    {
        SessionState.stepStream.Subscribe(_ => LoadVisualState());

        SessionState.leftClickStream.Subscribe(selectedObject =>
        {
            if(selectedObject.name == name && !SessionState.FormActive && SessionState.ActiveLiquid != null)
            {
                if(SessionState.ActiveTool.name == "micropipette")
                {
                    if (SessionState.AddActiveLiquidToWell(name))
                    {
                        UpdateVisualState();
                    }
                }
                else
                {
                    AddLiquidMultichannel(SessionState.ActiveTool.numChannels);
                }
            }
        });


        SessionState.rightClickStream.Subscribe(selectedObject =>
        {
            if (selectedObject.name == name && !SessionState.FormActive)
            {
                if (SessionState.RemoveActiveLiquidFromWell(name))
                {
                    UpdateVisualState();
                }
            }
        });
    }

    void AddLiquidMultichannel(int numChannels)
    {
        if (SessionState.ActiveTool.orientation == "Horizontal")
        {
            if ((Int32.Parse(name.Substring(1)) - 1) + numChannels > 12)
            {
                return;
            }
        }
        else if (SessionState.ActiveTool.orientation == "Vertical")
        {
            if (((int)name[0] % 32) - 1 + numChannels > 8)
            {
                return;
            }
        }

        if (SessionState.AddActiveLiquidToWell(name))
        {
            UpdateVisualState();
        }

        numChannels--;

        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Horizontal" && NextInRow != null)
        {
            NextInRow.AddLiquidMultichannel(numChannels);
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Vertical" && NextInCol != null)
        {
            NextInCol.AddLiquidMultichannel(numChannels);
        }
    }

    //called when well is clicked
    void UpdateVisualState()
    {
        if (SessionState.Steps != null & SessionState.Steps[SessionState.Step] != null)
        {
            var currentStep = SessionState.Steps[SessionState.Step];
            //check if this well has liquids in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                if (currentStep.wells[name].liquids.Count != liquidCount)
                {
                    liquidCount = currentStep.wells[name].liquids.Count;

                    for (int i = 0; i < 3; i++)
                    {
                        if (i + 1 <= liquidCount)
                        {
                            liquidIndicators[i].gameObject.SetActive(true);
                            liquidIndicators[i].color = currentStep.wells[name].liquids[i].color;
                        }
                        else
                        {
                            liquidIndicators[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    //called when step is changed
    void LoadVisualState()
    {
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

    bool ActivateHighlight(int numChannels)
    {
        if(SessionState.ActiveTool.orientation == "Horizontal")
        {
            if ((Int32.Parse(name.Substring(1)) - 1) + numChannels > 12)
            {
                return false;
            }
        }
        else if(SessionState.ActiveTool.orientation == "Vertical")
        {
            if (((int)name[0] % 32) - 1 + numChannels > 8)
            {
                return false;
            }
        }

        numChannels--;

        if(numChannels > 0 && SessionState.ActiveTool.orientation == "Horizontal" && NextInRow != null)
        {
            if(!NextInRow.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if(numChannels > 0 && SessionState.ActiveTool.orientation == "Vertical" && NextInCol != null)
        {
            if(!NextInCol.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }

        if (this.liquidCount < 3 && SessionState.ActiveLiquid != null)
        {
            if (SessionState.Steps[SessionState.Step].wells.ContainsKey(name) && SessionState.Steps[SessionState.Step].wells[name].liquids.Contains(SessionState.ActiveLiquid))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
            this.liquidIndicators[liquidCount].gameObject.SetActive(true);
            this.liquidIndicators[liquidCount].color = SessionState.ActiveLiquid.color;
            return true;
        }
        DeactivateHighlight(SessionState.ActiveTool.numChannels);
        return false;
    }

    void DeactivateHighlight(int numChannels)
    {
        if (this.liquidCount < 3 && SessionState.ActiveLiquid != null)
        {
            this.liquidIndicators[liquidCount].gameObject.SetActive(false);
        }

        numChannels++;

        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Horizontal" && NextInRow != null)
        {
            NextInRow.DeactivateHighlight(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Vertical" && NextInCol != null)
        {
            NextInCol.DeactivateHighlight(numChannels);
        }
    }

    void OnMouseEnter()
    {
        if(SessionState.ActiveTool.name == "micropipette")
        {
            ActivateHighlight(1);
        }
        else if(SessionState.ActiveTool.name == "multichannel")
        {
            ActivateHighlight(SessionState.ActiveTool.numChannels);
        }
    }

    void OnMouseExit()
    {
        if (SessionState.ActiveTool.name == "micropipette")
        {
            DeactivateHighlight(0);
        }
        else if (SessionState.ActiveTool.name == "multichannel")
        {
            DeactivateHighlight(0);
        }
    }
}

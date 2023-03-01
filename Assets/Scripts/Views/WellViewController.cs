using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class WellViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
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
        ProcedureLoader.procedureStream.Subscribe(_ => UpdateVisualState());

        SessionState.liquidRemovedStream.Subscribe(well =>
        {
            if (well == name)
            {
                UpdateVisualState();
            }
        });
    }


    // Pointer events
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!SessionState.FormActive)
        {
            if (SessionState.ActiveTool.name == "micropipette")
            {
                ActivateHighlight(1);
            }
            else if (SessionState.ActiveTool.name == "multichannel")
            {
                ActivateHighlight(SessionState.ActiveTool.numChannels);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!SessionState.FormActive)
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!SessionState.FormActive)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!SessionState.FormActive && SessionState.ActiveLiquid != null)
                {
                    if (SessionState.ActiveTool.name == "micropipette")
                    {
                        if (SessionState.AddActiveLiquidToWell(name, false, false, false))
                        {
                            UpdateVisualState();
                        }
                    }
                    else
                    {
                        AddLiquidMultichannel(SessionState.ActiveTool.numChannels);
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!SessionState.FormActive & SessionState.RemoveActiveLiquidFromWell(name))
                {
                    UpdateVisualState();
                }
            }
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

    /// <summary>
    /// Recursivly adds liquid to all wells in multichannel if possible
    /// Only ever called on multichannel clicks
    /// </summary>
    /// <param name="numChannels"></param>
    void AddLiquidMultichannel(int numChannels)
    {
        //if number of channels is greater than the number of wells in the given orientation return
        if (SessionState.ActiveTool.orientation == "Row")
        {
            if ((Int32.Parse(name.Substring(1)) - 1) + numChannels > 12)
            {
                return;
            }
        }
        else if (SessionState.ActiveTool.orientation == "Column")
        {
            if (((int)name[0] % 32) - 1 + numChannels > 8)
            {
                return;
            }
        }

        //determine if this is the first or last well in the group
        bool isStart = (numChannels == SessionState.ActiveTool.numChannels);
        bool isEnd = (numChannels == 1);

        //add liquid to clicked well
        if (SessionState.AddActiveLiquidToWell(name, true, isStart, isEnd));
        {
            UpdateVisualState();
        }

        numChannels--;

        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.AddLiquidMultichannel(numChannels);
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.AddLiquidMultichannel(numChannels);
        }
    }

    /// <summary>
    /// activates well highlights for active tool
    /// </summary>
    /// <param name="numChannels"></param>
    /// <returns></returns>
    bool ActivateHighlight(int numChannels)
    {
        //if number of channels is greater than the number of wells in the given orientation return
        if (SessionState.ActiveTool.orientation == "Row")
        {
            if ((Int32.Parse(name.Substring(1)) - 1) + numChannels > 12)
            {
                return false;
            }
        }
        else if(SessionState.ActiveTool.orientation == "Column")
        {
            if (((int)name[0] % 32) - 1 + numChannels > 8)
            {
                return false;
            }
        }

        numChannels--;

        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInRow.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if(numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInCol.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }

        if (this.liquidCount < 3 && SessionState.ActiveLiquid != null)
        {
            //if this well already contains the active deactivate all highlights
            if (SessionState.Steps[SessionState.Step].wells.ContainsKey(name) && SessionState.Steps[SessionState.Step].wells[name].liquids.Contains(SessionState.ActiveLiquid))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
            //if we make it through all the above checks activate this wells highlight
            this.liquidIndicators[liquidCount].gameObject.SetActive(true);
            this.liquidIndicators[liquidCount].color = SessionState.ActiveLiquid.color;
            return true;
        }
        //if this well is full or we have no active well deactivate all highlights
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

        //if we have more channels to deactivate make recursive calls
        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.DeactivateHighlight(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.DeactivateHighlight(numChannels);
        }
    }
}

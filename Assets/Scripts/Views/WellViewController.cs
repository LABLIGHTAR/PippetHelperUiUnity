using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class WellViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public string name;
    public List<SpriteRenderer> SampleIndicators;
    public WellViewController NextInRow;
    public WellViewController NextInCol;

    private int SampleCount;
    // Start is called before the first frame update
    void Start()
    {
        SessionState.stepStream.Subscribe(_ => LoadVisualState());
        
        ProcedureLoader.procedureStream.Subscribe(_ => LoadVisualState());

        SessionState.SampleRemovedStream.Subscribe(well =>
        {
            if (well == name)
            {
                UpdateVisualState();
            }
        });

        //subscribe to update indicators if sample is edited
        SessionState.editedSampleStream.Subscribe(editedSample =>
        {
            var currentStep = SessionState.Steps[SessionState.Step];
            if (currentStep.wells.ContainsKey(name))
            {
                //if this well contains the edited sample
                if (currentStep.wells[name].Samples.Keys.Where(sample => sample.name == editedSample.Item2).FirstOrDefault() != null)
                {
                    int index = -1;
                    SampleIndicators[0].gameObject.SetActive(false);
                    SampleIndicators[1].gameObject.SetActive(false);
                    SampleIndicators[2].gameObject.SetActive(false);

                    //update indicator colors
                    foreach (var sample in currentStep.wells[name].Samples)
                    {
                        index++;
                        SampleIndicators[index].gameObject.SetActive(true);
                        SampleIndicators[index].color = sample.Key.color;
                    }
                }
            }
        });
    }


    // Highlight and update focused well on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!SessionState.FormActive && SessionState.ActiveTool != null)
        {
            if (SessionState.ActiveTool.name == "micropipette")
            {
                ActivateHighlight(1);
            }
            else if (SessionState.ActiveTool.name == "multichannel")
            {
                ActivateHighlight(SessionState.ActiveTool.numChannels);
            }
            SessionState.SetFocusedWell(name);
        }
    }

    //remove highlight on hover exit
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!SessionState.FormActive & SessionState.ActiveTool != null)
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

    //add sample to well and update focused well on click
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!SessionState.FormActive)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (SessionState.ActiveTool != null && SessionState.ActiveSample != null)
                {
                    if (SessionState.ActiveTool.name == "micropipette")
                    {
                        if (SessionState.AddActiveSampleToWell(name, false, false, false))
                        {
                            UpdateVisualState();
                        }
                    }
                    else
                    {
                        AddSampleMultichannel(SessionState.ActiveTool.numChannels);
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!SessionState.FormActive & SessionState.RemoveActiveSampleFromWell(name, SessionState.Steps[SessionState.Step]))
                {
                    UpdateVisualState();
                }
            }
            if (SessionState.Steps[SessionState.Step].wells.ContainsKey(name))
            {
                SessionState.SetFocusedWell(name);
            }
        }
    }

    //called when well is clicked
    void UpdateVisualState()
    {
        if (SessionState.Steps != null & SessionState.Steps[SessionState.Step] != null)
        {
            var currentStep = SessionState.Steps[SessionState.Step];
            //check if this well has Samples in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                if (currentStep.wells[name].Samples.Count != SampleCount)
                {
                    SampleCount = currentStep.wells[name].Samples.Count;

                    int index = -1;
                    SampleIndicators[0].gameObject.SetActive(false);
                    SampleIndicators[1].gameObject.SetActive(false);
                    SampleIndicators[2].gameObject.SetActive(false);

                    //update indicator colors
                    foreach (var sample in currentStep.wells[name].Samples)
                    {
                        index++;
                        SampleIndicators[index].gameObject.SetActive(true);
                        SampleIndicators[index].color = sample.Key.color;
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
            foreach (SpriteRenderer sr in SampleIndicators)
            {
                sr.gameObject.SetActive(false);
            }

            SampleCount = 0;
            var currentStep = SessionState.Steps[SessionState.Step];
            //check if this well has Samples in it, if it does render them
            if (currentStep.wells.ContainsKey(name))
            {
                SampleCount = currentStep.wells[name].Samples.Count;
                
                int index = -1;
                SampleIndicators[0].gameObject.SetActive(false);
                SampleIndicators[1].gameObject.SetActive(false);
                SampleIndicators[2].gameObject.SetActive(false);

                //update indicator colors
                foreach (var sample in currentStep.wells[name].Samples)
                {
                    index++;
                    SampleIndicators[index].gameObject.SetActive(true);
                    SampleIndicators[index].color = sample.Key.color;
                }
            }
        }
    }

    /// <summary>
    /// Recursivly adds Sample to all wells in multichannel if possible
    /// Only ever called on multichannel clicks
    /// </summary>
    /// <param name="numChannels"></param>
    void AddSampleMultichannel(int numChannels)
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

        //add Sample to clicked well
        if (SessionState.AddActiveSampleToWell(name, true, isStart, isEnd))
        {
            UpdateVisualState();
        }

        numChannels--;

        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.AddSampleMultichannel(numChannels);
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.AddSampleMultichannel(numChannels);
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

        if (this.SampleCount < 3 && SessionState.ActiveSample != null)
        {
            //if this well already contains the active deactivate all highlights
            if (SessionState.Steps[SessionState.Step].wells.ContainsKey(name) && SessionState.Steps[SessionState.Step].wells[name].Samples.ContainsKey(SessionState.ActiveSample))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
            //if we make it through all the above checks activate this wells highlight
            this.SampleIndicators[SampleCount].gameObject.SetActive(true);
            this.SampleIndicators[SampleCount].color = SessionState.ActiveSample.color;
            return true;
        }
        //if this well is full or we have no active well deactivate all highlights
        DeactivateHighlight(SessionState.ActiveTool.numChannels);
        return false;
    }


    void DeactivateHighlight(int numChannels)
    {
        if (this.SampleCount < 3 && SessionState.ActiveSample != null)
        {
            this.SampleIndicators[SampleCount].gameObject.SetActive(false);
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

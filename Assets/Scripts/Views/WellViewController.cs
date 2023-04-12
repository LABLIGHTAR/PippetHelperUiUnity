using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class WellViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public string wellId;
    public int plateId;
    public List<SpriteRenderer> SampleIndicators;
    public WellViewController NextInRow;
    public WellViewController NextInCol;

    public SpriteRenderer SelectionSprite;
    public bool selected;

    private int SampleCount;

    private int maxRowNum;
    private int maxColNum;

    void Awake()
    {
        ProcedureLoader.procedureStream.Subscribe(_ => LoadVisualState());
        SelectionManager.Instance.AvailableWells.Add(this);

        if(this.transform.parent.transform.childCount == 96)
        {
            maxRowNum = 12;
            maxColNum = 8;
        }
        else if(this.transform.parent.transform.childCount == 384)
        {
            maxRowNum = 24;
            maxColNum = 16;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        wellId = gameObject.name;
        plateId = transform.parent.transform.parent.GetComponent<WellPlateViewController>().id;

        SessionState.stepStream.Subscribe(_ => LoadVisualState());

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
            if (SessionState.CurrentStep.materials[plateId].ContainsWell(wellId))
            {
                //if this well contains the edited sample
                if (SessionState.CurrentStep.materials[plateId].GetWell(wellId).Samples.Keys.Where(sample => sample.sampleName == editedSample.Item2).FirstOrDefault() != null)
                {
                    UpdateSampleIndicators();
                }
            }
        });

        SessionState.actionStatusStream.Subscribe(status =>
        {
            switch(status)
            {
                case LabAction.ActionStatus.selectingSource:
                    if (!selected)
                        SelectionSprite.color = Color.red;
                    break;
                case LabAction.ActionStatus.selectingTarget:
                    if (!selected)
                        SelectionSprite.color = Color.green;
                    break;
                case LabAction.ActionStatus.submitted:
                    selected = false;
                    SessionState.CurrentStep.materials[plateId].GetWell(wellId).selected = false;
                    OnDeselected(SessionState.ActiveTool.numChannels);
                    break;
            }
        });

        SessionState.focusedActionStream.Subscribe(action => 
        {
            if (action != null)
                HighlightAction(action);
            else
                OnDeselected(SessionState.ActiveTool.numChannels);
        });
    }


    // Highlight and update focused well on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!SessionState.FormActive && !SessionState.SelectionActive && SessionState.ActiveTool != null)
        {
            if(SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                ActivateHighlight(SessionState.ActiveTool.numChannels);
            }
            else if((SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource || SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget))
            {
                OnSelected(SessionState.ActiveTool.numChannels);
            }
            SessionState.SetFocusedWell(wellId, plateId);
        }
    }

    //remove highlight on hover exit
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!SessionState.FormActive && !SessionState.SelectionActive && SessionState.ActiveTool != null)
        {
            if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                DeactivateHighlight(0);
            }
            else if(SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource || SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
            {
                OnDeselected(SessionState.ActiveTool.numChannels);
            }
        }
    }

    //add sample to well and update focused well on click
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!SessionState.FormActive && !SessionState.SelectionActive)
        {
            if(SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    if (SessionState.RemoveActiveSampleFromWell(wellId, plateId, SessionState.CurrentStep))
                    {
                        UpdateVisualState();
                    } 
                }
            }
            else if(SessionState.ActiveActionType == LabAction.ActionType.transfer)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    if(OnSelected(SessionState.ActiveTool.numChannels))
                    {
                        OnSelectedAndClicked(SessionState.ActiveTool.numChannels);
                        SessionState.SetSelectedWells(plateId);
                    }
                }
            }
            else if(SessionState.ActiveActionType == LabAction.ActionType.dilution && SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
            {
                selected = true;
                SessionState.CurrentStep.materials[plateId].GetWell(wellId).selected = true;
                OnSelected(SessionState.ActiveTool.numChannels);
                SessionState.SetSelectedWells(plateId);
            }
            if (SessionState.CurrentStep.materials[plateId].ContainsWell(wellId))
            {
                SessionState.SetFocusedWell(wellId, plateId);
            }
        }
    }

    //called when well is clicked
    public void UpdateVisualState()
    {
        if (SessionState.Steps != null & SessionState.CurrentStep != null)
        {
            var currentStep = SessionState.CurrentStep;
            //check if this well has Samples in it, if it does render them
            if (currentStep.materials[plateId].ContainsWell(wellId))
            {
                if (currentStep.materials[plateId].GetWell(wellId).Samples.Count != SampleCount)
                {
                    SampleCount = currentStep.materials[plateId].GetWell(wellId).Samples.Count;
                    UpdateSampleIndicators();
                }
            }
        }
    }

    //called when step is changed
    public void LoadVisualState()
    {
        if (SessionState.Steps != null & SessionState.CurrentStep != null)
        {
            foreach (SpriteRenderer sr in SampleIndicators)
            {
                sr.gameObject.SetActive(false);
            }

            SampleCount = 0;
            var currentStep = SessionState.CurrentStep;
            //check if this well has Samples in it, if it does render them
            if (currentStep.materials[plateId].ContainsWell(wellId))
            {
                SampleCount = currentStep.materials[plateId].GetWell(wellId).Samples.Count;
                UpdateSampleIndicators();
            }
        }
    }

    /// <summary>
    /// Recursivly adds Sample to all wells in multichannel if possible
    /// Only ever called on multichannel clicks
    /// </summary>
    /// <param name="numChannels"></param>
    public void AddSampleMultichannel(int numChannels)
    {
        //if number of channels is greater than the number of wells in the given orientation return
        if (SessionState.ActiveTool.orientation == "Row")
        {
            if (int.Parse(wellId.Substring(1)) - 1 + numChannels > maxRowNum)
            {
                return;
            }
        }
        else if (SessionState.ActiveTool.orientation == "Column")
        {
            if ((wellId[0] % 32) - 1 + numChannels > maxColNum)
            {
                return;
            }
        }

        //determine if this is the first or last well in the group
        bool isStart = (numChannels == SessionState.ActiveTool.numChannels);
        bool isEnd = (numChannels == 1);

        //add Sample to clicked well
        if (SessionState.AddActiveSampleToWell(wellId, plateId, true, isStart, isEnd))
        {
            UpdateVisualState();
        }

        numChannels--;

        if(maxRowNum == 12)
        {
            AddMultichannel96(numChannels);
        }
        else
        {
            AddMultichannel384(numChannels);
        }
    }

    void AddMultichannel96(int numChannels)
    {
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

    void AddMultichannel384(int numChannels)
    {
        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
        {
            NextInRow.NextInRow.AddSampleMultichannel(numChannels);
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
        {
            NextInCol.NextInCol.AddSampleMultichannel(numChannels);
        }
    }

    void UpdateSampleIndicators()
    {
        SampleIndicators[0].gameObject.SetActive(false);
        SampleIndicators[1].gameObject.SetActive(false);
        SampleIndicators[2].gameObject.SetActive(false);

        int index = -1;
        //update indicator colors
        foreach (var sample in SessionState.CurrentStep.materials[plateId].GetWell(wellId).Samples)
        {
            index++;
            SampleIndicators[index].gameObject.SetActive(true);
            SampleIndicators[index].color = sample.Key.color;
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
            if ((Int32.Parse(wellId.Substring(1)) - 1) + numChannels > maxRowNum)
            {
                return false;
            }
        }
        else if(SessionState.ActiveTool.orientation == "Column")
        {
            if (((int)wellId[0] % 32) - 1 + numChannels > maxColNum)
            {
                return false;
            }
        }

        numChannels--;

        if(maxRowNum == 12)
        {
            if (!Highlight96(numChannels))
            {
                return false;
            }
        }
        else
        {
            if(!Highlight384(numChannels))
            {
                return false;
            }
        }

        if (this.SampleCount < 3 && SessionState.ActiveSample != null)
        {
            //if this well already contains the active deactivate all highlights
            if (SessionState.CurrentStep.materials[plateId].ContainsWell(wellId) && SessionState.CurrentStep.materials[plateId].GetWell(wellId).Samples.ContainsKey(SessionState.ActiveSample))
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

    bool Highlight96(int numChannels)
    {
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
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInCol.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        return true;
    }

    bool Highlight384(int numChannels)
    {
        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInRow.NextInRow.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInCol.NextInCol.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        return true;
    }

    void DeactivateHighlight(int numChannels)
    {
        if (this.SampleCount < 3 && SessionState.ActiveSample != null)
        {
            this.SampleIndicators[SampleCount].gameObject.SetActive(false);
        }

        numChannels++;

        if(maxRowNum == 12)
        {
            UnHighlight96(numChannels);
        }
        else
        {
            UnHighlight384(numChannels);
        }
    }

    void UnHighlight96(int numChannels)
    {
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

    void UnHighlight384(int numChannels)
    {
        //if we have more channels to deactivate make recursive calls
        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
        {
            NextInRow.NextInRow.DeactivateHighlight(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
        {
            NextInCol.NextInCol.DeactivateHighlight(numChannels);
        }
    }

    public bool OnSelected(int numChannels)
    {
        if(SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            ActivateHighlight(1);
            SelectionSprite.color = Color.blue;
            SelectionSprite.gameObject.SetActive(true);
            return true;
        }
        else
        {
            numChannels--;

            if (maxRowNum == 12)
            {
                if (!Select96(numChannels))
                {
                    return false;
                }
            }
            else
            {
                if (!Select384(numChannels))
                {
                    return false;
                }
            }

            if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource && !selected) 
            {
                SelectionSprite.color = Color.red;
            }
            else if(SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget && !selected)
            {
                SelectionSprite.color = Color.green;
            }
            SelectionSprite.gameObject.SetActive(true);
            return true;
        }
    }

    bool Select96(int numChannels)
    {
        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInRow.OnSelected(numChannels))
            {
                OnDeselected(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInCol.OnSelected(numChannels))
            {
                OnDeselected(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        return true;
    }

    bool Select384(int numChannels)
    {
        //if there are more channels to add call this method on the next well in the orientation
        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInRow.NextInRow.OnSelected(numChannels))
            {
                OnDeselected(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
        {
            //if we cannot activate the highlight in the next well deactivate all highlights
            if (!NextInCol.NextInCol.OnSelected(numChannels))
            {
                OnDeselected(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        return true;
    }

    void OnSelectedAndClicked(int numChannels)
    {
        selected = true;
        SessionState.CurrentStep.materials[plateId].GetWell(wellId).selected = true;

        numChannels--;

        if(numChannels > 0)
        {
            if (maxRowNum == 12)
            {
                if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
                {
                    NextInRow.OnSelectedAndClicked(numChannels);
                }
                else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
                {
                    NextInCol.OnSelectedAndClicked(numChannels);
                }
            }
            else
            {
                if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
                {
                    NextInRow.NextInRow.OnSelectedAndClicked(numChannels);
                }
                else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
                {
                    NextInCol.NextInCol.OnSelectedAndClicked(numChannels);
                }
            }
        }
    }

    public void OnDeselected(int numChannels)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            SelectionSprite.color = Color.blue;
            SelectionSprite.gameObject.SetActive(false);
            DeactivateHighlight(1);
        }
        else
        {
            if(!selected)
            {
                SelectionSprite.color = Color.blue;
                SelectionSprite.gameObject.SetActive(false);
                
                numChannels++;

                if (maxRowNum == 12)
                {
                    DeSelect96(numChannels);
                }
                else
                {
                    DeSelect384(numChannels);
                }
            }    
        }
    }

    void DeSelect96(int numChannels)
    {
        //if we have more channels to deactivate make recursive calls
        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.OnDeselected(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.OnDeselected(numChannels);
        }
    }

    void DeSelect384(int numChannels)
    {
        //if we have more channels to deactivate make recursive calls
        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null && NextInRow.NextInRow != null)
        {
            NextInRow.NextInRow.OnDeselected(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null && NextInCol.NextInCol != null)
        {
            NextInCol.NextInCol.OnDeselected(numChannels);
        }
    }

    private void HighlightAction(LabAction action)
    {
        if(plateId.ToString() == action.source.matID && wellId == action.source.matSubID)
        {
            this.SelectionSprite.color = action.source.color;
            this.SelectionSprite.gameObject.SetActive(true);
        }
        else if(plateId.ToString() == action.target.matID && wellId == action.target.matSubID)
        {
            this.SelectionSprite.color = action.target.color;
            this.SelectionSprite.gameObject.SetActive(true);
        }
    }
}

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

    public int SampleCount;

    public int maxRowNum;
    public int maxColNum;

    // Highlight and update focused well on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!SessionState.FormActive && !SessionState.SelectionActive && SessionState.ActiveTool != null)
        {
            if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                ActivateHighlight(SessionState.ActiveTool.numChannels);
            }
            else if ((SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource || SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget))
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
            else if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource || SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
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
            if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    if (SessionState.CurrentStep.RemoveActiveSampleFromWell(wellId, plateId))
                    {
                        UpdateVisualState();
                    }
                }
            }
            else if (SessionState.ActiveActionType == LabAction.ActionType.transfer)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    if (OnSelected(SessionState.ActiveTool.numChannels))
                    {
                        OnSelectedAndClicked(SessionState.ActiveTool.numChannels);
                        SessionState.SetSelectedWells(plateId);
                    }
                }
            }
            else if (SessionState.ActiveActionType == LabAction.ActionType.dilution && SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
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
    public virtual void AddSampleMultichannel(int numChannels)
    {
        return;
    }

    public void UpdateSampleIndicators()
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
    public virtual bool ActivateHighlight(int numChannels)
    {
        return false;
    }

    public virtual void DeactivateHighlight(int numChannels)
    {
        return;
    }

    public virtual bool OnSelected(int numChannels)
    {
        return false;
    }

    public virtual void OnSelectedAndClicked(int numChannels)
    {
        return;
    }

    public virtual void OnDeselected(int numChannels)
    {
        return;
    }

    public void HighlightAction(LabAction action)
    {
        if (plateId.ToString() == action.source.matID && wellId == action.source.matSubID)
        {
            this.SelectionSprite.color = action.source.color;
            this.SelectionSprite.gameObject.SetActive(true);
        }
        else if (plateId.ToString() == action.target.matID && wellId == action.target.matSubID)
        {
            this.SelectionSprite.color = action.target.color;
            this.SelectionSprite.gameObject.SetActive(true);
        }
    }

    public virtual WellViewController GetNextInRow()
    {
        return null;
    }

    public virtual WellViewController GetNextInCol()
    {
        return null;
    }
}
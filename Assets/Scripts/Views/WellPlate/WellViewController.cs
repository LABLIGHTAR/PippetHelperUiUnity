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

    public Transform SampleIndicators;
    public GameObject SampleIndicatorPrefab;

    public WellViewController NextInRow;
    public WellViewController NextInCol;

    public SpriteRenderer SelectionSprite;
    public bool selected;

    public int maxRowNum;
    public int maxColNum;

    IDisposable actionAddedSubscription;
    IDisposable actionRemovedSubscription;

    void Awake()
    {
        wellId = gameObject.name;

        NextInRow = GetNextInRow();
        NextInCol = GetNextInCol();

        SelectionManager.Instance.AvailableWells.Add(this);

        ProcedureLoader.procedureStream.Subscribe(_ => LoadSampleIndicators()).AddTo(this);
        
        SessionState.stepStream.Subscribe(_ => 
        {
            SessionState.Materials[plateId].GetWell(wellId);
            LoadSampleIndicators();
            RenewStepSubscriptions();
        }).AddTo(this);

        SessionState.editedSampleStream.Subscribe(samples =>
        {
            if (SessionState.Materials[plateId].GetWell(wellId).ContainsSample(samples.Item2))
            {
                UpdateSampleIndicator(samples.Item1.color, samples.Item2.color);
            }
        }).AddTo(this);

        SessionState.actionStatusStream.Subscribe(status =>
        {
            switch (status)
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
                    SessionState.Materials[plateId].GetWell(wellId).selected = false;
                    OnDeselected(SessionState.ActiveTool.numChannels);
                    break;
            }
        }).AddTo(this);

        SessionState.focusedActionStream.Subscribe(action =>
        {
            if (action != null)
                HighlightAction(action);
            else
                OnDeselected(SessionState.ActiveTool.numChannels);
        }).AddTo(this);

        RenewStepSubscriptions();
    }

    private void RenewStepSubscriptions()
    {
        if (actionAddedSubscription != null)
            actionAddedSubscription.Dispose();
        if (actionRemovedSubscription != null)
            actionRemovedSubscription.Dispose();

        actionAddedSubscription = SessionState.CurrentStep.actionAddedStream.Subscribe(action =>
        {
            if (action.WellIsTarget(plateId.ToString(), wellId) || action.WellIsSource(plateId.ToString(), wellId))
            {
                LoadSampleIndicators();
            }
        }).AddTo(this);

        actionRemovedSubscription = SessionState.CurrentStep.actionRemovedStream.Subscribe(action =>
        {
            if (action.WellIsTarget(plateId.ToString(), wellId) || action.WellIsSource(plateId.ToString(), wellId))
            {
                LoadSampleIndicators();
            }
        }).AddTo(this);
    }

    public virtual void AddSampleMultichannel(int numChannels)
    {
        if (SessionState.ActiveTool.orientation == "Row" && int.Parse(wellId.Substring(1)) - 1 + numChannels > maxRowNum)
        {
            return;
        }
        else if (SessionState.ActiveTool.orientation == "Column" && (wellId[0] % 32) - 1 + numChannels > maxColNum)
        {
            return;   
        }

        bool isStart = (numChannels == SessionState.ActiveTool.numChannels);
        bool isEnd = (numChannels == 1);

        SessionState.CurrentStep.TryAddActiveSampleToWell(wellId, plateId, true, isStart, isEnd);

        numChannels--;

        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.AddSampleMultichannel(numChannels);
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.AddSampleMultichannel(numChannels);
        }
    }

    public void AddTempSampleIndicator()
    {
        RemoveAllSampleIndicators();

        LabAction mostRecentAction = SessionState.TryGetMostRecentAction();
        Well thisWell = SessionState.Materials[plateId].GetWell(wellId);
        float sampleVolume = 0f;
        float wellVolume = 0f;

        if (mostRecentAction != null)
        {
            wellVolume = thisWell.GetVolumeAtAction(mostRecentAction) + SessionState.ActiveTool.volume;
            foreach (Sample sample in thisWell.GetSamples())
            {
                sampleVolume = thisWell.GetSampleVolumeAtAction(sample, mostRecentAction);

                if (sampleVolume > 0)
                {
                    float samplePercent = sampleVolume / wellVolume;
                    var newIndicator = Instantiate(SampleIndicatorPrefab, SampleIndicators);
                    newIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = sample.color;
                    newIndicator.GetComponent<SampleIndicatorViewController>().Resize(samplePercent);
                }
                sampleVolume = 0f;
            }
        }

        var tempIndicator = Instantiate(SampleIndicatorPrefab, SampleIndicators);
        tempIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = SessionState.ActiveSample.color;
        if(wellVolume > 0)
        {
            var tempSamplePercent = SessionState.ActiveTool.volume / wellVolume;
            tempIndicator.GetComponent<SampleIndicatorViewController>().Resize(tempSamplePercent);
        }
        else
        {
            tempIndicator.GetComponent<SampleIndicatorViewController>().Resize(1);
        }
    }

    public void UpdateSampleIndicator(Color oldColor, Color newColor)
    {
        foreach(Transform indicator in SampleIndicators)
        {
            if (indicator.GetChild(0).GetComponent<SpriteRenderer>().color == oldColor)
            {
                indicator.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
            }
        }
    }

    public void LoadSampleIndicators()
    {
        RemoveAllSampleIndicators();

        LabAction mostRecentAction = SessionState.TryGetMostRecentAction();
        Well thisWell = SessionState.Materials[plateId].GetWell(wellId);
        float sampleVolume = 0f;
        float wellVolume = 0f;

        if (mostRecentAction != null)
        {
            wellVolume = thisWell.GetVolumeAtAction(mostRecentAction);
            foreach (Sample sample in thisWell.GetSamples())
            {
                sampleVolume = thisWell.GetSampleVolumeAtAction(sample, mostRecentAction);

                if (sampleVolume > 0)
                {
                    float samplePercent = sampleVolume / wellVolume;
                    var newIndicator = Instantiate(SampleIndicatorPrefab, SampleIndicators);
                    newIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = sample.color;
                    newIndicator.GetComponent<SampleIndicatorViewController>().Resize(samplePercent);
                }
                sampleVolume = 0f;
            }
        }
    }

    public void RemoveAllSampleIndicators()
    {
        foreach(Transform indicator in SampleIndicators)
        {
            Destroy(indicator.gameObject);
        }
    }

    public virtual bool ActivateHighlight(int numChannels)
    {
        if (SessionState.ActiveTool.orientation == "Row" && (Int32.Parse(wellId.Substring(1)) - 1) + numChannels > maxRowNum)
        {
            return false;
        }
        else if (SessionState.ActiveTool.orientation == "Column" && ((int)wellId[0] % 32) - 1 + numChannels > maxColNum)
        {
           return false;
        }

        numChannels--;

        if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            if (!NextInRow.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            if (!NextInCol.ActivateHighlight(numChannels))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }
        }

        if (SessionState.ActiveSample != null)
        {
            if (SessionState.Materials[plateId].GetWell(wellId).ContainsSample(SessionState.ActiveSample))
            {
                DeactivateHighlight(SessionState.ActiveTool.numChannels);
                return false;
            }

            AddTempSampleIndicator();
            return true;
        }

        DeactivateHighlight(SessionState.ActiveTool.numChannels);
        return false;
    }

    public virtual void DeactivateHighlight(int numChannels)
    {
        LoadSampleIndicators();

        numChannels++;

        if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
        {
            NextInRow.DeactivateHighlight(numChannels);
        }
        else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
        {
            NextInCol.DeactivateHighlight(numChannels);
        }
    }

    public virtual bool OnSelected(int numChannels)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            ActivateHighlight(1);
            SelectionSprite.color = Color.blue;
            SelectionSprite.gameObject.SetActive(true);
            return true;
        }
        else
        {
            numChannels--;

            if (numChannels > 0 && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
            {
                if (!NextInRow.OnSelected(numChannels))
                {
                    OnDeselected(SessionState.ActiveTool.numChannels);
                    return false;
                }
            }
            else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
            {
                if (!NextInCol.OnSelected(numChannels))
                {
                    OnDeselected(SessionState.ActiveTool.numChannels);
                    return false;
                }
            }

            if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource && !selected)
            {
                SelectionSprite.color = Color.red;
                SelectionSprite.gameObject.SetActive(true);
            }
            else if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget && !selected)
            {
                SelectionSprite.color = Color.green;
                SelectionSprite.gameObject.SetActive(true);
            }
            return true;
        }
    }

    public virtual void OnSelectedAndClicked(int numChannels)
    {
        selected = true;
        SessionState.Materials[plateId].GetWell(wellId).selected = true;

        numChannels--;

        if (numChannels > 0)
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
    }

    public virtual void OnDeselected(int numChannels)
    {
        if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
        {
            SelectionSprite.color = Color.blue;
            SelectionSprite.gameObject.SetActive(false);
            DeactivateHighlight(1);
        }
        else
        {
            if (!selected)
            {
                SelectionSprite.color = Color.blue;
                SelectionSprite.gameObject.SetActive(false);

                numChannels++;

                if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Row" && NextInRow != null)
                {
                    NextInRow.OnDeselected(numChannels);
                }
                else if (numChannels != SessionState.ActiveTool.numChannels && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
                {
                    NextInCol.OnDeselected(numChannels);
                }
            }
        }
    }

    public void HighlightAction(LabAction action)
    {
        if (action.WellIsSource(plateId.ToString(), wellId))
        {
            this.SelectionSprite.color = action.source.color;
            this.SelectionSprite.gameObject.SetActive(true);
        }
        else if (action.WellIsTarget(plateId.ToString(), wellId))
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

    //pointer events
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!SessionState.FormActive && !SessionState.SelectionActive)
        {
            if (SessionState.ActiveActionType == LabAction.ActionType.pipette)
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    SessionState.CurrentStep.TryRemoveActiveSampleFromWell(wellId, plateId);
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
            else if (SessionState.ActiveActionType == LabAction.ActionType.dilution && (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget || SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource))
            {
                OnSelectedAndClicked(SessionState.ActiveTool.numChannels);
                SessionState.SetSelectedWells(plateId);
            }
            if (SessionState.Materials[plateId].ContainsWell(wellId))
            {
                SessionState.SetFocusedWell(wellId, plateId);
            }
        }
    }
}
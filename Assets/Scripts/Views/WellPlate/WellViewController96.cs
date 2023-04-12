using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class WellViewController96 : WellViewController
{
    void Awake()
    {
        ProcedureLoader.procedureStream.Subscribe(_ => LoadVisualState());
        SelectionManager.Instance.AvailableWells.Add(this);

        maxRowNum = 12;
        maxColNum = 8;

        wellId = gameObject.name;
        plateId = transform.parent.transform.parent.GetComponent<WellPlateViewController>().id;

        NextInRow = GetNextInRow();
        NextInCol = GetNextInCol();
    }

    // Start is called before the first frame update
    void Start()
    {
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

    /// <summary>
    /// Recursivly adds Sample to all wells in multichannel if possible
    /// Only ever called on multichannel clicks
    /// </summary>
    /// <param name="numChannels"></param>
    public override void AddSampleMultichannel(int numChannels)
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
        if (SessionState.CurrentStep.AddActiveSampleToWell(wellId, plateId, true, isStart, isEnd))
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
    public override bool ActivateHighlight(int numChannels)
    {
        //if number of channels is greater than the number of wells in the given orientation return
        if (SessionState.ActiveTool.orientation == "Row")
        {
            if ((Int32.Parse(wellId.Substring(1)) - 1) + numChannels > maxRowNum)
            {
                return false;
            }
        }
        else if (SessionState.ActiveTool.orientation == "Column")
        {
            if (((int)wellId[0] % 32) - 1 + numChannels > maxColNum)
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
        else if (numChannels > 0 && SessionState.ActiveTool.orientation == "Column" && NextInCol != null)
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

    public override void DeactivateHighlight(int numChannels)
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

    public override bool OnSelected(int numChannels)
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

            if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource && !selected)
            {
                SelectionSprite.color = Color.red;
            }
            else if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget && !selected)
            {
                SelectionSprite.color = Color.green;
            }
            SelectionSprite.gameObject.SetActive(true);
            return true;
        }
    }

    public override void OnSelectedAndClicked(int numChannels)
    {
        selected = true;
        SessionState.CurrentStep.materials[plateId].GetWell(wellId).selected = true;

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

    public override void OnDeselected(int numChannels)
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
        }
    }

    public override WellViewController GetNextInRow()
    {
        int wellNum;
        string nextWellId;

        if (wellId.Length == 2)
        {
            wellNum = Int32.Parse(wellId[1].ToString());
        }
        else
        {
            char[] chars = { wellId[1], wellId[2] };
            wellNum = Int32.Parse(new string(chars));
        }

        if(wellNum < maxRowNum)
        {
            wellNum++;
            nextWellId = new string(wellId[0] + wellNum.ToString());
            return transform.parent.Find(nextWellId).GetComponent<WellViewController>();
        }
        return null;
    }

    public override WellViewController GetNextInCol()
    {
        string nextWellId;

        char nextRowId = (char)(((int)wellId[0]) + 1);
        string columnNum = wellId.Substring(1);

        if ((int)wellId[0] - 64 < maxColNum)
        {
            nextWellId = new string(nextRowId.ToString() + columnNum);
            return transform.parent.Find(nextWellId).GetComponent<WellViewController>();
        }
        return null;
    }
}

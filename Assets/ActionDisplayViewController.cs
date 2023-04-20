using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ActionDisplayViewController : MonoBehaviour
{
    public GameObject ActionItemPrefab;
    public Transform ContentParent;
    public Canvas canvas;

    IDisposable actionAddedSubscription;
    IDisposable actionRemovedSubscription;

    void Awake()
    {
        SessionState.stepStream.Subscribe(stepNum =>
        {
            if(ContentParent != null)
            {
                foreach (Transform child in ContentParent)
                {
                    Destroy(child.gameObject);
                }
                foreach (LabAction action in SessionState.Steps[SessionState.ActiveStep].actions)
                {

                    CreateActionItem(action);
                }
            }

            RenewStepSubscriptions();
        });

        if(SessionState.Steps.Count > 0)
        {
            RenewStepSubscriptions();
        }
    }

    void RenewStepSubscriptions()
    {
        if(actionAddedSubscription != null)
            actionAddedSubscription.Dispose();
        if(actionRemovedSubscription != null)
            actionRemovedSubscription.Dispose();

        actionAddedSubscription = SessionState.CurrentStep.actionAddedStream.Subscribe(action =>
        {
            CreateActionItem(action);
        });

        actionRemovedSubscription = SessionState.CurrentStep.actionRemovedStream.Subscribe(action =>
        {
            Debug.Log(action.type);
            foreach (Transform child in ContentParent)
            {
                if (child.GetComponent<ActionItemViewController>().associatedAction == action)
                {
                    Destroy(child.gameObject);
                }
            }
        });
    }

    void CreateActionItem(LabAction action)
    {
        GameObject newActionItem = Instantiate(ActionItemPrefab) as GameObject;
        newActionItem.transform.SetParent(ContentParent, false);
        newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action);
        newActionItem.GetComponent<ActionItemViewController>().canvas = canvas;

        switch (action.type)
        {
            case LabAction.ActionType.pipette:
                newActionItem.GetComponent<Image>().color = new Color32(102, 178, 255, 255);
                break;
            case LabAction.ActionType.transfer:
                newActionItem.GetComponent<Image>().color = new Color32(255, 102, 102, 255);
                break;
            case LabAction.ActionType.dilution:
                newActionItem.GetComponent<Image>().color = new Color32(102, 255, 78, 255);
                break;
        }
    }
}

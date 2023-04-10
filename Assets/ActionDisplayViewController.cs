using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ActionDisplayViewController : MonoBehaviour
{
    public GameObject ActionItemPrefab;
    //public GameObject ActionSlotPrefab;
    public Transform ContentParent;
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.actionAddedStream.Subscribe(action =>
        {
           CreateActionItem(action);
        });

        SessionState.actionRemovedStream.Subscribe(action =>
        {
            foreach (Transform child in ContentParent)
            {
                if (child.GetComponent<ActionItemViewController>().actionText.text == action.GetActionString())
                {
                    Destroy(child.gameObject);
                }
            }
        });

        SessionState.stepStream.Subscribe(stepNum =>
        {
            foreach (Transform child in ContentParent)
            {
                Destroy(child.gameObject);
            }
            foreach (LabAction action in SessionState.Steps[SessionState.ActiveStep].actions)
            {

                CreateActionItem(action);
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

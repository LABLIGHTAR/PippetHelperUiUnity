using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
/*            //creat action slot
            GameObject newActionSlot = Instantiate(ActionSlotPrefab) as GameObject;
            newActionSlot.transform.SetParent(ContentParent, false);*/
            //create sample entry in list
            GameObject newActionItem = Instantiate(ActionItemPrefab) as GameObject;
            newActionItem.transform.SetParent(ContentParent, false);
            newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action.GetActionString());
            newActionItem.GetComponent<ActionItemViewController>().canvas = canvas;
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
/*                //creat action slot
                GameObject newActionSlot = Instantiate(ActionSlotPrefab) as GameObject;
                newActionSlot.transform.SetParent(ContentParent, false);*/
                //create sample entry in list
                GameObject newActionItem = Instantiate(ActionItemPrefab) as GameObject;
                newActionItem.transform.SetParent(ContentParent, false);
                newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action.GetActionString());
                newActionItem.GetComponent<ActionItemViewController>().canvas = canvas;
            }
        });
    }
}

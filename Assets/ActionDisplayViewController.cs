using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ActionDisplayViewController : MonoBehaviour
{
    public GameObject ActionItemPrefab;
    public Transform ContentParent;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.actionAddedStream.Subscribe(action =>
        {
            //create sample entry in list
            GameObject newActionItem = Instantiate(ActionItemPrefab) as GameObject;
            newActionItem.transform.SetParent(ContentParent, false);
            newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action.GetActionString());
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
                //create sample entry in list
                GameObject newActionItem = Instantiate(ActionItemPrefab) as GameObject;
                newActionItem.transform.SetParent(ContentParent, false);
                newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action.GetActionString());
            }
        });
    }
}

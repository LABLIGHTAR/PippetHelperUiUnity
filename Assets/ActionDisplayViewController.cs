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

            Debug.Log(action.source.matID);
            Debug.Log(action.target.matID);

            string sourceName = SessionState.Materials[int.Parse(action.source.matID)].GetNameAsSource(action.source.matSubID);
            string targetName = SessionState.Materials[int.Parse(action.target.matID)].GetNameAsTarget(action.target.matSubID);

            newActionItem.GetComponent<ActionItemViewController>().InitActionItem(action.type.ToString() + " " + action.source.volume + "μl" + " from " + sourceName + " to " + targetName);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

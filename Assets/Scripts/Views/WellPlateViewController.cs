using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WellPlateViewController : MonoBehaviour
{
    private GameObject selectedObject;
    public Transform wells;
    public int id;

    // Start is called before the first frame update
    void Start()
    {
        AddWellsToDictionary();
        SessionState.newStepStream.Subscribe(_ => AddWellsToDictionary());
    }

    void AddWellsToDictionary()
    {
        foreach (Transform child in wells)
        {
            if (!SessionState.Steps[SessionState.ActiveStep].plates[id].wells.ContainsKey(child.gameObject.name))
            {
                SessionState.Steps[SessionState.ActiveStep].plates[id].wells.Add(child.gameObject.name, new Well(child.name, id));
            }
        }
    }
}

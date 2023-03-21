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
        SessionState.newStepStream.Subscribe(_ => StartCoroutine(AddWellsToDictionary()));
        StartCoroutine(AddWellsToDictionary());
    }

    IEnumerator AddWellsToDictionary()
    {
        yield return new WaitForEndOfFrame();
        foreach (Transform child in wells)
        {
            Debug.Log("Step count: " + SessionState.Steps.Count + " current step: " + SessionState.ActiveStep + " plate ID: " + id + "Number of plates: " + SessionState.Steps[SessionState.ActiveStep].plates.Count);
            if (!SessionState.Steps[SessionState.ActiveStep].plates[id].wells.ContainsKey(child.gameObject.name))
            {
                SessionState.Steps[SessionState.ActiveStep].plates[id].wells.Add(child.gameObject.name, new Well(child.name, id));
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WellPlateViewController : MonoBehaviour
{
    private GameObject selectedObject;
    public Transform wells;
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
            if (!SessionState.Steps[SessionState.Step].wells.ContainsKey(child.gameObject.name))
            {
                SessionState.Steps[SessionState.Step].wells.Add(child.gameObject.name, new SessionState.Well());
            }
        }
    }
}

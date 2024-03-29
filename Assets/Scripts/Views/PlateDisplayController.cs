using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlateDisplayController : MonoBehaviour
{
    public List<GameObject> plateSlots;

    public GameObject wellplate96Prefab;
    public GameObject wellplate384Prefab;

    void Start()
    {
        MaterialViewController.materialsSelectedStream.Subscribe(materialCount =>
        {
            AddWellplatesToScene(materialCount);
        }).AddTo(this);

        ProcedureLoader.materialsLoadedStream.Subscribe(materialCount =>
        {
            AddWellplatesToScene(materialCount);
        }).AddTo(this);
    }

    void AddWellplatesToScene(int materialCount)
    {
        for (int i = 0; i < materialCount; i++)
        {
            if (SessionState.Materials[i] is Wellplate)
            {
                plateSlots[i].SetActive(true);
                if (((Wellplate)SessionState.Materials[i]).numWells == 96)
                {
                    var newPlateDisplay = Instantiate(wellplate96Prefab, plateSlots[i].transform);
                    newPlateDisplay.GetComponent<WellPlateViewController>().id = SessionState.Materials[i].id;
                }
                else if (((Wellplate)SessionState.Materials[i]).numWells == 384)
                {
                    var newPlateDisplay = Instantiate(wellplate384Prefab, plateSlots[i].transform);
                    newPlateDisplay.GetComponent<WellPlateViewController>().id = SessionState.Materials[i].id;
                }
            }
        }
    }
}

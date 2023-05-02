using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class PlateTitleViewController : MonoBehaviour
{
    [SerializeField]
    private List<TextMeshProUGUI> plateTitles;

    void Awake()
    {
        MaterialViewController.materialsSelectedStream.Subscribe(_ => UpdateTitles()).AddTo(this);
        ProcedureLoader.materialsLoadedStream.Subscribe(_ => UpdateTitles()).AddTo(this);
    }

    void UpdateTitles()
    {
        for(int i = 0; i < SessionState.Materials.Count; i++)
        {
            if (SessionState.Materials[i] is Wellplate) 
            {
                plateTitles[i].gameObject.SetActive(true);
                plateTitles[i].text = SessionState.Materials[i].customName;
            }
        }    
    }
}

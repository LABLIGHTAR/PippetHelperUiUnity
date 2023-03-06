using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SampleDisplayViewController : MonoBehaviour
{
    public Button addNewSampleButton;
    public GameObject newSampleForm;
    public GameObject SampleSwatchPrefab;

    public Transform ContentParent;

    // Start is called before the first frame update
    void Start()
    {
        //create button events
        addNewSampleButton.onClick.AddListener(delegate {
            if(SessionState.AvailableSamples.Count < 20)
            {
                newSampleForm.SetActive(true);
                SessionState.FormActive = true;
            }
            else
            {
                addNewSampleButton.enabled = false;
            }
        });

        //subscribe to data stream
        SessionState.newSampleStream.Subscribe(newSample =>
        {
            //create sample entry in list
            GameObject newSampleSwatch = Instantiate(SampleSwatchPrefab) as GameObject;
            newSampleSwatch.transform.SetParent(ContentParent, false);
            newSampleSwatch.GetComponent<SampleSwatchViewController>().InitSampleItem(newSample.name, newSample.abreviation, newSample.volume.ToString(), newSample.color);

            //set edit button event
            newSampleSwatch.GetComponent<SampleSwatchViewController>().editButton.onClick.AddListener(delegate
            {
                //open edit form
                Debug.Log(newSample.volume.ToString());
                newSampleForm.GetComponent<NewSampleFormController>().EditSample(newSample.name, newSample.abreviation, newSample.colorName, newSample.volume.ToString());
                //remove this entry
                Destroy(newSampleSwatch);
            });
        });
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
        addNewSampleButton.onClick.AddListener(OpenNewSampleForm);

        //subscribe to data stream
        SessionState.newSampleStream.Subscribe(newSample =>
        {
            AddSampleDisplay(newSample);
        }).AddTo(this);

        ProcedureLoader.procedureStream.Subscribe(_ =>
        {
            foreach (Sample sample in SessionState.AvailableSamples)
            {
                AddSampleDisplay(sample);
            }
        }).AddTo(this);
    }

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame && !SessionState.FormActive)
        {
            OpenNewSampleForm();
        }
    }

    void OpenNewSampleForm()
    {
        if (SessionState.AvailableSamples.Count < 20)
        {
            newSampleForm.SetActive(true);
            SessionState.FormActive = true;
        }
        else
        {
            addNewSampleButton.enabled = false;
        }
    }

    void AddSampleDisplay(Sample sample)
    {
        //create sample entry in list
        GameObject newSampleSwatch = Instantiate(SampleSwatchPrefab) as GameObject;
        newSampleSwatch.transform.SetParent(ContentParent, false);
        newSampleSwatch.GetComponent<SampleSwatchViewController>().InitSampleItem(sample.sampleName, sample.abreviation, sample.color);

        //set edit button event
        newSampleSwatch.GetComponent<SampleSwatchViewController>().editButton.onClick.AddListener(delegate
        {
            //open edit form
            newSampleForm.GetComponent<NewSampleFormController>().EditSample(sample.sampleName, sample.abreviation, sample.colorName);
        });
    }
}

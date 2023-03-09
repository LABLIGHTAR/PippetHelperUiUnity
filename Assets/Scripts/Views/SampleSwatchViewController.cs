using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;

public class SampleSwatchViewController : MonoBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI abreviation;
    public Image swatch;

    public Button editButton;
    public Button deleteButton;

    void Start()
    {
        deleteButton.onClick.AddListener(delegate
        {
            if (name.text != null)
            {
                SessionState.RemoveSample(name.text);
                Destroy(gameObject);
            }
        });

        SessionState.editedSampleStream.Subscribe(sampleNames =>
        {
            //if the edited samples old name is equal to this displays sample name update this display
            if (sampleNames.Item1 == name.text)
            {
                SessionState.Sample editedSample = SessionState.AvailableSamples.Where(sample => sample.name == sampleNames.Item2).FirstOrDefault();
                if (editedSample != null)
                {
                    InitSampleItem(editedSample.name, editedSample.abreviation, editedSample.color);
                }
            }
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left & !SessionState.FormActive)
        {
            SessionState.SetActiveSample(SessionState.AvailableSamples.Where(x => x.name.Equals(name.GetComponent<TMP_Text>().text)).FirstOrDefault());
        }
    }

    public bool InitSampleItem(string SampleName, string SampleAbrev, Color displayColor)
    {
        swatch.color = displayColor;
        name.GetComponent<TMP_Text>().text = SampleName;
        abreviation.GetComponent<TMP_Text>().text = SampleAbrev;
        return true;
    }
}

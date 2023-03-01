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
    public TextMeshProUGUI volume;
    public Image swatch;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left & !SessionState.FormActive)
        {
            SessionState.SetActiveSample(SessionState.AvailableSamples.Where(x => x.abreviation.Equals(abreviation.GetComponent<TMP_Text>().text)).FirstOrDefault());
        }
    }

    public bool InitSampleItem(string SampleName, string SampleAbrev, string SampleVolume, Color displayColor)
    {
        swatch.color = displayColor;
        name.GetComponent<TMP_Text>().text = SampleName;
        abreviation.GetComponent<TMP_Text>().text = SampleAbrev;
        volume.GetComponent<TMP_Text>().text = SampleVolume + "μL";

        return true;
    }
}

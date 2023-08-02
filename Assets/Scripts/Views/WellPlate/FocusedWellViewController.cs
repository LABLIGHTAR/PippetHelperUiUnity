using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UniRx;

public class FocusedWellViewController : MonoBehaviour
{
    public GameObject wellDisplay;
    public GameObject wellVolumeDisplay;
    public TextMeshProUGUI wellIdText;
    public TextMeshProUGUI wellVolumeText;
    public TextMeshProUGUI wellGroupText;

    public GameObject SampleItemPrefab;
    public RectTransform ScrollViewContent;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.focusedWellStream.Subscribe(well => UpdateVisualState(well)).AddTo(this);
    }

    void Update()
    {
        if(ScrollViewContent.childCount > 4)
        {
            if(ScrollViewContent.localPosition.y < ScrollViewContent.rect.height/2)
                ScrollViewContent.localPosition = new Vector3(ScrollViewContent.localPosition.x, ScrollViewContent.localPosition.y + 0.15f, ScrollViewContent.localPosition.z);
            else
                ScrollViewContent.localPosition = new Vector3(ScrollViewContent.localPosition.x, 0f, ScrollViewContent.localPosition.z);
        }
    }

    void UpdateVisualState(Well well)
    {
        Debug.Log("updating focused well");
        ClearSampleDisplays();

        float sampleVolume = 0f;

        LabAction mostRecentAction = SessionState.TryGetMostRecentAction();

        foreach (Sample sample in well.GetSamples())
        {
            if (mostRecentAction != null)
            {
                sampleVolume = well.GetSampleVolumeAtAction(sample, mostRecentAction);
                if (sampleVolume > 0)
                {
                    var newSampleItem = Instantiate(SampleItemPrefab, ScrollViewContent);
                    newSampleItem.GetComponent<FocusedSampleItemViewController>().InitItem(sample, sampleVolume);
                }
            }
            sampleVolume = 0f;
        }

        //update well display
        if (well.id != null)
        {
            wellIdText.text = well.id;

            if(mostRecentAction != null)
            {
                wellVolumeText.text = well.GetVolumeAtAction(mostRecentAction).ToString() + " μL";
            }

            wellDisplay.SetActive(true);
            wellVolumeDisplay.SetActive(true);
        }
    }

    void ClearSampleDisplays()
    {
        foreach(Transform child in ScrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }
}

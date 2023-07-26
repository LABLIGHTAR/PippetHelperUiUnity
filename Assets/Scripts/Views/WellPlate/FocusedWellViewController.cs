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
        ClearSampleDisplays();

        float wellVolume = 0f;
        float sampleVolume = 0f;

        LabAction sampleAddedAction = null;

        foreach(var sample in well.GetSamples())
        {
            for (int i = 0; i <= SessionState.ActiveStep; i++)
            {
                foreach (LabAction action in SessionState.Steps[i].GetActionsWithTargetWell(well))
                {
                    if(action.SampleIsSource(sample))
                    {
                        sampleAddedAction = action;
                        sampleVolume += action.source.volume;
                    }
                    else if(action.TryGetSourceWellSamples() != null && action.TryGetSourceWellSamples().Contains(sample))
                    {
                        var prevAction = SessionState.Steps[i].actions[(SessionState.Steps[i].actions.IndexOf(action) - 1)];
                        var sourceWell = action.TryGetSourceWell();
                        
                        float ratio = action.source.volume / sourceWell.GetVolumeAtAction(prevAction);

                        sampleVolume += ((sourceWell.GetSampleVolumeAtAction(sample, prevAction) * ratio));
                    }
                }
            }
            if(sampleAddedAction != null)
            {
                foreach (LabAction action in SessionState.GetAllActionsAfter(sampleAddedAction).Where(action => action.WellIsSource(well.plateId.ToString(), well.id)))
                {
                    var prevAction = SessionState.Steps[action.step].actions[(SessionState.Steps[action.step].actions.IndexOf(action) - 1)];

                    sampleVolume -= (sampleVolume / well.GetVolumeAtAction(prevAction)) * action.source.volume;
                }
            }
            else
            {
                for (int i = 0; i <= SessionState.ActiveStep; i++)
                {
                    foreach (LabAction action in SessionState.Steps[i].GetActionsWithSourceWell(well))
                    {
                        var prevAction = SessionState.Steps[action.step].actions[(SessionState.Steps[action.step].actions.IndexOf(action) - 1)];

                        sampleVolume -= (sampleVolume / well.GetVolumeAtAction(prevAction)) * action.source.volume;
                    }
                }
            }

            wellVolume += sampleVolume;
            if(sampleVolume > 0)
            {
                var newSampleItem = Instantiate(SampleItemPrefab, ScrollViewContent);
                newSampleItem.GetComponent<FocusedSampleItemViewController>().InitItem(sample, sampleVolume);
            }
            sampleVolume = 0f;
        }
        
        //update well display
        if(well.id != null)
        {
            wellIdText.text = well.id;
            wellVolumeText.text = wellVolume.ToString() + " μL";

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

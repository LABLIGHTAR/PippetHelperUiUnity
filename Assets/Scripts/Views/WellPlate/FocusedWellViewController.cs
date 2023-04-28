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
        
        foreach(Step step in SessionState.Steps)
        {
            foreach (LabAction action in step.GetActionsWithTargetWell(well).Where(action => action.type == LabAction.ActionType.pipette))
            {
                wellVolume += action.source.volume;
                var newSampleItem = Instantiate(SampleItemPrefab, ScrollViewContent);
                newSampleItem.GetComponent<FocusedSampleItemViewController>().InitItem(action);
            }
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

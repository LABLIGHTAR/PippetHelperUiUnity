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
    public Transform ScrollViewContent;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.focusedWellStream.Subscribe(well => UpdateVisualState(well));
    }

    void UpdateVisualState(Well well)
    {
        ClearSampleDisplays();

        float wellVolume = 0f;
        
        foreach(LabAction action in SessionState.CurrentStep.GetActionsWithTargetWell(well).Where(action => action.type == LabAction.ActionType.pipette))
        {
            wellVolume += action.source.volume;
            var newSampleItem = Instantiate(SampleItemPrefab, ScrollViewContent);
            newSampleItem.GetComponent<FocusedSampleItemViewController>().InitItem(action);
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

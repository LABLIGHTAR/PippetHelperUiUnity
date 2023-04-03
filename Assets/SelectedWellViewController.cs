using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class SelectedWellViewController : MonoBehaviour
{
    public Well selectedWell;

    public GameObject wellDisplay;
    public GameObject wellVolumeDisplay;
    public TextMeshProUGUI wellIdText;
    public TextMeshProUGUI wellVolumeText;
    public TextMeshProUGUI wellGroupText;

    public GameObject sampleOneDisplay;
    public TextMeshProUGUI sampleOneNameText;
    public TextMeshProUGUI sampleOneVolumeText;
    public Image sampleOneBG;

    public GameObject sampleTwoDisplay;
    public TextMeshProUGUI sampleTwoNameText;
    public TextMeshProUGUI sampleTwoVolumeText;
    public Image sampleTwoBG;

    public GameObject sampleThreeDisplay;
    public TextMeshProUGUI sampleThreeNameText;
    public TextMeshProUGUI sampleThreeVolumeText;
    public Image sampleThreeBG;

    public bool selecting;
    public bool wellSelected;

    // Start is called before the first frame update
    void Start()
    {
        SessionState.focusedWellStream.Subscribe(well =>
        {   
            if(selecting && !wellSelected)
                UpdateVisualState(well);
        });
        SessionState.selectedWellStream.Subscribe(well =>
        {
            if(selecting)
            {
                UpdateVisualState(well);
                selectedWell = well;
                wellSelected = true;
                selecting = false;
            }
        });
    }

    public void ClearDisplay()
    {
        sampleOneDisplay.SetActive(false);
        sampleTwoDisplay.SetActive(false);
        sampleThreeDisplay.SetActive(false);
        wellIdText.text = "";
        wellVolumeText.text = "";
        wellSelected = false;
        selectedWell = null;
    }

    void UpdateVisualState(Well well)
    {
        sampleOneDisplay.SetActive(false);
        sampleTwoDisplay.SetActive(false);
        sampleThreeDisplay.SetActive(false);

        float wellVolume = 0f;

        int index = -1;

        foreach (var sample in well.Samples)
        {
            index++;

            if (index == 0)
            {
                sampleOneNameText.text = sample.Key.sampleName;
                sampleOneBG.color = sample.Key.color;
                sampleOneVolumeText.text = sample.Value.ToString() + " μL";
                wellVolume += sample.Value;
                sampleOneDisplay.SetActive(true);
            }
            else if (index == 1)
            {
                sampleTwoNameText.text = sample.Key.sampleName;
                sampleTwoBG.color = sample.Key.color;
                sampleTwoVolumeText.text = sample.Value.ToString() + " μL";
                wellVolume += sample.Value;
                sampleTwoDisplay.SetActive(true);
            }
            else if (index == 3)
            {
                sampleThreeNameText.text = sample.Key.sampleName;
                sampleThreeBG.color = sample.Key.color;
                sampleThreeVolumeText.text = sample.Value.ToString() + " μL";
                wellVolume += sample.Value;
                sampleThreeDisplay.SetActive(true);
            }
        }

        //update well display
        wellIdText.text = well.id;
        wellVolumeText.text = wellVolume.ToString() + " μL";

        wellDisplay.SetActive(true);
        wellVolumeDisplay.SetActive(true);
    }
}

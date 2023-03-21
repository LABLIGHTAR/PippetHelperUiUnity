using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class FocusedWellViewController : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        SessionState.focusedWellStream.Subscribe(well => UpdateVisualState(well));
    }

    void UpdateVisualState(Well well)
    {
        sampleOneDisplay.SetActive(false);
        sampleTwoDisplay.SetActive(false);
        sampleThreeDisplay.SetActive(false);

        float wellVolume = 0f;

        int index = -1;

        foreach(var sample in well.Samples)
        {
            index++;

            if(index == 0)
            {
                sampleOneNameText.text = sample.Key.name;
                sampleOneBG.color = sample.Key.color;
                sampleOneVolumeText.text = sample.Value.ToString() + " μL";
                wellVolume += sample.Value;
                sampleOneDisplay.SetActive(true);
            }
            else if(index == 1)
            {
                sampleTwoNameText.text = sample.Key.name;
                sampleTwoBG.color = sample.Key.color;
                sampleTwoVolumeText.text = sample.Value.ToString() + " μL";
                wellVolume += sample.Value;
                sampleTwoDisplay.SetActive(true);
            }
            else if(index == 3)
            {
                sampleThreeNameText.text = sample.Key.name;
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

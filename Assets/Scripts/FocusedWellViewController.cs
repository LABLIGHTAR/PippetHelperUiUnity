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

    void UpdateVisualState(SessionState.Well well)
    {
        float wellVolume = 0f;

        //update sample one display
        if (well.Samples.Count > 0)
        {
            sampleOneNameText.text = well.Samples[0].name;
            sampleOneBG.color = well.Samples[0].color;
            sampleOneVolumeText.text = well.Samples[0].volume.ToString() + " μL";
            wellVolume += well.Samples[0].volume;
            sampleOneDisplay.SetActive(true);
        }
        else
        {
            sampleOneDisplay.SetActive(false);
        }
        //update sample two display
        if (well.Samples.Count > 1)
        {
            sampleTwoNameText.text = well.Samples[1].name;
            sampleTwoBG.color = well.Samples[1].color;
            sampleTwoVolumeText.text = well.Samples[1].volume.ToString() + " μL";
            wellVolume += well.Samples[1].volume;
            sampleTwoDisplay.SetActive(true);
        }
        else
        {
            sampleTwoDisplay.SetActive(false);
        }
        //update sample three display
        if (well.Samples.Count > 2)
        {
            sampleThreeNameText.text = well.Samples[2].name;
            sampleThreeBG.color = well.Samples[2].color;
            sampleThreeVolumeText.text = well.Samples[2].volume.ToString() + " μL";
            wellVolume += well.Samples[2].volume;
            sampleThreeDisplay.SetActive(true);
        }
        else
        {
            sampleThreeDisplay.SetActive(false);
        }
        //update well display
        wellIdText.text = SessionState.Steps[SessionState.Step].wells.Where(x => x.Value == well).FirstOrDefault().Key;
        wellVolumeText.text = wellVolume.ToString() + " μL";

        wellDisplay.SetActive(true);
        wellVolumeDisplay.SetActive(true);
    }
}

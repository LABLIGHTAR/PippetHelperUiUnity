using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FocusedSampleItemViewController : MonoBehaviour
{
    public TextMeshProUGUI sampleName;
    public TextMeshProUGUI sampleVolume;
    public Image bgImage;

    public void InitItem(LabAction action)
    {
        Sample sample = action.TryGetSourceSample();
        if(sample != null)
        {
            sampleName.text = sample.sampleName;
            bgImage.color = sample.color;
            sampleVolume.text = action.source.volume.ToString() + "μl";
        }
    }
}

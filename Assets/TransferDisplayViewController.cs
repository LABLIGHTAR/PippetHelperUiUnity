using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransferDisplayViewController : MonoBehaviour
{
    public GameObject sourceWellDisplay;
    public GameObject targetWellDisplay;

    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI volumeErrorText;

    public Button confirmButton;

    public TextMeshProUGUI instructionText;

    private SelectedWellViewController source;
    private SelectedWellViewController target;

    // Start is called before the first frame update
    void Start()
    {
        source = sourceWellDisplay.GetComponent<SelectedWellViewController>();
        target = targetWellDisplay.GetComponent<SelectedWellViewController>();
        instructionText.text = "Select a source";
        source.selecting = true;
        target.selecting = false;
        confirmButton.enabled = false;

        confirmButton.onClick.AddListener(SubmitTransferAction);
    }

    // Update is called once per frame
    void Update()
    {
        if(!source.selecting && !target.wellSelected)
        {
            target.selecting = true;
            instructionText.text = "Select a target";
        }
        if(source.wellSelected && target.wellSelected)
        {
            instructionText.text = "Enter the volume to be transfered";
            if(VolumeValid())
            {
                confirmButton.enabled = true;
            }
        }
    }

    private bool VolumeValid()
    {
        if (!(volumeText.text.Length > 0))
        {
            volumeErrorText.gameObject.SetActive(true);
            volumeErrorText.text = "Volume cannot be empty*";
            return false;
        }
        if (!float.TryParse(volumeText.text.Substring(0, volumeText.text.Length - 1), out _))
        {
            volumeErrorText.gameObject.SetActive(true);
            volumeErrorText.text = "Please do not include units*";
            return false;
        }

        float volume = float.Parse(volumeText.text.Substring(0, volumeText.text.Length - 1), CultureInfo.InvariantCulture.NumberFormat);

        if (volume < 0)
        {
            volumeErrorText.gameObject.SetActive(true);
            volumeErrorText.text = "Please enter a positive number*";
            return false;
        }

        volumeErrorText.gameObject.SetActive(false);
        return true;
    }

    private void SubmitTransferAction()
    {
        float volume = float.Parse(volumeText.text.Substring(0, volumeText.text.Length - 1), CultureInfo.InvariantCulture.NumberFormat);

        SessionState.AddTransferAction(source.selectedWell, target.selectedWell, volume);

        //clear UI
        instructionText.text = "Select a source";
        source.ClearDisplay();
        target.ClearDisplay();
        source.selecting = true;
        target.selecting = false;
        confirmButton.enabled = false;
    }
}

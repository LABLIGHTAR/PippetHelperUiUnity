using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class ToolViewController : MonoBehaviour
{
    public GameObject micropipette;
    public SpriteRenderer pipetteIndicator;
    public TMP_InputField pipetteVolumeInput;
    
    public GameObject multichannel;
    public SpriteRenderer multichannelIndicator;
    public TMP_InputField multichannelVolumeInput;
    public TextMeshProUGUI multichannelDropdown;
    public TextMeshProUGUI orientationText;
    public TMP_Dropdown orientationDropdown;
    // Start is called before the first frame update
    void Start()
    {
        //subscribe to data streams
        SessionState.leftClickStream.Subscribe(selectedObject =>
        {
            if(selectedObject.name == "micropipette")
            {
                SelectMicropipette();
            }
            else if(selectedObject.name == "multichannel")
            {
                SelectMultichannel();
            }
        });

        SessionState.activeLiquidStream.Subscribe(_ => UpdateVisualState());

        //set up input defaults
        pipetteVolumeInput.text = "10";
        multichannelVolumeInput.text = "10";

        //set micropipette as default tool
        SelectMicropipette();
    }

    void UpdateVisualState()
    {
        if(SessionState.ActiveTool != null)
        {
            if (SessionState.ActiveTool.name == "micropipette")
            {
                pipetteIndicator.color = SessionState.ActiveLiquid.color;
            }
            else if (SessionState.ActiveTool.name == "multichannel")
            {
                multichannelIndicator.color = SessionState.ActiveLiquid.color;
            }
        }
    }

    public void SelectMicropipette()
    {
        multichannelIndicator.color = Color.white;
        if(SessionState.ActiveLiquid != null)
        {
            pipetteIndicator.color = SessionState.ActiveLiquid.color;
        }
        else
        {
            pipetteIndicator.color = Color.green;
        }
        
        if(pipetteVolumeInput != null && pipetteVolumeInput.text.Length > 0)
        {
            float volume = float.Parse(pipetteVolumeInput.text, CultureInfo.InvariantCulture.NumberFormat);
            SessionState.ActiveTool = new SessionState.Tool("micropipette", volume, 1, "Horizontal");
        }

        //Debug.Log(SessionState.ActiveTool.name + " " + SessionState.ActiveTool.volume + " " + SessionState.ActiveTool.numChannels + " " + SessionState.ActiveTool.orientation);
    }

    public void SelectMultichannel()
    {
        pipetteIndicator.color = Color.white;
        if (SessionState.ActiveLiquid != null)
        {
            multichannelIndicator.color = SessionState.ActiveLiquid.color;
        }
        else
        {
            multichannelIndicator.color = Color.green;
        }

        if (multichannelVolumeInput != null && multichannelVolumeInput.text.Length > 0)
        {
            float volume = float.Parse(multichannelVolumeInput.text, CultureInfo.InvariantCulture.NumberFormat);
            int channels = int.Parse(multichannelDropdown.text);
            SessionState.ActiveTool = new SessionState.Tool("multichannel", volume, channels, orientationText.text);
        }
        //Debug.Log(SessionState.ActiveTool.name + " " + SessionState.ActiveTool.volume + " " + SessionState.ActiveTool.numChannels + " " + SessionState.ActiveTool.orientation);
    }

    public void ChangeOrientation()
    {
        if(int.Parse(multichannelDropdown.text) > 8 && orientationText.text == "Vertical")
        {
            orientationDropdown.value = 1;
        }
    }
}

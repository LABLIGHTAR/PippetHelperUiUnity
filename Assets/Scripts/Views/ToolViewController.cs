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
    public Image pipetteIndicator;
    
    public GameObject multichannel;
    public Image multichannelIndicator;
    public TextMeshProUGUI multichannelDropdown;
    public TextMeshProUGUI orientationText;
    public TMP_Dropdown orientationDropdown;
    // Start is called before the first frame update
    void Start()
    {
        //subscribe to data streams
        UiInteraction.uiClickStream.Subscribe(selectedObject =>
        {
            if(selectedObject.name == "PipetteImage")
            {
                SelectMicropipette();
            }
            else if(selectedObject.name == "MultichannelImage")
            {
                SelectMultichannel();
            }
        });

        SessionState.activeSampleStream.Subscribe(_ => UpdateVisualState());

        //set micropipette as default tool
        SelectMicropipette();
    }

    void UpdateVisualState()
    {
        if(SessionState.ActiveTool != null && SessionState.ActiveSample != null)
        {
            if (SessionState.ActiveTool.name == "micropipette")
            {
                pipetteIndicator.color = SessionState.ActiveSample.color;
            }
            else if (SessionState.ActiveTool.name == "multichannel")
            {
                multichannelIndicator.color = SessionState.ActiveSample.color;
            }
        }
    }

    public void SelectMicropipette()
    {
        multichannelIndicator.color = Color.white;
        if(SessionState.ActiveSample != null)
        {
            pipetteIndicator.color = SessionState.ActiveSample.color;
        }
        else
        {
            pipetteIndicator.color = Color.green;
        }
        SessionState.ActiveTool = new SessionState.Tool("micropipette", 1, "Horizontal");
        //Debug.Log(SessionState.ActiveTool.name + " " + SessionState.ActiveTool.volume + " " + SessionState.ActiveTool.numChannels + " " + SessionState.ActiveTool.orientation);
    }

    public void SelectMultichannel()
    {
        pipetteIndicator.color = Color.white;
        if (SessionState.ActiveSample != null)
        {
            multichannelIndicator.color = SessionState.ActiveSample.color;
        }
        else
        {
            multichannelIndicator.color = Color.green;
        }
        int channels = int.Parse(multichannelDropdown.text);
        SessionState.ActiveTool = new SessionState.Tool("multichannel", channels, orientationText.text);
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

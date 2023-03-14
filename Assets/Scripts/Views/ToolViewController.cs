using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;

public class ToolViewController : MonoBehaviour
{
    public GameObject micropipette;
    public Image pipetteIndicator;
    public TMP_InputField pipetteVolumeText;
    public TextMeshProUGUI pipetteErrorText;

    public GameObject multichannel;
    public Image multichannelIndicator;
    public TMP_InputField multiVolumeText;
    public TextMeshProUGUI multiErrorText;
    public TextMeshProUGUI multichannelDropdown;
    public TextMeshProUGUI orientationText;
    public TMP_Dropdown orientationDropdown;

    // Start is called before the first frame update
    void Start()
    {
/*        //subscribe to data streams
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
        });*/

        SessionState.activeSampleStream.Subscribe(_ => UpdateVisualState());

        //set default volume values
        pipetteVolumeText.text = "10";
        multiVolumeText.text = "10";

        //set micropipette as default tool
        SelectMicropipette();
    }

    void Update()
    {
        if(Keyboard.current.digit1Key.wasPressedThisFrame && !SessionState.FormActive)
        {
            SelectMicropipette();
        }
        else if(Keyboard.current.digit2Key.wasPressedThisFrame && !SessionState.FormActive)
        {
            SelectMultichannel();
        }
    }

    //change volume with mousewheel
    void OnGUI()
    {
        if (Mouse.current.scroll.ReadValue().y > 0 & SessionState.ActiveTool != null)
        {
            SessionState.ActiveTool.SetVolume((float)Math.Round(SessionState.ActiveTool.volume + 0.1f, 2));
            if(SessionState.ActiveTool.name == "micropipette")
            {
                pipetteVolumeText.text = SessionState.ActiveTool.volume.ToString();
            }
            else if(SessionState.ActiveTool.name == "multichannel")
            {
                multiVolumeText.text = SessionState.ActiveTool.volume.ToString();
            }
        }
        else if(Mouse.current.scroll.ReadValue().y < 0 & SessionState.ActiveTool != null)
        {
            if (SessionState.ActiveTool.volume - 0.1f > 0)
            {
                SessionState.ActiveTool.SetVolume((float)Math.Round(SessionState.ActiveTool.volume - 0.1f, 2));
                if (SessionState.ActiveTool.name == "micropipette")
                {
                    pipetteVolumeText.text = SessionState.ActiveTool.volume.ToString();
                }
                else if (SessionState.ActiveTool.name == "multichannel")
                {
                    multiVolumeText.text = SessionState.ActiveTool.volume.ToString();
                }
            }
            
        }
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

    //sets micropipette as active tool if values are valid
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
        if (!(pipetteVolumeText.text.Length > 0))
        {
            pipetteErrorText.gameObject.SetActive(true);
            pipetteErrorText.text = "Volume cannot be empty*";
            return;
        }
        if (!float.TryParse(pipetteVolumeText.text.Substring(0, pipetteVolumeText.text.Length), out _))
        {
            pipetteErrorText.gameObject.SetActive(true);
            pipetteErrorText.text = "Please do not include units*";
            return;
        }

        float volume = float.Parse(pipetteVolumeText.text.Substring(0, pipetteVolumeText.text.Length), CultureInfo.InvariantCulture.NumberFormat);

        if(volume < 0)
        {
            pipetteErrorText.gameObject.SetActive(true);
            pipetteErrorText.text = "Please enter a positive number*";
            return;
        }

        pipetteErrorText.gameObject.SetActive(false);

        SessionState.ActiveTool = new SessionState.Tool("micropipette", 1, "Horizontal", volume);
        //Debug.Log(SessionState.ActiveTool.name + " " + SessionState.ActiveTool.volume + " " + SessionState.ActiveTool.numChannels + " " + SessionState.ActiveTool.orientation + " " + SessionState.ActiveTool.volume);
    }

    //sets multichannel as active tool if input valid
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

        if (!(multiVolumeText.text.Length > 0))
        {
            multiErrorText.gameObject.SetActive(true);
            multiErrorText.text = "Volume cannot be empty*";
            return;
        }
        if (!float.TryParse(multiVolumeText.text.Substring(0, multiVolumeText.text.Length), out _))
        {
            multiErrorText.gameObject.SetActive(true);
            multiErrorText.text = "Please do not include units*";
            return;
        }

        float volume = float.Parse(multiVolumeText.text.Substring(0, multiVolumeText.text.Length), CultureInfo.InvariantCulture.NumberFormat);

        if (volume < 0)
        {
            multiErrorText.gameObject.SetActive(true);
            multiErrorText.text = "Please enter a positive number*";
            return;
        }

        multiErrorText.gameObject.SetActive(false);

        SessionState.ActiveTool = new SessionState.Tool("multichannel", channels, orientationText.text, volume);
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

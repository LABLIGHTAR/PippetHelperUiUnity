using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelViewController : MonoBehaviour
{
    public GameObject settingsPanel;
    public Button closeButton;
    public Toggle fullscreenToggle;
    public Toggle tutorialToggle;

    void Start()
    {
        closeButton.onClick.AddListener(delegate
        {
            settingsPanel.SetActive(false);
            SessionState.FormActive = false;
        });

        //init player prefs
        if (!PlayerPrefs.HasKey("Fullscreen")) PlayerPrefs.SetInt("FullScreen", 1);
        if (!PlayerPrefs.HasKey("Tutorial")) PlayerPrefs.SetInt("Tutorial", 1);
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        SessionState.FormActive = true;

        fullscreenToggle.isOn = (PlayerPrefs.GetInt("Fullscreen") != 0);
        tutorialToggle.isOn = (PlayerPrefs.GetInt("Tutorial") != 0);
    }

    public void Fullscreen(bool isFullscreen)
    {
        if(isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerPrefs.SetInt("Fullscreen", (true ? 1 : 0));
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt("Fullscreen", (false ? 1 : 0));
        }
    }

    public void Tutorial(bool allowTutorial)
    {
        if (allowTutorial)
        {
            PlayerPrefs.SetInt("Tutorial", (true ? 1 : 0));
        }
        else
        {
            PlayerPrefs.SetInt("Tutorial", (false ? 1 : 0));
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}

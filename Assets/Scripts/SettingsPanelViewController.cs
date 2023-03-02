using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelViewController : MonoBehaviour
{
    public GameObject settingsPanel;
    public Button closeButton;

    void Start()
    {
        closeButton.onClick.AddListener(delegate
        {
            settingsPanel.SetActive(false);
            SessionState.FormActive = false;
        });
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        SessionState.FormActive = true;
    }

    public void Fullscreen(bool isFullscreen)
    {
        if(isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
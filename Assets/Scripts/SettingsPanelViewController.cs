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
        Screen.fullScreen = isFullscreen;
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}

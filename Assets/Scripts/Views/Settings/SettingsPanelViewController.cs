using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SettingsPanelViewController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject savePanel;
    public Button quitDesktopButton;
    public Button quitMenuButton;
    public Button yesButton;
    public Button noButton;
    public Button closeButton;
    public Toggle fullscreenToggle;
    public Toggle tutorialToggle;

    public ProcedureGenerator generator;
    public SceneLoader sceneLoader;

    private bool quitApp;

    void Start()
    {
        closeButton.onClick.AddListener(delegate
        {
            settingsPanel.SetActive(false);
            SessionState.FormActive = false;
        });

        quitDesktopButton.onClick.AddListener(delegate
        {
            quitApp = true;
            ActivateSavePanel();
        });

        quitMenuButton.onClick.AddListener(delegate
        {
            quitApp = false;
            ActivateSavePanel();
        });

        yesButton.onClick.AddListener(delegate
        {
            savePanel.SetActive(false);
            generator.GenerateProcedure();
            Quit();
        });

        noButton.onClick.AddListener(delegate 
        {
            savePanel.SetActive(false);
            Quit();
        });

        //init player prefs
        if (!PlayerPrefs.HasKey("Fullscreen")) PlayerPrefs.SetInt("FullScreen", 1);
        if (!PlayerPrefs.HasKey("Tutorial")) PlayerPrefs.SetInt("Tutorial", 1);
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame && !SessionState.FormActive)
        {
            OpenSettingsPanel();
        }
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

    private void ActivateSavePanel()
    {
        savePanel.SetActive(true);
    }

    private void Quit()
    {
        if (quitApp)
        {
            Application.Quit();
        }
        else
        {
            sceneLoader.LoadMainMenuScene();
        }
    }
}

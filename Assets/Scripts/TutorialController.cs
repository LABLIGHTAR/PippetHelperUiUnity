using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public List<TutorialPanelController> panels;

    private int currentPanel;

    // Start is called before the first frame update
    void Start()
    {
        if((PlayerPrefs.GetInt("Tutorial") != 0))
        {
            currentPanel = 0;
            panels[currentPanel].gameObject.SetActive(true);
            SessionState.FormActive = true;

            foreach (TutorialPanelController panel in panels)
            {
                panel.nextButton.onClick.AddListener(delegate
                {
                    if (currentPanel < panels.Count - 1)
                    {
                        panels[currentPanel].gameObject.SetActive(false);
                        currentPanel++;
                        panels[currentPanel].gameObject.SetActive(true);
                    }
                    else
                    {
                        this.gameObject.SetActive(false);
                        SessionState.FormActive = false;
                    }
                });

                panel.skipButton.onClick.AddListener(delegate {
                    this.gameObject.SetActive(false);
                    SessionState.FormActive = false;
                });
            }
        }
    }
}

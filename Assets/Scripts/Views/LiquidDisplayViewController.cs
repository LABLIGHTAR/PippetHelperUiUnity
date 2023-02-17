using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiquidDisplayViewController : MonoBehaviour
{
    public Button addNewLiquidButton;
    public GameObject newLiquidForm;
    // Start is called before the first frame update
    void Start()
    {
        addNewLiquidButton.onClick.AddListener(delegate {
            if(SessionState.availableLiquids.Count < 20)
            {
                newLiquidForm.SetActive(true);
            }
            else
            {
                addNewLiquidButton.enabled = false;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

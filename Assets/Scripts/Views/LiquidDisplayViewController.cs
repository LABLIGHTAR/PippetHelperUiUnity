using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LiquidDisplayViewController : MonoBehaviour
{
    public Button addNewLiquidButton;
    public GameObject newLiquidForm;
    public GameObject LiquidSwatchPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //create button events
        addNewLiquidButton.onClick.AddListener(delegate {
            if(SessionState.AvailableLiquids.Count < 20)
            {
                newLiquidForm.SetActive(true);
                SessionState.FormActive = true;
            }
            else
            {
                addNewLiquidButton.enabled = false;
            }
        });

        //subscribe to data stream
        SessionState.newLiquidStream.Subscribe(newLiquid =>
        {
            GameObject newLiquidSwatch = Instantiate(LiquidSwatchPrefab) as GameObject;
            newLiquidSwatch.transform.SetParent(this.transform, false);
            newLiquidSwatch.GetComponent<LiquidSwatchViewController>().SetAbreviation(newLiquid.abreviation);
            newLiquidSwatch.GetComponent<LiquidSwatchViewController>().SetColor(newLiquid.color);
        });
    }
}

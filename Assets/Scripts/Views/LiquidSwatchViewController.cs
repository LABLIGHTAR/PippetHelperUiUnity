using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class LiquidSwatchViewController : MonoBehaviour
{
    public Transform abreviation;
    public SpriteRenderer swatch;

    void Start()
    {
        SessionState.clickStream.Subscribe(selectedObject =>
        {
            if(selectedObject.GetComponent<LiquidSwatchViewController>() && selectedObject.GetComponent<LiquidSwatchViewController>().abreviation == abreviation && !SessionState.FormActive)
            {
                SessionState.SetActiveLiquid(SessionState.AvailableLiquids.Where(x => x.abreviation.Equals(abreviation.GetComponent<TMP_Text>().text)).FirstOrDefault());
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color newColor)
    {
        swatch.color = newColor;
    }

    public bool SetAbreviation(string textInput)
    {
        if(textInput != null & textInput.Length <= 3)
        {
            abreviation.GetComponent<TMP_Text>().text = textInput;
            return true;
        }
        return false;
    }
}

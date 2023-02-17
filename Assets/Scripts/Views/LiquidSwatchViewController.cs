using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LiquidSwatchViewController : MonoBehaviour
{
    public Transform abreviation;
    public SpriteRenderer swatch;

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

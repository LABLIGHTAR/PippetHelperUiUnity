using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;

public class LiquidSwatchViewController : MonoBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI abreviation;
    public Image swatch;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left & !SessionState.FormActive)
        {
            SessionState.SetActiveLiquid(SessionState.AvailableLiquids.Where(x => x.abreviation.Equals(abreviation.GetComponent<TMP_Text>().text)).FirstOrDefault());
        }
    }

    public void SetColor(Color newColor)
    {
        swatch.color = newColor;
    }

    public bool SetAbreviation(string textInput)
    {
        if(textInput != null & textInput.Length <= 4)
        {
            abreviation.GetComponent<TMP_Text>().text = textInput;
            return true;
        }
        return false;
    }
}

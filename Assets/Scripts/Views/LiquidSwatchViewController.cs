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
    public TextMeshProUGUI name;
    public TextMeshProUGUI abreviation;
    public TextMeshProUGUI volume;
    public Image swatch;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left & !SessionState.FormActive)
        {
            SessionState.SetActiveLiquid(SessionState.AvailableLiquids.Where(x => x.abreviation.Equals(abreviation.GetComponent<TMP_Text>().text)).FirstOrDefault());
        }
    }

    public bool InitLiquidItem(string liquidName, string liquidAbrev, string liquidVolume, Color displayColor)
    {
        swatch.color = displayColor;
        name.GetComponent<TMP_Text>().text = liquidName;
        abreviation.GetComponent<TMP_Text>().text = liquidAbrev;
        volume.GetComponent<TMP_Text>().text = liquidVolume + "μL";

        return true;
    }
}

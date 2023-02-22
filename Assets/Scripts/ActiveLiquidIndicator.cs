using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ActiveLiquidIndicator : MonoBehaviour
{
    public SpriteRenderer indicator;
    // Start is called before the first frame update
    void Start()
    {
        indicator.enabled = false;

        SessionState.activeLiquidStream.Subscribe(liquid => UpdateIndicatorColor(liquid.color));
    }

    // Update is called once per frame
    void Update()
    {
        if(SessionState.ActiveLiquid != null)
        {
            indicator.enabled = true;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            indicator.transform.position = mousePosition;
        }
        else
        {
            indicator.enabled = false;
        }
    }

    void UpdateIndicatorColor(Color newColor)
    {
        indicator.color = newColor;
    }
}

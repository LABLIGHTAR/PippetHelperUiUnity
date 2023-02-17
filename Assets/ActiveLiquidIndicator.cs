using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveLiquidIndicator : MonoBehaviour
{
    public SpriteRenderer indicator;
    // Start is called before the first frame update
    void Start()
    {
        indicator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(SessionState.activeLiquid != null)
        {
            indicator.enabled = true;
            indicator.color = SessionState.activeLiquid.color;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            indicator.transform.position = mousePosition;
        }
        else
        {
            indicator.enabled = false;
        }
    }
}

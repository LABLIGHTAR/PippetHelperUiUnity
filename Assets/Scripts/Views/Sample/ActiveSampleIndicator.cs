using UnityEngine;
using UniRx;

public class ActiveSampleIndicator : MonoBehaviour
{
    public SpriteRenderer indicator;
    // Start is called before the first frame update
    void Start()
    {
        indicator.enabled = false;

        SessionState.activeSampleStream.Subscribe(Sample => UpdateIndicatorColor(Sample.color)).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(SessionState.ActiveSample != null)
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

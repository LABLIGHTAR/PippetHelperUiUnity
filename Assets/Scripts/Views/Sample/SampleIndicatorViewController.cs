using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleIndicatorViewController : MonoBehaviour
{
    public Transform sprite;

    public void Resize(float yScale)
    {
        transform.localScale = new Vector3(sprite.localScale.x, yScale, sprite.localScale.z);
    }
}

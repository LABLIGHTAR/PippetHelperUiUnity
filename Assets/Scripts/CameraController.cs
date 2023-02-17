using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public SpriteRenderer wellplate;
    // Start is called before the first frame update
    void Start()
    {
        float orthoSize = wellplate.bounds.size.x * Screen.height / Screen.width;

        Camera.main.orthographicSize = orthoSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

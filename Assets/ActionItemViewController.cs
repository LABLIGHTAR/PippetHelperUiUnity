using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionItemViewController : MonoBehaviour
{
    public TextMeshProUGUI actionText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitActionItem(string text)
    {
        actionText.text = text;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProtocolListItemViewController : MonoBehaviour
{
    public TextMeshProUGUI protocolName;

    private string filePath;

    public void InitItem(string name, string path)
    {
        protocolName.text = name;
        filePath = path;
    }

    public string GetPath()
    {
        return filePath;
    }
}

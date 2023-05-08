using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProtocolListItemViewController : MonoBehaviour
{
    public TextMeshProUGUI protocolName;
    public TextMeshProUGUI creationDate;
    public TextMeshProUGUI lastEdit;

    public Button editButton;
    public Button uploadButton;
    public Button deleteButton;

    private string filePath;

    public void InitItem(string name, string path)
    {
        protocolName.text = name;
        filePath = path;

        creationDate.text = "Created: " + File.GetCreationTime(filePath).ToString();
        lastEdit.text = "Last Edited: " + File.GetLastWriteTime(filePath).ToString();

        uploadButton.onClick.AddListener(delegate
        {
            File.Copy(filePath, Path.Combine(@Application.temporaryCachePath, "..", "new_protocols", protocolName.text + ".csv"));
        });

        deleteButton.onClick.AddListener(delegate
        {
            File.Delete(filePath);
            Destroy(this.gameObject);
        });
    }

    public string GetPath()
    {
        return filePath;
    }
}

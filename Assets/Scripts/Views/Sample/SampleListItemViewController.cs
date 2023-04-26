using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SampleListItemViewController : MonoBehaviour
{
    public Button selectButton;
    [SerializeField]
    private Button deleteButton;
    [SerializeField]
    private TextMeshProUGUI listName;
    [SerializeField]
    private TextMeshProUGUI creationDate;

    private string filePath;

    public void InitItem(string path)
    {
        filePath = path;
        listName.text = Path.GetFileNameWithoutExtension(filePath);
        creationDate.text = "Created: " + File.GetCreationTime(filePath).ToString();

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

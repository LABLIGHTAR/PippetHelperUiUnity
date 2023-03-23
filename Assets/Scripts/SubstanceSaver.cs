using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB; //Copyright (c) 2017 Gökhan Gökçe Under MIT License

public class SubstanceSaver : MonoBehaviour
{
    public Button saveSubstancesButton;

    private string filePath;

    private string delimiter = ",";

    // Start is called before the first frame update
    void Start()
    {
        saveSubstancesButton.onClick.AddListener(SaveSubstances);
    }

    public void SaveSubstances()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

        if (SessionState.ProcedureName != null)
        {
            filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", SessionState.ProcedureName + "_Sample_List", extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
            Debug.Log(filePath);
        }
        else
        {
            filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
            Debug.Log(filePath);
        }

        if(filePath == null || filePath == "")
        {
            return;
        }

        StreamWriter sw = new StreamWriter(filePath);

        foreach (Sample sample in SessionState.AvailableSamples)
        {
            sw.WriteLine(sample.sampleName + delimiter + sample.abreviation + delimiter + sample.colorName + delimiter + Color32ToHex(sample.color));
        }

        sw.Close();

        Debug.Log("CSV file written to: " + filePath);
    }

    public static string Color32ToHex(Color32 color)
    {
        // Convert the color values to hexadecimal strings
        string hexR = color.r.ToString("X2");
        string hexG = color.g.ToString("X2");
        string hexB = color.b.ToString("X2");

        // Combine the hexadecimal strings into a single color code
        string hexColor = "#" + hexR + hexG + hexB;

        return hexColor;
    }
}

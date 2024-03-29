﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class SubstanceSaver : MonoBehaviour
{
    public Button saveSubstancesButton;
    public TextMeshProUGUI saveMessage;

    private string folderPath;
    private string filePath;

    private string delimiter = ",";

    // Start is called before the first frame update
    void Start()
    {
        saveSubstancesButton.onClick.AddListener(SaveSubstances);

        //check if sample list folder exists
        folderPath = Path.Combine(@Application.temporaryCachePath, "..", "sample_lists");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public void SaveSubstances()
    {
        if(SessionState.AvailableSamples.Count <= 0)
        {
            Debug.LogWarning("Sample list empty, nothing to save...");
            return;
        }

        if (SessionState.ProcedureName != null)
        {
            filePath = Path.Combine(folderPath, SessionState.ProcedureName + "_Sample_List.csv");
            Debug.Log(filePath);
        }

        if(filePath == null || filePath == "")
        {
            return;
        }

        StreamWriter sw = new StreamWriter(filePath);

        foreach (LabMaterial material in SessionState.Materials)
        {
            if (material.GetSampleList() != null)
            {
                foreach (Sample sample in material.GetSampleList())
                {
                    sw.WriteLine(sample.sampleName + delimiter + sample.abreviation + delimiter + sample.colorName + delimiter + Color32ToHex(sample.color) + delimiter + material.materialName);
                }
            }
        }

        sw.Close();

        Debug.Log("CSV file written to: " + filePath);
        StartCoroutine(ShowSaveMessage());
    }

    private IEnumerator ShowSaveMessage()
    {
        saveMessage.gameObject.SetActive(true);

        saveMessage.color = new Color(saveMessage.color.r, saveMessage.color.g, saveMessage.color.b, 1);
        while (saveMessage.color.a > 0.0f)
        {
            saveMessage.color = new Color(saveMessage.color.r, saveMessage.color.g, saveMessage.color.b, saveMessage.color.a - (Time.deltaTime * 1f));
            yield return null;
        }

        saveMessage.gameObject.SetActive(false);
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

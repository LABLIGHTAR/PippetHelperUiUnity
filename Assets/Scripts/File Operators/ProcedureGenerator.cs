using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class ProcedureGenerator : MonoBehaviour
{
    public Button generateProcedureButton;

    public TextMeshProUGUI saveMessage;

    private string folderPathTemp;
    private string folderPathLighthouse;
    private string folderPathPersistent;
    private string tempFilePath;
    private string filePath;

    private string delimiter = ",";

    private List<Sample> addedSamples;

    private int currentPlateId;

    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(delegate
        {
            GenerateProcedure();
            StartCoroutine(ShowSaveMessage());
        });

        addedSamples = new List<Sample>();

        //check if new protocol folder exists
        folderPathTemp = Path.Combine(@Application.temporaryCachePath, "..", "temp");
        folderPathLighthouse = Path.Combine(@Application.temporaryCachePath, "..", "new_protocols");
        folderPathPersistent = Path.Combine(@Application.temporaryCachePath, "..", "saved_protocols");
        if (!Directory.Exists(folderPathLighthouse))
        {
            Directory.CreateDirectory(folderPathLighthouse);
        }
        if (!Directory.Exists(folderPathPersistent))
        {
            Directory.CreateDirectory(folderPathPersistent);
        }
        if(!Directory.Exists(folderPathTemp))
        {
            Directory.CreateDirectory(folderPathTemp);
        }
    }

    public void GenerateProcedure()
    {
        if(SessionState.ProcedureName != null)
        {
            tempFilePath = Path.Combine(folderPathTemp, SessionState.ProcedureName + ".csv");
            filePath = Path.Combine(folderPathLighthouse, SessionState.ProcedureName + ".csv");
        }
        else
        {
            return;
        }

        StreamWriter sw = new StreamWriter(tempFilePath);

        foreach(LabMaterial material in SessionState.Materials)
        {
            var samples = material.GetSampleList();
            if (samples != null)
            {
                foreach(var sample in samples)
                {
                    string matID = material.id + ":" + samples.IndexOf(sample);
                    sw.WriteLine("material" + delimiter + material.materialName + delimiter + "horizontal" + delimiter + matID + delimiter + sample.sampleName + ":" + sample.abreviation + delimiter + Color32ToHex(sample.color) + ":" + sample.colorName);
                }
            }
            else
            {
                sw.WriteLine("material" + delimiter + material.materialName + ":" + material.customName + delimiter + "horizontal" + delimiter + material.id + delimiter);
            }
        }

        foreach (Step step in SessionState.Steps)
        {
            //write step start code
            sw.WriteLine("step");

            foreach (var action in step.actions)
            {
                string actionString = delimiter + "action:" + action.type.ToString() + delimiter + action.source.matID + ":" + action.source.matSubID;
                if (action.SourceIsWellplate() && action.numChannels > 1)
                {
                    actionString += ";" + action.numChannels;
                }
                actionString += delimiter + Color32ToHex(action.source.color) + ":" + action.source.colorName + delimiter + action.source.volume + delimiter + "μL" + delimiter + action.target.matID + ":" + action.target.matSubID;
                if(action.TargetIsWellplate() && action.numChannels > 1)
                {
                    actionString += ";" + action.numChannels;
                }
                sw.WriteLine(actionString + delimiter + Color32ToHex(action.target.color) + ":" + action.target.colorName);
            }
            
            //write step end code
            sw.WriteLine("end");
        }

        sw.Close();
        Debug.Log("CSV file written to: " + tempFilePath);

        if (File.Exists(filePath))
        {
            Debug.Log("File already exists, overwriting " + filePath);
            File.Delete(filePath);
        }
        File.Move(tempFilePath, filePath);
        Debug.Log("CSV file moved to: " + filePath);

        string persistantPath = Path.Combine(folderPathPersistent, SessionState.ProcedureName + ".csv");
        if (File.Exists(persistantPath))
        {
            Debug.Log("File already exists, overwriting " + persistantPath);
            File.Delete(persistantPath);
        }
        File.Copy(filePath, persistantPath);
        Debug.Log("CSV file copied to: " + persistantPath);
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

    string FindGroupEnd(int groupId, int plateId)
    {
            foreach (Wellplate plate in SessionState.Materials)
            {
                if(plate.id == plateId)
                {
                    foreach (var well in plate.wells)
                    {
                        foreach (var group in well.Value.groups)
                        {
                            if (group.groupId == groupId && group.isEnd)
                            {
                                return well.Key;
                            }
                        }
                    }
                }
            }
        return null;
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

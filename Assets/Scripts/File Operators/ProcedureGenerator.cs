using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB; //Copyright (c) 2017 Gökhan Gökçe Under MIT License

public class ProcedureGenerator : MonoBehaviour
{
    public Button generateProcedureButton;

    private string folderPath;
    private string filePath;

    private string delimiter = ",";

    private List<Sample> addedSamples;

    private int currentPlateId;

    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(GenerateProcedure);

        addedSamples = new List<Sample>();
#if UNITY_STANDALONE && !UNITY_EDITOR
        //check if new protocol folder exists
        folderPath = Path.Combine(@Application.temporaryCachePath, "..", "new_protocols");
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
#endif
    }

    public void GenerateProcedure()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

        if(SessionState.ProcedureName != null)
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
                filePath = StandaloneFileBrowser.SaveFilePanel("Save File", folderPath, SessionState.ProcedureName, extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif
#if UNITY_EDITOR
               filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", SessionState.ProcedureName, extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif
        }
        else
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
                filePath = StandaloneFileBrowser.SaveFilePanel("Save File", folderPath, "", extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif
#if UNITY_EDITOR
                filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif
        }

        StreamWriter sw = new StreamWriter(filePath);

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
                sw.WriteLine("material" + delimiter + material.materialName + delimiter + "horizontal" + delimiter + material.id);
            }
        }

        foreach (Step step in SessionState.Steps)
        {
            //write step start code
            sw.WriteLine("step");

            foreach (var action in step.actions)
            {
                sw.WriteLine("action:" + action.type.ToString() + delimiter + action.source.matID + ":" + action.source.matSubID + delimiter + Color32ToHex(action.source.color) + ":" + action.source.colorName + delimiter + action.source.volume + delimiter + "μL" + delimiter + action.target.matID + ":" + action.target.matSubID + delimiter + Color32ToHex(action.target.color) + ":" + action.target.colorName);
            }
            
            //write step end code
            sw.WriteLine("end");
        }

        sw.Close();

        Debug.Log("CSV file written to: " + filePath);
    }

    string FindGroupEnd(int groupId, int plateId)
    {
        foreach (Step step in SessionState.Steps)
        {
            foreach (Wellplate plate in step.materials)
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

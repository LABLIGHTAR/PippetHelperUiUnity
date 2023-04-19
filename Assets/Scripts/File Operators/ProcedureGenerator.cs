using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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

        //check if new protocol folder exists
        folderPath = Path.Combine(@Application.temporaryCachePath, "..", "new_protocols");
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public void GenerateProcedure()
    {
        if(SessionState.ProcedureName != null)
        {
            filePath = Path.Combine(folderPath, SessionState.ProcedureName + ".csv");
        }
        else
        {
            return;
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
                sw.WriteLine("material" + delimiter + material.materialName + delimiter + "horizontal" + delimiter + material.id + delimiter);
            }
        }

        foreach (Step step in SessionState.Steps)
        {
            //write step start code
            sw.WriteLine("step");

            foreach (var action in step.actions)
            {
                string actionString = delimiter + "action:" + action.type.ToString() + delimiter + action.source.matID + ":" + action.source.matSubID;
                if (action.SourceIsWellplate())
                {
                    actionString += ";" + action.numChannels;
                }
                actionString += delimiter + Color32ToHex(action.source.color) + ":" + action.source.colorName + delimiter + action.source.volume + delimiter + "μL" + delimiter + action.target.matID + ":" + action.target.matSubID;
                if(action.TargetIsWellplate())
                {
                    actionString += ";" + action.numChannels;
                }
                sw.WriteLine(actionString + delimiter + Color32ToHex(action.target.color) + ":" + action.target.colorName);
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

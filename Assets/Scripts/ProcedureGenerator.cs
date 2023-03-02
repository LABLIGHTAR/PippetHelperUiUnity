using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB; //Copyright (c) 2017 Gökhan Gökçe Under MIT License

public class ProcedureGenerator : MonoBehaviour
{
    public Button generateProcedureButton;

    private string filePath;

    private string delimiter = ",";

    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(GenerateProcedure);

        filePath = Application.dataPath + "/example.csv";
    }
    
    void GenerateProcedure()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

        if(SessionState.ProcedureName != null)
        {
            filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", SessionState.ProcedureName, extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
            Debug.Log(filePath);
        }
        else
        {
            filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", extensionList); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
            Debug.Log(filePath);
        }

        StreamWriter sw = new StreamWriter(filePath);

        foreach (SessionState.WellPlate step in SessionState.Steps)
        {
            sw.WriteLine("plate:horizontal" + delimiter);
            //iterate through each well
            foreach (var well in step.wells)
            {
                //iterate through each Sample in the well
                foreach (var Sample in well.Value.Samples)
                {
                    string groupStart = null;
                    string groupEnd = null;
                    bool isGrouped = false;
                    int groupId = -1;

                    //check if this Sample is a part of a group
                    foreach (var group in well.Value.groups)
                    {
                        Debug.Log(group.groupId);
                        if(group.Sample == Sample)
                        {
                            isGrouped = true;
                            groupId = group.groupId;
                            if(group.isStart)
                            {
                                groupStart = well.Key;
                            }
                        }
                    }
                    if (isGrouped)
                    {
                        if (groupStart != null)
                        {
                            groupEnd = FindGroupEnd(groupId);

                            //Debug.Log(groupStart + ":" + groupEnd + " " + Sample.color + " " + Sample.colorName + Sample.name);
                            sw.WriteLine(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(Sample.color).ToString() + delimiter + Sample.colorName + delimiter + Sample.name.Remove(Sample.name.Length - 1, 1) + delimiter + Sample.volume + delimiter + "μL");
                        }
                    }
                    else
                    {
                        //Debug.Log(well.Key + " " + Sample.color + " " + Sample.colorName + Sample.name);
                        sw.WriteLine(delimiter + well.Key + delimiter + Color32ToHex(Sample.color).ToString() + delimiter + Sample.colorName + delimiter + Sample.name.Remove(Sample.name.Length - 1, 1) + delimiter + Sample.volume + delimiter + "μL");
                    }
                }
            }
        }

        sw.Close();

        Debug.Log("CSV file written to: " + filePath);
    }

    string FindGroupEnd(int Id)
    {
        foreach (SessionState.WellPlate step in SessionState.Steps)
        {
            foreach (var well in step.wells)
            {
                foreach (var group in well.Value.groups)
                {
                    if(group.groupId == Id && group.isEnd)
                    {
                        return well.Key;
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

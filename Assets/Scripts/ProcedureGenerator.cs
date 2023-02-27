using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

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

        filePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", extensionList);
        Debug.Log(filePath);

        StreamWriter sw = new StreamWriter(filePath);

        foreach (SessionState.WellPlate step in SessionState.Steps)
        {
            sw.WriteLine("plate:horizontal" + delimiter);
            //iterate through each well
            foreach (var well in step.wells)
            {
                //iterate through each liquid in the well
                foreach (var liquid in well.Value.liquids)
                {
                    string groupStart = null;
                    string groupEnd = null;
                    bool isGrouped = false;
                    int groupId = -1;

                    //check if this liquid is a part of a group
                    foreach (var group in well.Value.groups)
                    {
                        Debug.Log(group.groupId);
                        if(group.liquid == liquid)
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

                            //Debug.Log(groupStart + ":" + groupEnd + " " + liquid.color + " " + liquid.colorName + liquid.name);
                            sw.WriteLine(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(liquid.color).ToString() + delimiter + liquid.colorName + delimiter + liquid.name.Remove(liquid.name.Length - 1, 1) + delimiter + liquid.volume + delimiter + "μL");
                        }
                    }
                    else
                    {
                        //Debug.Log(well.Key + " " + liquid.color + " " + liquid.colorName + liquid.name);
                        sw.WriteLine(delimiter + well.Key + delimiter + Color32ToHex(liquid.color).ToString() + delimiter + liquid.colorName + delimiter + liquid.name.Remove(liquid.name.Length - 1, 1) + delimiter + liquid.volume + delimiter + "μL");
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

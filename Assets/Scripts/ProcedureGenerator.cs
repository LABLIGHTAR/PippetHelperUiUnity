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

    private List<SessionState.Sample> addedSamples;

    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(GenerateProcedure);

        filePath = Application.dataPath + "/example.csv";

        addedSamples = new List<SessionState.Sample>();
    }
    
    public void GenerateProcedure()
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
            //clear added samples list
            addedSamples.Clear();

            //write step start code
            sw.WriteLine("plate:horizontal" + delimiter);

            //iterate through each well to go start from the top left of the plate
            foreach (var well in step.wells)
            {
                Debug.Log(well.Key);
                //iterate through each Sample in the well
                foreach (var sample in well.Value.Samples)
                {
                    //if this sample has already been added continue to the next sample
                    if(addedSamples.Contains(sample))
                    {
                        continue;
                    }
                    //iterate through each well again to order actions by sample
                    foreach (var well2 in step.wells)
                    {
                        //check if each well contains the sample
                        if(well2.Value.Samples.Contains(sample))
                        {
                            //reset grouping vars
                            string groupStart = null;
                            string groupEnd = null;
                            bool isGrouped = false;
                            int groupId = -1;

                            //check if this Sample is a part of a group
                            foreach (var group in well2.Value.groups)
                            {
                                if (group.Sample == sample)
                                {
                                    isGrouped = true;
                                    groupId = group.groupId;

                                    //check if this is the start of the group
                                    if (group.isStart)
                                    {
                                        groupStart = well2.Key;
                                    }
                                }
                            }
                            if (isGrouped)
                            {
                                //if this well was the start of the sample group add the group to the csv output
                                if (groupStart != null)
                                {
                                    groupEnd = FindGroupEnd(groupId);
                                    Debug.Log(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(sample.color).ToString() + delimiter + sample.colorName + delimiter + sample.name + ":" + sample.abreviation + delimiter + sample.volume + delimiter + "μL");
                                    sw.WriteLine(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(sample.color).ToString() + delimiter + sample.colorName + delimiter + sample.name + ":" + sample.abreviation + delimiter + sample.volume + delimiter + "μL");
                                }
                            }
                            //if this is a single sample well add the single sample entry to the csv output
                            else
                            {
                                Debug.Log(delimiter + well2.Key + delimiter + Color32ToHex(sample.color).ToString() + delimiter + sample.colorName + delimiter + sample.name + ":" + sample.abreviation + delimiter + sample.volume + delimiter + "μL");
                                sw.WriteLine(delimiter + well2.Key + delimiter + Color32ToHex(sample.color).ToString() + delimiter + sample.colorName + delimiter + sample.name + ":" + sample.abreviation + delimiter + sample.volume + delimiter + "μL");
                            }
                        }
                    }
                    //once finished looking at all wells add this sample to the added samples list
                    addedSamples.Add(sample);
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

using System.Collections;
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

    private List<SessionState.Sample> addedSamples;

    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(GenerateProcedure);


        addedSamples = new List<SessionState.Sample>();
#if UNITY_STANDALONE && !UNITY_EDITOR
        //check if new protocol folder exists
        folderPath = @"%userprofile%\AppData\Local\Temp\LablightAR\new_protocols";
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

        foreach (SessionState.WellPlate step in SessionState.Steps)
        {
            //clear added samples list
            addedSamples.Clear();

            //write step start code
            sw.WriteLine("plate:horizontal" + delimiter + step.wells.Count);

            //iterate through each well to go start from the top left of the plate
            foreach (var well in step.wells)
            {
                //iterate through each Sample in the well
                foreach (var sample in well.Value.Samples)
                {
                    //if this sample has already been added continue to the next sample
                    if(addedSamples.Contains(sample.Key))
                    {
                        continue;
                    }
                    //iterate through each well again to order actions by sample
                    foreach (var well2 in step.wells)
                    {
                        //check if each well contains the sample
                        if(well2.Value.Samples.ContainsKey(sample.Key))
                        {
                            //reset grouping vars
                            string groupStart = null;
                            string groupEnd = null;
                            bool isGrouped = false;
                            int groupId = -1;

                            //get the volume in this well
                            float volume;
                            well2.Value.Samples.TryGetValue(sample.Key, out volume);

                            //check if this Sample is a part of a group
                            foreach (var group in well2.Value.groups)
                            {
                                if (group.Sample == sample.Key)
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
                                    //Debug.Log(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(sample.Key.color).ToString() + delimiter + sample.Key.colorName + delimiter + sample.Key.name + ":" + sample.Key.abreviation + delimiter + volume.ToString() + delimiter + "μL");
                                    sw.WriteLine(delimiter + groupStart + ":" + groupEnd + delimiter + Color32ToHex(sample.Key.color).ToString() + delimiter + sample.Key.colorName + delimiter + sample.Key.name + ":" + sample.Key.abreviation + delimiter + volume.ToString() + delimiter + "μL");
                                }
                            }
                            //if this is a single sample well add the single sample entry to the csv output
                            else
                            {
                                //Debug.Log(delimiter + well2.Key + delimiter + Color32ToHex(sample.Key.color).ToString() + delimiter + sample.Key.colorName + delimiter + sample.Key.name + ":" + sample.Key.abreviation + delimiter + volume.ToString() + delimiter + "μL");
                                sw.WriteLine(delimiter + well2.Key + delimiter + Color32ToHex(sample.Key.color).ToString() + delimiter + sample.Key.colorName + delimiter + sample.Key.name + ":" + sample.Key.abreviation + delimiter + volume.ToString() + delimiter + "μL");
                            }
                        }
                    }
                    //once finished looking at all wells add this sample to the added samples list
                    addedSamples.Add(sample.Key);
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

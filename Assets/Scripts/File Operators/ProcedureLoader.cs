using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;
using SFB;//Copyright (c) 2017 Gökhan Gökçe Under MIT License

public class ProcedureLoader : MonoBehaviour
{
    public static Subject<bool> procedureStream = new Subject<bool>();
    public static Subject<int> materialsLoadedStream = new Subject<int>();

    private string folderPath;

    private string[] fileName;

    private bool startOfDilution = true;

    // Start is called before the first frame update
    void Start()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

#if UNITY_STANDALONE && !UNITY_EDITOR
        //check if protocol folder exists
        folderPath = Application.dataPath + "/../protocols";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        //open file
        fileName = StandaloneFileBrowser.OpenFilePanel("Open File", folderPath, extensionList, true); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif
#if UNITY_EDITOR
        fileName = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensionList, true); //Copyright (c) 2017 Gökhan Gökçe Under MIT License
#endif


        if (fileName.Count() > 0)
        {
            StartCoroutine(LoadProcedure(fileName[0]));
        }
        else
        {
            Application.Quit();
        }
    }

    ///loads procedure from csv (async to ensure all objects in scene are instantiated fully before loading)
    IEnumerator LoadProcedure(string fileName)
    {
        yield return new WaitForEndOfFrame();

        StreamReader sr = new StreamReader(fileName);
        string currentLine;
        string[] lineCells;

        //read the file until the end of file is reached
        while ((currentLine = sr.ReadLine()) != null)
        {
            ParseLine(currentLine);
        }

        materialsLoadedStream.OnNext(SessionState.Materials.Count);
        yield return new WaitForEndOfFrame();

        SessionState.SetActiveStep(0);
        SessionState.ActiveSample = null;
        
        procedureStream.OnNext(true);

        if (fileName != null)
        {
            SessionState.ProcedureName = Path.GetFileNameWithoutExtension(fileName);
        }
    }

    void ParseLine(string currentLine)
    {
        //parse materials
        if (currentLine.Contains("material"))
        {
            ParseMaterial(currentLine);
        }

        //parse step
        else if (currentLine.Contains("step"))
        {
            SessionState.AddNewStep();
        }

        //parse actions
        else if (currentLine.Contains("action"))
        {
           ParseAction(currentLine);
        }
    }

    void ParseMaterial(string currentLine)
    {
        string[] lineCells = currentLine.Split(',');

        //cells go: [0]start code, [1]material name, [2]material orientation,
        //[3]material_id:material_subID, [4]contents, [5]contentColorHex:contentColorName
        string materialName = lineCells[1];

        int numWells = 0;
        if (materialName.Contains("wellplate"))
        {
            numWells = int.Parse(Regex.Match(lineCells[1], @"\d+").Value);
        }

        string orientation = lineCells[2];

        int materialID;
        string materialSubID = "";
        if (lineCells[3].Contains(":"))
        {
            string[] IDs = lineCells[3].Split(":");
            materialID = int.Parse(IDs[0]);
            materialSubID = IDs[1];
        }
        else
        {
            materialID = int.Parse(lineCells[3]);
        }

        string contentsName = "";
        string contentsAbrev = "";
        Color contentsColor = Color.white;
        string contentsColorName = "";
        if (lineCells.Length > 5)
        {
            string[] contents = lineCells[4].Split(":");
            contentsName = contents[0];
            contentsAbrev = contents[1];

            string[] color = lineCells[5].Split(":");
            ColorUtility.TryParseHtmlString(color[0], out contentsColor);
            contentsColorName = color[1];
        }

        //add materials to session state
        if (materialName.Contains("wellplate"))
        {
            SessionState.Materials.Add(new Wellplate(materialID, materialName, numWells));
        }
        else if (materialName.Contains("tuberack5ml"))
        {
            if (materialSubID != "" && materialSubID == "0")
            {
                var newTubeRack = new TubeRack5mL(materialID, materialName);
                SessionState.Materials.Add(newTubeRack);
                if (contentsColor != Color.white && contentsColorName != "")
                {
                    Sample newSample = new Sample(contentsName, contentsAbrev, contentsColorName, contentsColor);
                    newTubeRack.AddNewSample(newSample);
                }
            }
            else if (materialSubID != "" && materialSubID != "0")
            {
                var tubeRack = SessionState.Materials[materialID];
                if (contentsColor != Color.white && contentsColorName != "")
                {
                    Sample newSample = new Sample(contentsName, contentsAbrev, contentsColorName, contentsColor);
                    tubeRack.AddNewSample(newSample);
                }
            }
        }
        else if (materialName.Contains("reservoir"))
        {
            var newReservoir = new Reservoir(materialID, materialName);
            SessionState.Materials.Add(newReservoir);
            if (contentsColor != Color.white && contentsColorName != "")
            {
                Sample newSample = new Sample(contentsName, contentsAbrev, contentsColorName, contentsColor);
                newReservoir.AddNewSample(newSample);
            }
        }
    }

    void ParseAction(string currentLine)
    {
        string[] lineCells = currentLine.Split(',');
        //cells go: [0]blank, [1]action code, [2]SourceID:SourceSubID, [3]SourceHex:SourceColorName,
        //[4]SampleVolume, [5]SampleUnits, [6]targetID:targetSubID, [7]TargetHex:TargetColorName

        LabAction.ActionType actionType;
        Enum.TryParse<LabAction.ActionType>(lineCells[1].Split(":")[1], out actionType);

        string[] sourceIDs = lineCells[2].Split(':');
        string sourceID = sourceIDs[0];
        string sourceSubID = sourceIDs[1];

        Color sourceColor;
        string sourceColorName;
        string[] sourceColors = lineCells[3].Split(':');
        ColorUtility.TryParseHtmlString(sourceColors[0], out sourceColor);
        sourceColorName = sourceColors[1];

        float volume = float.Parse(lineCells[4], CultureInfo.InvariantCulture.NumberFormat);

        string[] targetIDs;
        string targetID = "";
        string targetSubID = "";
        if (lineCells[6].Contains(":"))
        {
            targetIDs = lineCells[6].Split(':');
            targetID = targetIDs[0];
            targetSubID = targetIDs[1];
        }
        else
        {
            targetID = lineCells[6];
        }

        Color targetColor;
        string targetColorName;
        string[] targetColors = lineCells[7].Split(':');
        ColorUtility.TryParseHtmlString(targetColors[0], out targetColor);
        targetColorName = targetColors[1];

        if (actionType == LabAction.ActionType.pipette)
        {
            startOfDilution = true;
            //set actions sample as active  
            SessionState.ActiveSample = SessionState.AvailableSamples.Where(sample => sample.color == sourceColor).FirstOrDefault();
            //set active tool to match action
            if (targetSubID != "" && targetSubID.Contains("-"))
            {
                string[] wellGroup = targetSubID.Split('-');
                int numChannels;
                string activeWellId = wellGroup[0];
                if (wellGroup[0][0] == wellGroup[1][0])
                {
                    numChannels = GetNumberChannels(wellGroup, true);
                    SessionState.ActiveTool = new Tool("multichannel", numChannels, "row", volume);
                    while (numChannels > 0)
                    {
                        if (activeWellId == wellGroup[0])
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, true, false);
                        }
                        else if (activeWellId == wellGroup[1])
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, false, true);
                        }
                        else
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, false, false);
                        }
                        numChannels--;
                        activeWellId = GetNextWellHorizontal(activeWellId);
                    }
                }
                else
                {
                    numChannels = GetNumberChannels(wellGroup, false);
                    SessionState.ActiveTool = new Tool("multichannel", numChannels, "column", volume);
                    while (numChannels > 0)
                    {
                        if (activeWellId == wellGroup[0])
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, true, false);
                        }
                        else if (activeWellId == wellGroup[1])
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, false, true);
                        }
                        else
                        {
                            SessionState.AddActiveSampleToWell(activeWellId, int.Parse(targetID), true, false, false);
                        }
                        numChannels--;
                        activeWellId = GetNextWellVertical(activeWellId);
                    }
                }

            }
            else
            {
                SessionState.ActiveTool = new Tool("pipette", 1, "row", volume);
                SessionState.AddActiveSampleToWell(targetSubID, int.Parse(targetID), false, false, false);
            }
        }
        else if (actionType == LabAction.ActionType.transfer)
        {
            startOfDilution = true;
            if (!SessionState.CurrentStep.materials[int.Parse(sourceID)].ContainsWell(sourceSubID))
            {
                SessionState.CurrentStep.materials[int.Parse(sourceID)].AddWell(sourceSubID, new Well(targetSubID, int.Parse(sourceID)));
            }
            if (!SessionState.CurrentStep.materials[int.Parse(targetID)].ContainsWell(targetSubID))
            {
                SessionState.CurrentStep.materials[int.Parse(targetID)].AddWell(targetSubID, new Well(targetSubID, int.Parse(targetID)));
            }
            SessionState.AddTransferAction(SessionState.CurrentStep.materials[int.Parse(sourceID)].GetWell(sourceSubID), SessionState.CurrentStep.materials[int.Parse(targetID)].GetWell(targetSubID), volume);
        }
        else if (actionType == LabAction.ActionType.dilution)
        {
            if (startOfDilution)
            {
                if (SessionState.CurrentStep.materials[int.Parse(targetID)].ContainsWell(targetSubID))
                {
                    SessionState.AddDilutionActionStart(SessionState.Materials[int.Parse(sourceID)].GetSampleList()[int.Parse(sourceSubID)], SessionState.Materials[int.Parse(targetID)].GetWell(targetSubID), volume);
                }
                else
                {
                    SessionState.CurrentStep.materials[int.Parse(targetID)].AddWell(targetSubID, new Well(targetSubID, int.Parse(targetID)));
                    SessionState.AddDilutionActionStart(SessionState.Materials[int.Parse(sourceID)].GetSampleList()[int.Parse(sourceSubID)], SessionState.CurrentStep.materials[int.Parse(targetID)].GetWell(targetSubID), volume);
                }
                startOfDilution = false;
            }
            else
            {
                if (SessionState.CurrentStep.materials[int.Parse(sourceID)].ContainsWell(sourceSubID) && SessionState.CurrentStep.materials[int.Parse(targetID)].ContainsWell(targetSubID))
                {
                    SessionState.AddDilutionAction(SessionState.Materials[int.Parse(sourceID)].GetWell(sourceSubID), SessionState.Materials[int.Parse(targetID)].GetWell(targetSubID), volume);
                }
                else
                {
                    if(!SessionState.CurrentStep.materials[int.Parse(sourceID)].ContainsWell(sourceSubID))
                    {
                        SessionState.CurrentStep.materials[int.Parse(sourceID)].AddWell(sourceSubID, new Well(targetSubID, int.Parse(sourceID)));
                    }
                    if (!SessionState.CurrentStep.materials[int.Parse(targetID)].ContainsWell(targetSubID))
                    {
                        SessionState.CurrentStep.materials[int.Parse(targetID)].AddWell(targetSubID, new Well(targetSubID, int.Parse(targetID)));
                    }
                    SessionState.AddDilutionAction(SessionState.CurrentStep.materials[int.Parse(sourceID)].GetWell(sourceSubID), SessionState.CurrentStep.materials[int.Parse(targetID)].GetWell(targetSubID), volume);
                }
            }
        }
    }

    //returns number of channels from well group identifier
    int GetNumberChannels(string[] wellGroup, bool isHorizontal)
    {
        int startWellNumber;
        int endWellNumber;
        
        if (isHorizontal)
        {
            if (wellGroup[0].Length == 2)
            {
                startWellNumber = Int32.Parse(wellGroup[0][1].ToString());
            }
            else
            {
                char[] chars = { wellGroup[0][1], wellGroup[0][2] };
                startWellNumber = Int32.Parse(new string(chars));
            }
            if (wellGroup[1].Length == 2)
            {
                endWellNumber = Int32.Parse(wellGroup[1][1].ToString());
            }
            else
            {
                char[] chars = { wellGroup[1][1], wellGroup[1][2] };
                endWellNumber = Int32.Parse(new string(chars));
            }
        }
        else
        {
            startWellNumber = (wellGroup[0][0] - 64);
            endWellNumber = (wellGroup[1][0] - 64);
        }

        return endWellNumber - (startWellNumber - 1);
    }

    //returns the well id of the well to right
    string GetNextWellHorizontal(string currentWell)
    {
        int startWellNumber;

        if (currentWell.Length == 2)
        {
            startWellNumber = Int32.Parse(currentWell[1].ToString());
        }
        else
        {
            char[] chars = { currentWell[1], currentWell[2] };
            startWellNumber = Int32.Parse(new string(chars));
        }
        
        startWellNumber++;

        return new string(currentWell[0] + startWellNumber.ToString());
    }

    //returns the id of the well below
    string GetNextWellVertical(string currentWell)
    {
        string columnId = currentWell.Substring(1);

        char nextRowId = (char)(((int)currentWell[0]) + 1);

        return new string(nextRowId.ToString() + columnId);
    }
}

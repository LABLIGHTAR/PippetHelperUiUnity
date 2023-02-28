using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using UnityEngine;
using SFB;

public class ProcedureLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

        var fileName = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensionList, true)[0];

        Debug.Log(fileName);
        if(fileName != null)
        {
            LoadProcedure(fileName);
        }
    }

    void LoadProcedure(string fileName)
    {
        StreamReader sr = new StreamReader(fileName);

        string currentLine;

        string[] lineCells;

        bool firstStep = true;

        //read the file until the end of file is reached
        while((currentLine = sr.ReadLine()) != null)
        {
            if(currentLine.Contains("plate:horizontal"))
            {
                Debug.Log("new step");
                if(!firstStep)
                {
                    SessionState.AddNewStep();
                    SessionState.SetStep(SessionState.Step + 1);
                }
                else
                {
                    firstStep = false;
                }
            }
            else
            {
                lineCells = currentLine.Split(',');
                
                //cell 0 will always be blank
                //cell goes: "","wellID","#Hex","ColorName","LiquidName", "LiquidVolume"
                string wellId = lineCells[1];
                Color color;
                ColorUtility.TryParseHtmlString(lineCells[2], out color);
                string colorName = lineCells[3];
                string liquidName = lineCells[4];
                float liquidVolume = float.Parse(lineCells[5], CultureInfo.InvariantCulture.NumberFormat);

                //add liquid to sessionState
                SessionState.Liquid newLiquid = new SessionState.Liquid(liquidName, liquidName, colorName, color, liquidVolume);
                SessionState.AddNewLiquid(newLiquid.name, newLiquid.abreviation, newLiquid.colorName, newLiquid.color, newLiquid.volume);
                //set new liquid as active  
;               SessionState.ActiveLiquid = newLiquid;

                //if the well id has a colon this is a multichannel
                if(wellId.Contains(':'))
                {
                    int numChannels;

                    Debug.Log("Group");
                    string[] wellGroup = wellId.Split(':');
                    Debug.Log(wellGroup[0]);
                    Debug.Log(wellGroup[1]);

                    if (wellGroup[0][0] == wellGroup[1][0])
                    {
                        Debug.Log("Horizontal");
                        numChannels = GetNumberChannels(wellGroup, true);
                    }
                    else
                    {
                        Debug.Log("Vertical");
                        numChannels = GetNumberChannels(wellGroup, false);
                    }
                    Debug.Log("Number of channels: " + numChannels);
                }
                //else its a single well
                else
                {
                    Debug.Log(wellId);
                    Debug.Log(SessionState.AddActiveLiquidToWell(wellId, false, false, false));
                }
            }
        }
        SessionState.SetStep(0);
        SessionState.SetStep(SessionState.Step + 1);
        SessionState.SetStep(SessionState.Step - 1);
    }

    int GetNumberChannels(string[] wellGroup, bool isHorizontal)
    {
        int startWellNumber;
        int endWellNumber;
        int numChannels;

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
}

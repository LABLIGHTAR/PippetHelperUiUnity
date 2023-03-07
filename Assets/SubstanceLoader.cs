using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using SFB;//Copyright (c) 2017 G�khan G�k�e Under MIT License

public class SubstanceLoader : MonoBehaviour
{
    public Button loadSubstancesButton;
    public Button yesButton;
    public Button noButton;
    public GameObject savePanel;
    public Transform substanceList;

    // Start is called before the first frame update
    void Start()
    {
        loadSubstancesButton.onClick.AddListener(ShowConfirmation);
        yesButton.onClick.AddListener(LoadSubstances);
        noButton.onClick.AddListener(delegate { savePanel.SetActive(false); });
    }

    void ShowConfirmation()
    {
        savePanel.SetActive(true);
    }

    void LoadSubstances()
    {
        savePanel.SetActive(false);

        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
        };

        string[] fileName = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensionList, true); //Copyright (c) 2017 G�khan G�k�e Under MIT License
        
        if (!(fileName.Count() > 0))
        {
            return;
        }

        StreamReader sr = new StreamReader(fileName[0]);

        string currentLine;

        string[] lineCells;

        //clear session state substances
        SessionState.AvailableSamples.Clear();
        //clear substances from UI
        foreach (Transform child in substanceList) {
            Destroy(child.gameObject);
        }
        foreach(SessionState.WellPlate step in SessionState.Steps)
        {
            step.wells.Clear();
        }

        //read the file until the end of file is reached
        while ((currentLine = sr.ReadLine()) != null)
        {

            lineCells = currentLine.Split(',');

            //line goes: "sampleName","SampleAbreviation","ColorName","#ColorHex", "SampleVolume"
            string sampleName = lineCells[0];
            string sampleAbbreviation = lineCells[1];
            string sampleColorName = lineCells[2];
            Color sampleColor;
            ColorUtility.TryParseHtmlString(lineCells[3], out sampleColor);
            float sampleVolume = float.Parse(lineCells[4], CultureInfo.InvariantCulture.NumberFormat);

            //add Sample to sessionState
            SessionState.AddNewSample(sampleName, sampleAbbreviation, sampleColorName, sampleColor, sampleVolume);
        }
        ProcedureLoader.procedureStream.OnNext(true);
    }
}

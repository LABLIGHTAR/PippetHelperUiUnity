using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SFB;//Copyright (c) 2017 Gökhan Gökçe Under MIT License

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

        string[] fileName = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensionList, true); //Copyright (c) 2017 Gökhan Gökçe Under MIT License

        if (!(fileName.Count() > 0))
        {
            return;
        }

        StreamReader sr = new StreamReader(fileName[0]);

        string currentLine;

        string[] lineCells;

        //clear session state substances
        SessionState.Materials = SessionState.Materials.Where(m => m.GetSampleList() == null).ToList();

        //clear substances from UI
        foreach (Transform child in substanceList)
        {
            Destroy(child.gameObject);
        }
       

        //read the file until the end of file is reached
        while ((currentLine = sr.ReadLine()) != null)
        {

            lineCells = currentLine.Split(',');

            //line goes: "sampleName","SampleAbreviation","ColorName","#ColorHex", "vesselName"
            string sampleName = lineCells[0];
            string sampleAbbreviation = lineCells[1];
            string sampleColorName = lineCells[2];
            Color sampleColor;
            ColorUtility.TryParseHtmlString(lineCells[3], out sampleColor);
            string vesselType = lineCells[4];

            if(vesselType == "tuberack5ml")
            {
                vesselType = "5mL Tube";
            }
            else if(vesselType == "reservoir")
            {
                vesselType = "Reservoir";
            }    

            //add Sample to sessionState
            SessionState.AddNewSample(sampleName, sampleAbbreviation, sampleColorName, sampleColor, vesselType);
        }
    }
}

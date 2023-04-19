using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SubstanceLoader : MonoBehaviour
{
    public Button loadSubstancesButton;
    public Button yesButton;
    public Button noButton;
    public GameObject savePanel;
    public Transform substanceList;

    private string folderPath;
    private string[] fileNames;

    // Start is called before the first frame update
    void Start()
    {
        loadSubstancesButton.onClick.AddListener(ShowConfirmation);
        yesButton.onClick.AddListener(LoadSubstances);
        noButton.onClick.AddListener(delegate { savePanel.SetActive(false); });

        //check if sample list folder exists
        folderPath = Path.Combine(@Application.temporaryCachePath, "..", "sample_lists");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        fileNames = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);
    }

    void ShowConfirmation()
    {
        savePanel.SetActive(true);
    }

    void LoadSubstances()
    {
        savePanel.SetActive(false);

        if (!(fileNames.Count() > 0))
        {
            return;
        }

        StreamReader sr = new StreamReader(fileNames[0]);

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

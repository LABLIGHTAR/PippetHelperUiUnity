using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SubstanceLoader : MonoBehaviour
{
    public Button loadSubstancesButton;
    public Button yesButton;
    public Button noButton;
    public Button backButton;
    public GameObject savePanel;
    public Transform substanceList;
    public GameObject sampleListItemPrefab;
    public GameObject savedSubstancesList;
    public Transform savedSubstancesContent;

    private string folderPath;
    private string[] fileNames;

    // Start is called before the first frame update
    void Start()
    {
        loadSubstancesButton.onClick.AddListener(ShowConfirmation);
        yesButton.onClick.AddListener(LoadSavedSubstancesList);
        noButton.onClick.AddListener(delegate 
        { 
            savePanel.SetActive(false);
            SessionState.FormActive = false;
        });
        backButton.onClick.AddListener(delegate
        {
            savedSubstancesList.SetActive(false);
            backButton.gameObject.SetActive(false);
            SessionState.FormActive = false;
        });
    }

    void ShowConfirmation()
    {
        savePanel.SetActive(true);
        SessionState.FormActive = true;
    }

    void LoadSavedSubstancesList()
    {
        savePanel.SetActive(false);
        savedSubstancesList.SetActive(true);
        backButton.gameObject.SetActive(true);

        foreach(Transform listItem in savedSubstancesContent)
        {
            Destroy(listItem.gameObject);
        }

        //check if sample list folder exists
        folderPath = Path.Combine(@Application.temporaryCachePath, "..", "sample_lists");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        fileNames = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);

        if (fileNames.Count() <= 0)
        {
            return;
        }

        foreach(string fileName in fileNames)
        {
            var newSampleListItem = Instantiate(sampleListItemPrefab, savedSubstancesContent);
            var newSampleListItemVC = newSampleListItem.GetComponent<SampleListItemViewController>();

            newSampleListItemVC.InitItem(fileName);
            newSampleListItemVC.selectButton.onClick.AddListener(delegate { LoadSubstances(newSampleListItemVC.GetPath()); });
        }
    }

    private void LoadSubstances(string filePath)
    {
        savedSubstancesList.SetActive(false);
        SessionState.FormActive = false;

        StreamReader sr = new StreamReader(filePath);

        string currentLine;

        string[] lineCells;

        //clear session state substances
        foreach(Sample s in SessionState.AvailableSamples)
        {
            SessionState.RemoveSample(s.sampleName);
        }
        SessionState.Materials = SessionState.Materials.Where(m => m is Wellplate).ToList();

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

            if (vesselType == "tuberack5ml")
            {
                vesselType = "5mL Tube";
            }
            else if (vesselType == "reservoir")
            {
                vesselType = "Reservoir";
            }

            //add Sample to sessionState
            SessionState.AddNewSample(sampleName, sampleAbbreviation, sampleColorName, sampleColor, vesselType);
        }
    }
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class DilutionDisplayViewController : MonoBehaviour
{
    public TextMeshProUGUI instructionText;

    public TMP_InputField dilutionFactorInput;
    public TextMeshProUGUI dilutionFactorError;

    public TMP_Dropdown numDilutionsDropdown;
    public TextMeshProUGUI numDilutionsError;

    public TMP_Dropdown sampleDropdown;
    public TextMeshProUGUI sampleError;

    public TMP_InputField initialVolumeInput;
    public TextMeshProUGUI volumeError;

    public TMP_InputField SolventNameInput;
    public TextMeshProUGUI solventError;

    public Transform dilutionView;
    public GameObject dilutionItemPrefab;

    public Button confirmButton;

    private bool selectingWells;
    private List<Well> selectedWells;

    private int dilutionFactor;
    private int numDilutions;

    // Start is called before the first frame update
    void Start()
    {
        numDilutionsDropdown.onValueChanged.AddListener(delegate
        {
            CreateDilutionItems();
            if(InputValid())
            {
                selectingWells = true;
            }
        });

        SessionState.selectedWellStream.Subscribe(well => SelectWell(well));

        confirmButton.onClick.AddListener(delegate { InputValid(); });

        PopulateSampleDropdown();

        instructionText.text = "Enter dilution parameters";

        selectingWells = false;

        selectedWells = new List<Well>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateDilutionItems()
    {
        numDilutions = numDilutionsDropdown.value + 1;

        foreach (Transform dilutionItem in dilutionView.transform)
        {
            Destroy(dilutionItem.gameObject);
        }

        for (int i = 0; i <= numDilutions; i++)
        {
            var newDilutionItem = Instantiate(dilutionItemPrefab, dilutionView);
            if (i != 0 && i % 5 != 0)
            {
                newDilutionItem.GetComponent<DilutionItemViewController>().arrow.SetActive(true);
            }
        }
    }

    void PopulateSampleDropdown()
    {
        List<string> options = new List<string>();

        foreach(var sample in SessionState.AvailableSamples)
        {
            options.Add(sample.sampleName);
        }

        sampleDropdown.ClearOptions();
        sampleDropdown.AddOptions(options);
    }

    public bool DilutionFactorValid()
    {
        if (dilutionFactorInput.text.Length > 1)
        {
            if (int.TryParse(dilutionFactorInput.text, out dilutionFactor))
            {
                if (!(dilutionFactor > 0))
                {
                    dilutionFactorError.gameObject.SetActive(true);
                    dilutionFactorError.text = "Factor must be positive*";
                    return false;
                }
                else
                {
                    dilutionFactorError.gameObject.SetActive(false);
                }
            }
            else
            {
                dilutionFactorError.gameObject.SetActive(true);
                dilutionFactorError.text = "Factor must be an integer*";
                return false;
            }
        }
        else
        {
            dilutionFactorError.gameObject.SetActive(true);
            dilutionFactorError.text = "Factor cannot be empty*";
            return false;
        }
        return true;
    }

    public bool SampleDropdownValid()
    {
        if (!(sampleDropdown.options.Count > 0))
        {
            sampleError.gameObject.SetActive(true);
            sampleError.text = "No samples defined*";
            return false;
        }
        else
        {
            sampleError.gameObject.SetActive(false);
        }
        return true;
    }

    public bool InitalVolumeValid()
    {
        float intialVolume;

        if (float.TryParse(initialVolumeInput.text, out intialVolume))
        {
            if (!(intialVolume > 0))
            {
                volumeError.gameObject.SetActive(true);
                volumeError.text = "volume must be positive*";
                return false;
            }
            else
            {
                volumeError.gameObject.SetActive(false);
            }
        }
        else
        {
            volumeError.gameObject.SetActive(true);
            volumeError.text = "volume must be a floating point number*";
            return false;
        }
        return true;
    }

    public bool SolventNameValid()
    {
        string solventName;

        if (!(SolventNameInput.text.Length > 1))
        {
            solventError.gameObject.SetActive(true);
            solventError.text = "Solvent name cannot be empty*";
            return false;
        }
        else
        {
            solventError.gameObject.SetActive(false);
        }
        return true;
    }

    bool InputValid()
    {
        if(DilutionFactorValid() && SampleDropdownValid() && InitalVolumeValid() && SolventNameValid())
        {
            return true;
        }
        return false;
    }

    void SelectWell(Well well)
    {
        if (selectedWells.Count <= numDilutions && selectingWells)
        {
            int itemNum = selectedWells.Count;
            var dilutionItem = dilutionView.GetChild(itemNum).GetComponent<DilutionItemViewController>();
            selectedWells.Add(well);

            dilutionItem.wellText.text = well.id;

            string concentration;
            if (itemNum == 0)
            {
                concentration = "1:" + dilutionFactor.ToString();
                dilutionItem.dilutionText.text = concentration;
                Sample sample = SessionState.AvailableSamples.Where(s => s.sampleName == sampleDropdown.captionText.text).FirstOrDefault();
                SessionState.AddDilutionActionStart(sample, well, float.Parse(initialVolumeInput.text));
            }
            else
            {
                double newConcentration = Math.Pow((double)dilutionFactor, (double)selectedWells.Count);
                concentration = "1:" + newConcentration.ToString();
                dilutionItem.dilutionText.text = concentration;
                SessionState.AddDilutionAction(selectedWells[itemNum-1], well, float.Parse(initialVolumeInput.text));
            }
        }
        else
        {
            selectingWells = false;
        }
    }
}


using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    public Button clearSelectionsButton;
    public Button confirmButton;

    private int dilutionFactor;
    private int numDilutions;

    private List<Well> selectedWells;
    private int itemNum;

    private int inputSelected;
    private float tabDelay = 0.2f;
    private float tabDownTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        selectedWells = new List<Well>();

        CreateDilutionItems();

        SessionState.selectedWellsStream.Subscribe(wells => SelectWells(wells)).AddTo(this);

        numDilutionsDropdown.onValueChanged.AddListener(delegate { CreateDilutionItems();  });

        clearSelectionsButton.onClick.AddListener(CreateDilutionItems);

        confirmButton.onClick.AddListener(delegate 
        { 
            if(InputValid())
            {
                AddDilutionActions();
                ClearUI();
            }
        });

        PopulateSampleDropdown();

        instructionText.text = "Enter dilution parameters";
    }

    void OnEnable()
    {
        PopulateSampleDropdown();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.isPressed && ((Time.time - tabDelay) > tabDownTime))
        {
            SelectInputField();
        }
        if(InputValid())
        {
            if(selectedWells.Count != numDilutions + 1)
            {
                SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingTarget;
                instructionText.text = "Select a well";
            }    
        }
        else
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;
            instructionText.text = "Enter dilution parameters";
        }
    }

    void SelectInputField()
    {
        tabDownTime = Time.time;
        inputSelected++;
        inputSelected = inputSelected > 2 ? 0 : inputSelected;

        switch (inputSelected)
        {
            case 0:
                initialVolumeInput.Select();
                break;
            case 1:
                SolventNameInput.Select();
                break;
            case 2:
                dilutionFactorInput.Select();
                break;
        }
    }

    public void VolumeSelected () => inputSelected = 0;
    public void SolventSelected() => inputSelected = 1;
    public void DilutionSelected() => inputSelected = 2;

    void DestroyDilutionItems()
    {
        foreach (Transform dilutionItem in dilutionView.transform)
        {
            Destroy(dilutionItem.gameObject);
        }
    }

    void CreateDilutionItems()
    {
        DestroyDilutionItems();

        numDilutions = numDilutionsDropdown.value + 1;

        for (int i = 0; i <= numDilutions; i++)
        {
            var newDilutionItem = Instantiate(dilutionItemPrefab, dilutionView);
            if (i != 0 && i % 5 != 0)
            {
                newDilutionItem.GetComponent<DilutionItemViewController>().arrow.SetActive(true);
            }
        }

        selectedWells.Clear();
        SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
        SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingTarget;
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
        if (dilutionFactorInput.text.Length > 0)
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

    void SelectWells(List<Well> wells)
    {
        if(SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
        {
            int itemNum = selectedWells.Count;
            var dilutionItem = dilutionView.GetChild(itemNum).GetComponent<DilutionItemViewController>();
            selectedWells.Add(wells[0]);

            dilutionItem.wellText.text = wells[0].id;

            string concentration;
            if (itemNum == 0)
            {
                concentration = "1:" + dilutionFactor.ToString();
                dilutionItem.dilutionText.text = concentration;
            }
            else
            {
                double newConcentration = Math.Pow((double)dilutionFactor, (double)selectedWells.Count);
                concentration = "1:" + newConcentration.ToString();
                dilutionItem.dilutionText.text = concentration;
            }
        }
        if (selectedWells.Count == numDilutions + 1)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.awaitingSubmission;
        }
    }

    void AddDilutionActions()
    {
        if(SessionState.ActiveActionStatus == LabAction.ActionStatus.awaitingSubmission)
        {
            for (int i = 0; i < selectedWells.Count; i++)
            {
                if (i == 0)
                {
                    Sample sample = SessionState.AvailableSamples.Where(s => s.sampleName == sampleDropdown.captionText.text).FirstOrDefault();
                    SessionState.CurrentStep.AddDilutionActionStart(sample, selectedWells[i], float.Parse(dilutionFactorInput.text));
                }
                else
                {
                    SessionState.CurrentStep.AddDilutionAction(selectedWells[i - 1], selectedWells[i], float.Parse(dilutionFactorInput.text));
                }
            }
            SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
        }
    }

    void ClearUI()
    {
        selectedWells.Clear();

        instructionText.text = "Enter dilution parameters";
        dilutionFactorInput.text = "";
        initialVolumeInput.text = "";
        SolventNameInput.text = "";
        numDilutionsDropdown.value = 0;

        SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;
        itemNum = 0;

        CreateDilutionItems();
    }
}


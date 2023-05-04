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

    public TextMeshProUGUI volumeTitle;
    public TMP_InputField initialVolumeInput;
    public TextMeshProUGUI volumeError;

    public TMP_Dropdown diluentDropdown;
    public TextMeshProUGUI diluentError;

    public GameObject sourceDisplay;
    public TextMeshProUGUI sourceName;
    public TextMeshProUGUI sourceVolume;

    public Transform dilutionView;
    public GameObject dilutionItemPrefab;

    public Button clearSelectionsButton;
    public Button confirmButton;

    private float initialVolume;

    private int dilutionFactor;
    private int numDilutions;

    private List<(string Id, string subId)> selected;
    private int itemNum;

    private bool sourceIsWell = true;
    private List<Well> sourceWells;
    private Sample sourceSample;
    private Sample diluent;

    private int inputSelected;
    private float tabDelay = 0.2f;
    private float tabDownTime = 0f;

    
    void Awake()
    {
        selected = new List<(string, string)>();
        sourceWells = new List<Well>();

        SessionState.selectedWellsStream.Subscribe(wells => SelectWells(wells)).AddTo(this);

        sampleDropdown.onValueChanged.AddListener(delegate { UpdateSource(); });
        numDilutionsDropdown.onValueChanged.AddListener(delegate { CreateDilutionItems(); });
        clearSelectionsButton.onClick.AddListener(CreateDilutionItems);
        confirmButton.onClick.AddListener(delegate
        {
            if (InputValid())
            {
                AddDilutionActions();
                ClearUI();
            }
        });
    }

    void Start()
    {
        CreateDilutionItems();

        instructionText.text = "Enter dilution parameters";
    }

    void OnEnable()
    {
        ClearUI();
        PopulateDropdowns();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.isPressed && ((Time.time - tabDelay) > tabDownTime))
        {
            SelectInputField();
        } 
        if(InputValid())
        {
            if(itemNum != numDilutions + 1)
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
        if (sourceIsWell && sourceWells.Count == 0)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;
            instructionText.text = "Select a source well for dilution";
        }
    }

    void CreateDilutionItems()
    {
        DestroyDilutionItems();

        numDilutions = numDilutionsDropdown.value;

        for (int i = 0; i <= numDilutions; i++)
        {
            var newDilutionItem = Instantiate(dilutionItemPrefab, dilutionView);
        }
        itemNum = 0;

        selected.Clear();
        SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
        SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingTarget;
    }

    void DestroyDilutionItems()
    {
        for(int i = 1; i < dilutionView.transform.childCount; i++)
        {
            Destroy(dilutionView.transform.GetChild(i).gameObject);
        }
    }

    void SelectWells(List<Well> wells)
    {
        if(SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingSource)
        {
            SelectSourceWells(wells);
        }
        if (SessionState.ActiveActionStatus == LabAction.ActionStatus.selectingTarget)
        {
            SelectTargetWells(wells);
        }
        if (itemNum == numDilutions + 1)
        {
            SessionState.ActiveActionStatus = LabAction.ActionStatus.awaitingSubmission;
        }
    }

    void SelectSourceWells(List<Well> wells)
    {
        sourceWells = wells;

        if (sourceWells.Count == 1)
        {
            sourceName.text = sourceWells[0].id;
        }
        else
        {
            sourceName.text = sourceWells[0].id + "-" + sourceWells[sourceWells.Count - 1].id;
        }
    }

    void SelectTargetWells(List<Well> wells)
    {
        var dilutionItemVC = dilutionView.GetChild(itemNum + 1).GetComponent<DilutionItemViewController>();

        if (wells.Count > 1)
        {
            dilutionItemVC.wellText.text = wells[0].id + "-" + wells[wells.Count - 1].id;
        }
        else
        {
            dilutionItemVC.wellText.text = wells[0].id;
        }

        selected.Add((wells[0].plateId.ToString(), dilutionItemVC.wellText.text));

        string concentration;
        if (itemNum == 0)
        {
            dilutionItemVC.dilutionText.text = "1:" + dilutionFactor;
        }
        else
        {
            double newConcentration = Math.Pow((double)dilutionFactor, (double)selected.Count);
            concentration = "1:" + newConcentration.ToString();
            dilutionItemVC.dilutionText.text = concentration;
        }

        itemNum++;
    }

    void AddDilutionActions()
    {
        float finalVolume = dilutionFactor * initialVolume;
        
        if (SessionState.ActiveActionStatus == LabAction.ActionStatus.awaitingSubmission)
        {
            for(int i = 0; i < selected.Count; i++)
            {
                SessionState.ActiveTool.SetVolume(initialVolume * dilutionFactor);
                SessionState.ActiveSample = diluent;
                SessionState.CurrentStep.AddPipetteAction(selected[i].Id, selected[i].subId);
            }
            for (int i = 0; i < numDilutions + 1; i++)
            {
                if (i == 0)
                {
                    if(sourceIsWell)
                    {
                        SessionState.CurrentStep.AddTransferAction(sourceWells[0].plateId.ToString(), sourceName.text, selected[i].Id, selected[i].subId, float.Parse(initialVolumeInput.text));
                    }
                    else
                    {
                        SessionState.ActiveSample = sourceSample;
                        SessionState.ActiveTool.SetVolume(float.Parse(initialVolumeInput.text));
                        SessionState.CurrentStep.AddPipetteAction(selected[i].Id, selected[i].subId);
                    }
                }
                else
                {
                    SessionState.CurrentStep.AddTransferAction(selected[i - 1].Id, selected[i-1].subId, selected[i].Id, selected[i].subId, float.Parse(initialVolumeInput.text));
                }
            }
            SessionState.ActiveActionStatus = LabAction.ActionStatus.submitted;
        }
    }

    void UpdateSource()
    {
        if (sampleDropdown.value == sampleDropdown.options.Count - 1)
        {
            sourceIsWell = true;
            sourceDisplay.GetComponent<Image>().color = Color.white;
        }
        else
        {
            sourceIsWell = false;
            sourceSample = SessionState.AvailableSamples.Where(s => s.sampleName == sampleDropdown.captionText.text).FirstOrDefault();
            sourceName.text = sourceSample.sampleName;
            sourceDisplay.GetComponent<Image>().color = sourceSample.color;
        }
    }

    bool InputValid()
    {
        if (InitalVolumeValid() && DiluentValid() && DilutionFactorValid())
        {
            return true;
        }
        return false;
    }

    public bool InitalVolumeValid()
    {
        if (float.TryParse(initialVolumeInput.text, out initialVolume))
        {
            if (!(initialVolume > 0))
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
        sourceVolume.text = initialVolume.ToString() + "μL";
        return true;
    }

    public bool DiluentValid()
    {
        if (diluentDropdown.options.Count < 1)
        {
            diluentError.gameObject.SetActive(true);
            diluentError.text = "No samples defined*";
            return false;
        }
        else
        {
            diluent = SessionState.AvailableSamples.Where(s => s.sampleName == diluentDropdown.captionText.text).FirstOrDefault();
            diluentError.gameObject.SetActive(false);
        }
        return true;
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

    void PopulateDropdowns()
    {
        List<string> options = new List<string>();

        foreach (var sample in SessionState.AvailableSamples)
        {
            options.Add(sample.sampleName);
        }

        diluentDropdown.ClearOptions();
        diluentDropdown.AddOptions(options);

        options.Add("Select Well");
        sampleDropdown.ClearOptions();
        sampleDropdown.AddOptions(options);
    }

    void ClearUI()
    {
        selected.Clear();

        ClearSourceWells();

        instructionText.text = "Enter dilution parameters";
        dilutionFactorInput.text = "";
        initialVolumeInput.text = "";
        diluentDropdown.value = -1;
        numDilutionsDropdown.value = -1;

        itemNum = 0;

        SessionState.ActiveActionStatus = LabAction.ActionStatus.selectingSource;

        CreateDilutionItems();
    }

    void ClearSourceWells()
    {
        sourceWells.Clear();
        if(sourceIsWell)
        {
            sourceName.text = "";
            sourceVolume.text = "";
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
                diluentDropdown.Select();
                break;
            case 2:
                dilutionFactorInput.Select();
                break;
        }
    }

    public void VolumeSelected() => inputSelected = 0;
    public void SolventSelected() => inputSelected = 1;
    public void DilutionSelected() => inputSelected = 2;
}


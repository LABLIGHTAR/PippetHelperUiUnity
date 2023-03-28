using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class NewSampleFormController : MonoBehaviour
{
    public Transform nameError;
    public Transform abreviationError;
    public Transform colorError;

    public TMP_Dropdown dropdown;
    public Button submitButton;
    public Button closeButton;

    public TMP_InputField nameText;
    public TextMeshProUGUI nameErrorText;
    public TMP_InputField abreviationText;
    public TextMeshProUGUI abreviationErrorText;
    public TextMeshProUGUI colorText;
    public TextMeshProUGUI vesselText;

    private Color newColor;
    private int inputSelected;
    private float tabDelay = 0.2f;
    private float tabDownTime = 0f;

    public List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

    // Start is called before the first frame update
    void Start()
    {
        //Add button events
        submitButton.onClick.AddListener(AddNewSample);

        closeButton.onClick.AddListener(delegate
        {
            this.gameObject.SetActive(false);
            SessionState.FormActive = false;
        });
    }

    void OnEnable()
    {
        //update color dropdown options
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> availableColors = dropdownOptions.Where(option => !SessionState.UsedColors.Contains(option.text)).ToList();
        dropdown.AddOptions(availableColors);
    }

    void OnDisable()
    {
        //reset form values
        nameText.text = "";
        abreviationText.text = "";
        dropdown.value = 0;
    }

    void Update()
    {
        if(Keyboard.current.tabKey.isPressed && ((Time.time - tabDelay) > tabDownTime))
        {
            tabDownTime = Time.time;
            inputSelected++;
            inputSelected = inputSelected > 1 ? 0 : inputSelected;

            SelectInputField();
        }
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            AddNewSample();
        }
    }

    void SelectInputField()
    {
        switch(inputSelected)
        {
            case 0: nameText.Select();
                break;
            case 1: abreviationText.Select(); 
                break;
        }
    }

    public void NameSelected() => inputSelected = 0;
    public void AbrevSelected() => inputSelected = 1;

    /// <summary>
    /// adds new sample to session state if input is valid
    /// </summary>
    void AddNewSample()
    {
        if(InputValidNew())
        {
            //generate color value
            var color = Colors.ColorValue((Colors.ColorNames)System.Enum.Parse(typeof(Colors.ColorNames), colorText.text, true));

            //add new Sample to session state
            SessionState.AddNewSample(nameText.text, abreviationText.text, colorText.text, color, vesselText.text);

            //update color dropdown options
            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> availableColors = dropdownOptions.Where(option => !SessionState.UsedColors.Contains(option.text)).ToList();
            dropdown.AddOptions(availableColors);

            //disable form
            this.gameObject.SetActive(false);
            SessionState.FormActive = false;
        }
    }

    /// <summary>
    /// edits existing sample in session state if input is valid
    /// </summary>
    /// <param name="oldName"> name of sample before edit </param>
    /// <param name="abbreviation"></param>
    /// <param name="colorText"></param>
    public void EditSample(string oldName, string oldAbbreviation, string oldColorString)
    {
        //activate edit form
        this.gameObject.SetActive(true);
        SessionState.FormActive = true;

        //add the edited samples color back to the dropdown and set the picker to it
        dropdown.AddOptions(new List<string>() { oldColorString });
        dropdown.value = dropdown.options.Count() - 1;

        //fill out the form with the old values
        nameText.text = oldName;
        abreviationText.text = oldAbbreviation;
        dropdown.value = dropdown.options.FindIndex((i) => { return i.text.Equals(oldColorString); });
        
        //set the submit button text and change event to edit
        submitButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Save Changes";
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(delegate
        {
            if(InputValidEdit(oldName, oldAbbreviation))
            {
                //generate color value
                var color = Colors.ColorValue((Colors.ColorNames)System.Enum.Parse(typeof(Colors.ColorNames), colorText.text, true));

                //edit sample in session state
                SessionState.EditSample(oldName, nameText.text, abreviationText.text, colorText.text, color);

                //reset submit button event
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(AddNewSample);

                //reset submit button text
                submitButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Substance";

                //disable form
                this.gameObject.SetActive(false);
                SessionState.FormActive = false;
            }
        });
    }

    private bool InputValidEdit(string oldName, string oldAbbreviation)
    {
        //check input
        if (nameText.text != oldName && SessionState.AvailableSamples.Exists(x => x.sampleName == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Substance with this name already exists*";
            return false;
        }
        if (!(nameText.text.Length > 0))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Name cannot be empty*";
            return false;
        }
        else
        {
            nameError.gameObject.SetActive(false);
        }
        if (abreviationText.text != oldAbbreviation && SessionState.AvailableSamples.Exists(x => x.abreviation == abreviationText.text))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Substance with this abreviation already exists*";
            return false;
        }
        if (!(abreviationText.text.Length > 0))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be empty*";
            return false;
        }
        if (abreviationText.text.Length > 4)
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be more than 4 characters*";
            return false;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }
        if(!(colorText.text.Length > 0))
        {
            colorError.gameObject.SetActive(true);
            return false;
        }
        else
        {
            colorError.gameObject.SetActive(false);
        }
        return true;
    }

    private bool InputValidNew()
    {
        //check input
        if (SessionState.AvailableSamples.Exists(x => x.sampleName == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Sample with this name already exists*";
            return false;
        }
        if (!(nameText.text.Length > 0))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Name cannot be empty*";
            return false;
        }
        else
        {
            nameError.gameObject.SetActive(false);
        }
        if (SessionState.AvailableSamples.Exists(x => x.abreviation == abreviationText.text))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Sample with this abreviation already exists*";
            return false;
        }
        if (!(abreviationText.text.Length > 0))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be empty*";
            return false;
        }
        if (abreviationText.text.Length > 4)
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be more than 4 characters*";
            return false;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }
        if (!(colorText.text.Length > 0))
        {
            colorError.gameObject.SetActive(true);
            return false;
        }
        else
        {
            colorError.gameObject.SetActive(false);
        }
        return true;
    }
}

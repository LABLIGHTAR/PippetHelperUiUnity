using System.Linq;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewSampleFormController : MonoBehaviour
{
    public Transform nameError;
    public Transform abreviationError;
    public Transform volumeError;

    public TMP_Dropdown dropdown;
    public Button submitButton;
    public Button closeButton;

    public TMP_InputField nameText;
    public TextMeshProUGUI nameErrorText;
    public TMP_InputField abreviationText;
    public TextMeshProUGUI abreviationErrorText;
    public TextMeshProUGUI colorText;
    public TMP_InputField volumeText;
    public TextMeshProUGUI volumeErrorText;
    public TextMeshProUGUI volumeLabel;
    public TextMeshProUGUI volumePlaceholder;

    private Color newColor;

    //colors
    List<string> dropdownOptions = new List<string>
    {
        "Lime",
        "Green",
        "Olive",
        "Brown",
        "Aqua",
        "Blue",
        "Navy",
        "Slate",
        "Purple",
        "Plum",
        "Pink",
        "Salmon",
        "Red",
        "Orange",
        "Yellow",
        "Khaki",
    };

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
        List<string> availableColors = dropdownOptions.Except(SessionState.UsedColors).ToList();
        dropdown.AddOptions(availableColors);
    }

    void OnDisable()
    {
        //reset form values
        nameText.text = "";
        abreviationText.text = "";
        volumeText.text = "";
        dropdown.value = 0;
    }

    /// <summary>
    /// adds new sample to session state if input is valid
    /// </summary>
    void AddNewSample()
    {
        if(InputValidNew())
        {
            //generate color an volume values
            var color = SessionState.Colors.ColorValue((SessionState.Colors.ColorNames)System.Enum.Parse(typeof(SessionState.Colors.ColorNames), colorText.text, true));
            var volume = float.Parse(volumeText.text.Substring(0, volumeText.text.Length), CultureInfo.InvariantCulture.NumberFormat);
            
            //add new Sample to session state
            SessionState.AddNewSample(nameText.text, abreviationText.text, colorText.text, color, volume);

            //update color dropdown options
            dropdown.ClearOptions();
            List<string> availableColors = dropdownOptions.Except(SessionState.UsedColors).ToList();
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
    /// <param name="volume"></param>
    public void EditSample(string oldName, string oldAbbreviation, string oldColorString, string oldVolume)
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
        volumeText.text = oldVolume;
        dropdown.value = dropdown.options.FindIndex((i) => { return i.text.Equals(oldColorString); });
        
        //set the submit button text and change event to edit
        submitButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Save Changes";
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(delegate
        {
            if(InputValidEdit(oldName, oldAbbreviation))
            {
                //generate color and volume values
                var color = SessionState.Colors.ColorValue((SessionState.Colors.ColorNames)System.Enum.Parse(typeof(SessionState.Colors.ColorNames), colorText.text, true));
                var volume = float.Parse(volumeText.text.Substring(0, volumeText.text.Length), CultureInfo.InvariantCulture.NumberFormat);

                //edit sample in session state
                SessionState.EditSample(oldName, nameText.text, abreviationText.text, colorText.text, color, volume);

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

    public void BoxChecked(bool isSolid)
    {
        if(isSolid)
        {
            volumeLabel.text = "Substance Weight (μg)";
            volumePlaceholder.text = "Enter Weight (μg)";
        }
        else
        {
            volumeLabel.text = "Substance Volume (μL)";
            volumePlaceholder.text = "Enter Volume (μL)";
        }
    }

    private bool InputValidEdit(string oldName, string oldAbbreviation)
    {
        //check input
        if (nameText.text != oldName && SessionState.AvailableSamples.Exists(x => x.name == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Substance with this name already exists*";
            return false;
        }
        if (!(nameText.text.Length > 1))
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
        if (!(abreviationText.text.Length > 1))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be empty*";
            return false;
        }
        if (abreviationText.text.Length > 5)
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be more than 4 characters*";
            return false;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }
        if (!(volumeText.text.Length > 0))
        {
            volumeError.gameObject.SetActive(true);
            volumeErrorText.text = "Volume cannot be empty*";
            return false;
        }
        if (!float.TryParse(volumeText.text.Substring(0, volumeText.text.Length), out _))
        {
            volumeError.gameObject.SetActive(true);
            volumeErrorText.text = "Please do not include units*";
            return false;
        }
        else
        {
            volumeError.gameObject.SetActive(false);
        }
        return true;
    }

    private bool InputValidNew()
    {
        //check input
        if (SessionState.AvailableSamples.Exists(x => x.name == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Sample with this name already exists*";
            return false;
        }
        if (!(nameText.text.Length > 1))
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
        if (!(abreviationText.text.Length > 1))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be empty*";
            return false;
        }
        if (abreviationText.text.Length > 5)
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be more than 4 characters*";
            return false;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }
        if (!(volumeText.text.Length > 0))
        {
            volumeError.gameObject.SetActive(true);
            volumeErrorText.text = "Volume cannot be empty*";
            return false;
        }
        if (!float.TryParse(volumeText.text.Substring(0, volumeText.text.Length), out _))
        {
            volumeError.gameObject.SetActive(true);
            volumeErrorText.text = "Please do not include units*";
            return false;
        }
        else
        {
            volumeError.gameObject.SetActive(false);
        }
        return true;
    }
}

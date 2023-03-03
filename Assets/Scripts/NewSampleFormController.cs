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

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameErrorText;
    public TextMeshProUGUI abreviationText;
    public TextMeshProUGUI abreviationErrorText;
    public TextMeshProUGUI colorText;
    public TextMeshProUGUI volumeText;
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

    void AddNewSample()
    {
        //check input
        if (SessionState.AvailableSamples.Exists(x => x.name == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Sample with this name already exists*";
            return;
        }
        if (!(nameText.text.Length > 1))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Name cannot be empty*";
            return;
        }
        else
        {
            nameError.gameObject.SetActive(false);
        }
        if (SessionState.AvailableSamples.Exists(x => x.abreviation == abreviationText.text))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Sample with this abreviation already exists*";
            return;
        }
        if (!(abreviationText.text.Length > 1))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be empty*";
            return;
        }
        if (abreviationText.text.Length > 5)
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Abreviation cannot be more than 4 characters*";
            return;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }
        if (!(volumeText.text.Length > 1))
        {
            volumeError.gameObject.SetActive(true);
            volumeErrorText.text = "Volume cannot be empty*";
            return;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }

        var color = SessionState.Colors.ColorValue((SessionState.Colors.ColorNames)System.Enum.Parse(typeof(SessionState.Colors.ColorNames), colorText.text, true));
        var volume = float.Parse(volumeText.text.Substring(0, volumeText.text.Length - 1), CultureInfo.InvariantCulture.NumberFormat);
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

    public void BoxChecked(bool isSolid)
    {
        if(isSolid)
        {
            volumeLabel.text = "Solid Weight (μg)";
            volumePlaceholder.text = "Enter Weight (μg)";
        }
        else
        {
            volumeLabel.text = "Sample Volume (μL)";
            volumePlaceholder.text = "Enter Volume (μL)";
        }
    }
}

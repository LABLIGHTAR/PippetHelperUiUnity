using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewLiquidFormController : MonoBehaviour
{
    public Transform name;
    public Transform nameError;
    public Transform abreviation;
    public Transform abreviationError;
    public Transform colorDropdown;
    public TMP_Dropdown dropdown;
    public Button submitButton;
    public Button closeButton;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI nameErrorText;
    private TextMeshProUGUI abreviationText;
    private TextMeshProUGUI abreviationErrorText;
    private TextMeshProUGUI colorText;
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
        //Get text components
        nameText = name.GetComponent<TextMeshProUGUI>();
        nameErrorText = nameError.GetComponent<TextMeshProUGUI>();
        abreviationText = abreviation.GetComponent<TextMeshProUGUI>();
        abreviationErrorText = abreviationError.GetComponent<TextMeshProUGUI>();
        colorText = colorDropdown.Find("Label").GetComponent<TextMeshProUGUI>();

        //Set Dropdown Colors
        dropdown.AddOptions(dropdownOptions);

        //Add button events
        submitButton.onClick.AddListener(delegate {
            if(nameText.text.Length > 1 & abreviationText.text.Length > 1 & colorText.text.Length > 1)
            {
                AddNewLiquid();
            }
        });

        closeButton.onClick.AddListener(delegate
        {
            this.gameObject.SetActive(false);
            SessionState.FormActive = false;
        });
    }

    void AddNewLiquid()
    {
        //check input
        if(SessionState.AvailableLiquids.Exists(x => x.name == nameText.text))
        {
            nameError.gameObject.SetActive(true);
            nameErrorText.text = "Liquid with this name already exists";
            return;
        }
        else
        {
            nameError.gameObject.SetActive(false);
        }
        if(SessionState.AvailableLiquids.Exists(x => x.abreviation == abreviationText.text))
        {
            abreviationError.gameObject.SetActive(true);
            abreviationErrorText.text = "Liquid with this abreviation already exists";
            return;
        }
        else
        {
            abreviationError.gameObject.SetActive(false);
        }

        //add new liquid to session state
        SessionState.AddNewLiquid(nameText.text, abreviationText.text, colorText.text, SessionState.Colors.ColorValue((SessionState.Colors.ColorNames)System.Enum.Parse(typeof(SessionState.Colors.ColorNames), colorText.text, true)));
        
        //log the used color in the session state
        SessionState.UsedColors.Add(colorText.text);

        //update color dropdown options
        dropdown.ClearOptions();
        List<string> availableColors = dropdownOptions.Except(SessionState.UsedColors).ToList();
        dropdown.AddOptions(availableColors);

        //disable form
        this.gameObject.SetActive(false);
        SessionState.FormActive = false;
    }
}

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
    public Button submitButton;
    public Button closeButton;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI nameErrorText;
    private TextMeshProUGUI abreviationText;
    private TextMeshProUGUI abreviationErrorText;
    private TextMeshProUGUI colorText;
    private Dropdown dropdown;
    private Color newColor;

    //colors
    List<string> dropdownOptions = new List<string>
    {
        "Lime",
        "Green",
        "Aqua",
        "Blue",
        "Navy",
        "Purple",
        "Pink",
        "Red",
        "Orange",
        "Yellow"
    };

    // Start is called before the first frame update
    void Start()
    {
        //Get text components
        nameText = name.GetComponent<TextMeshProUGUI>();
        nameErrorText = nameError.GetComponent<TextMeshProUGUI>();
        abreviationText = abreviation.GetComponent<TextMeshProUGUI>();
        abreviationErrorText = abreviationError.GetComponent<TextMeshProUGUI>();
        colorText = colorDropdown.GetComponent<TextMeshProUGUI>();

        //Get dropdown
        dropdown = colorDropdown.GetComponent<Dropdown>();

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

        SessionState.AddNewLiquid(nameText.text, abreviationText.text, SessionState.Colors.ColorValue((SessionState.Colors.ColorNames)Enum.Parse(typeof(SessionState.Colors.ColorNames), colorText.text, true)));
        nameText.text = "";
        abreviationText.text = "";

        SessionState.UsedColors.Add(colorText.text);

        dropdown.ClearOptions();

        List<string> availableColors = (List<string>)dropdownOptions.Except(SessionState.UsedColors);
        dropdown.AddOptions(availableColors);

        this.gameObject.SetActive(false);
        SessionState.FormActive = false;
    }
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownOpenScript : MonoBehaviour
{
    public TMP_Dropdown colorDropdown;
    public List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
    public Transform dropdownContent;
    public Sprite dropdownSprite;

    void Start()
    {
        if (this.gameObject.name == "Dropdown List")
        {
            int itemNum = colorDropdown.value;

            foreach (string colorName in Enum.GetNames(typeof(Colors.ColorNames)))
            {
                dropdownOptions.Add(new TMP_Dropdown.OptionData(colorName, dropdownSprite));
            }

            if(colorDropdown.options != dropdownOptions)
            {
                colorDropdown.ClearOptions();
                List<TMP_Dropdown.OptionData> availableColors = dropdownOptions.Where(option => !SessionState.UsedColors.Contains(option.text)).ToList();
                colorDropdown.AddOptions(availableColors);

                dropdownContent = colorDropdown.transform.Find("Dropdown List").GetChild(0).GetChild(0);

                foreach (Transform item in dropdownContent)
                {
                    Colors.ColorNames color;
                    Enum.TryParse(item.Find("Item Label").GetComponent<TextMeshProUGUI>().text, out color);
                    item.Find("Item Image").GetComponent<Image>().color = Colors.ColorValue(color);
                }

                colorDropdown.value = itemNum;
            }
        }
    }
}

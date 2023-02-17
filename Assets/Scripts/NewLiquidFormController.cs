using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewLiquidFormController : MonoBehaviour
{
    public Transform name;
    public Transform abreviation;
    public Transform colorDropdown;
    public Button submitButton;
    public Button closeButton;
    public Transform liquidDisplay;
    public GameObject LiquidSwatchPrefab;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI abreviationText;
    private TextMeshProUGUI colorText;
    private Color newColor;
    // Start is called before the first frame update
    void Start()
    {
        //Get text components
        nameText = name.GetComponent<TextMeshProUGUI>();
        abreviationText = abreviation.GetComponent<TextMeshProUGUI>();
        colorText = colorDropdown.GetComponent<TextMeshProUGUI>();

        //Add button events
        submitButton.onClick.AddListener(delegate {
            if(nameText.text.Length > 1 & abreviationText.text.Length > 1 & colorText.text.Length > 1)
            {
                AddNewLiquid();
                this.gameObject.SetActive(false);
            }
        });

        closeButton.onClick.AddListener(delegate
        {
            this.gameObject.SetActive(false);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateVisualState()
    {
        //destroy old swatches
        foreach(Transform child in liquidDisplay)
        {
            Destroy(child.gameObject);
        }
        //create new swatches
        foreach(SessionState.Liquid liquid in SessionState.availableLiquids)
        {
            GameObject newLiquidSwatch = Instantiate(LiquidSwatchPrefab) as GameObject;
            newLiquidSwatch.transform.parent = liquidDisplay;
            newLiquidSwatch.GetComponent<LiquidSwatchViewController>().SetAbreviation(liquid.abreviation);
            newLiquidSwatch.GetComponent<LiquidSwatchViewController>().SetColor(liquid.color);
        }
    }

    void AddNewLiquid()
    {
        //Add new liquid to available liquids in session state
        switch (colorText.text)
        {
            case ("Red"):
                newColor = Color.red;
                break;
            case ("Green"):
                newColor = Color.green;
                break;
            case ("Blue"):
                newColor = Color.blue;
                break;
        }
        SessionState.AddNewLiquid(nameText.text, abreviationText.text, newColor);
        nameText.text = "";
        abreviationText.text = "";
        UpdateVisualState();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class NewProcedureManager : MonoBehaviour
{
    public TextMeshProUGUI inputText;
    public TMP_InputField input;

    public GameObject inputError;

    public Button createProcedureButton;

    // Start is called before the first frame update
    void Start()
    {
        createProcedureButton.onClick.AddListener(CreateProcedure);
        SessionState.FormActive = true;

        //set background transparancy;
        var background = this.GetComponent<Image>();
        background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);

        input.Select();
    }

    void Update()
    {
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            CreateProcedure();
        }
    }
    
    void CreateProcedure()
    {
        if (inputText.text.Length > 1)
        {
            inputError.SetActive(false);
            SessionState.ProcedureName = inputText.text;
            this.gameObject.SetActive(false);
            SessionState.FormActive = false;
        }
        else
        {
            inputError.SetActive(true);
        }
    }
}

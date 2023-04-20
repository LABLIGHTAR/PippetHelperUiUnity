using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class NewProcedureManager : MonoBehaviour
{
    public TMP_InputField input;
    public TextMeshProUGUI inputText;
    public TextMeshProUGUI inputError;

    public Button createProcedureButton;

    private string procedureName;

    // Start is called before the first frame update
    void Start()
    {
        inputError.text = "";
        createProcedureButton.onClick.AddListener(CreateProcedure);
        SessionState.FormActive = true;

        //set background transparancy;
        var background = this.GetComponent<Image>();
        background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);

        input.Select();
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            CreateProcedure();
        }
    }

    void CreateProcedure()
    {
        procedureName = inputText.text.Substring(0, inputText.text.Length - 1);
        Debug.Log(procedureName);
        Debug.Log(procedureName.Length);

        if (ProcedureNameValid())
        {
            SessionState.ProcedureName = procedureName;
            this.gameObject.SetActive(false);
            SessionState.FormActive = false;
        }
    }

    bool ProcedureNameValid()
    {
        if(string.IsNullOrEmpty(procedureName))
        {
            inputError.text = "Protcol name empty*";
            return false;
        }
        if (procedureName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
        {
            inputError.text = "Invalid character in protocol name*";
            return false;
        }
        if (File.Exists(Path.Combine(@Application.temporaryCachePath, "..", "inflight_protocols", procedureName + ".csv")))
        {
            inputError.text = "A protocol with this name already exists*";
            return false;
        }

        inputError.text = "";
        return true;
    }
}

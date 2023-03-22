using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ProgressionViewController : MonoBehaviour
{
    public Button materialsButton;
    public Button methodButton;
    public Button exportButton;

    public Color activeColor;
    public Color inactiveColor;

    // Start is called before the first frame update
    void Start()
    {
/*        materialsButton.onClick.AddListener(SetMaterials);
        methodButton.onClick.AddListener(SetMethod);
        exportButton.onClick.AddListener(SetExport);*/

        MaterialViewController.materialsSelectedStream.Subscribe(_ =>
        {
            SetMethod();
        });

        ProcedureLoader.procedureStream.Subscribe(_ =>
        {
            SetMethod();
        });
    }

   void SetMaterials()
    {
        materialsButton.GetComponent<Image>().color = activeColor;
        methodButton.GetComponent<Image>().color = inactiveColor;
        exportButton.GetComponent<Image>().color = inactiveColor;
    }

    void SetMethod()
    {
        methodButton.GetComponent<Image>().color = activeColor;
        materialsButton.GetComponent<Image>().color = inactiveColor;
        exportButton.GetComponent<Image>().color = inactiveColor;
    }

    void SetExport()
    {
        exportButton.GetComponent<Image>().color = activeColor;
        methodButton.GetComponent<Image>().color = inactiveColor;
        materialsButton.GetComponent<Image>().color = inactiveColor;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class DilutionItemViewController : MonoBehaviour
{
    public TextMeshProUGUI wellText;
    public TextMeshProUGUI dilutionText;
    public GameObject arrow;

    public List<Well> selectedWells;
    public bool wellSelected;

    // Start is called before the first frame update
    void Start()
    {
        wellText.text = "";
        dilutionText.text = "";
    }

    void UpdateVisualState(List<Well> wells)
    {
        if (wells.Count == 1)
        {
            wellText.text = wells[0].id;
        }
        else
        {
            wellText.text = wells[0].id + "-" + wells[wells.Count - 1].id;
        }
    }
}

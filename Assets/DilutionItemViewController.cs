using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DilutionItemViewController : MonoBehaviour
{
    public TextMeshProUGUI wellText;
    public TextMeshProUGUI dilutionText;
    public GameObject arrow;

    public Well selectedWell;
    public bool wellSelected;

    // Start is called before the first frame update
    void Start()
    {
        wellText.text = "";
        dilutionText.text = "";
    }
}

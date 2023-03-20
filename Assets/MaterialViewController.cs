using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialViewController : MonoBehaviour
{
    public Button addMaterialButton;
    
    public Button plate96Button;
    public Sprite sprite96;

    public Button plate384Button;
    public Sprite sprite384;

    public GameObject materialViewPrefab;
    public GameObject materialList;

    private int numMaterials;

    // Start is called before the first frame update
    void Start()
    {
        addMaterialButton.onClick.AddListener(AddNewMaterial);
        plate96Button.onClick.AddListener(Add96WellPlate);
        plate384Button.onClick.AddListener(Add384WellPlate);
    }

    void Update()
    {
        if(!addMaterialButton.transform.parent.gameObject.activeSelf)
        {
            if (this.transform.childCount < 11)
            {
                addMaterialButton.transform.parent.gameObject.SetActive(true);
            }
        }
        else if(this.transform.childCount == 11)
        {
            addMaterialButton.transform.parent.gameObject.SetActive(false);
        }
    }

    void AddNewMaterial()
    {
        if(this.transform.childCount < 11)
        {
            addMaterialButton.transform.parent.gameObject.SetActive(true);
            materialList.SetActive(true);
        }
    }

    void Add96WellPlate()
    {
        materialList.SetActive(false);
        GameObject newMaterial = Instantiate(materialViewPrefab, this.transform);

        newMaterial.GetComponent<MaterialDisplayViewController>().InitDisplay("96-Well Plate", sprite96);

        numMaterials++;
    }

    void Add384WellPlate()
    {
        materialList.SetActive(false);
        GameObject newMaterial = Instantiate(materialViewPrefab, this.transform);

        newMaterial.GetComponent<MaterialDisplayViewController>().InitDisplay("384-Well Plate", sprite384);

        numMaterials++;
    }
}

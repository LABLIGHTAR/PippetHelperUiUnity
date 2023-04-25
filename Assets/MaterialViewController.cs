using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class MaterialViewController : MonoBehaviour
{
    public Button addMaterialButton;
    
    public Button plate96Button;
    public Sprite sprite96;

    public Button plate384Button;
    public Sprite sprite384;

    public Button continueButton;

    public GameObject materialViewPrefab;
    public GameObject materialList;

    private int numMaterials;

    private int materialID;

    //data stream to tell plate displays material selection is done
    public static Subject<int> materialsSelectedStream = new Subject<int>();

    // Start is called before the first frame update
    void Start()
    {
        addMaterialButton.onClick.AddListener(AddNewMaterial);
        continueButton.onClick.AddListener(ContinueToMethods);
        plate96Button.onClick.AddListener(Add96WellPlate);
        plate384Button.onClick.AddListener(Add384WellPlate);

        continueButton.interactable = false;
    }

    void Update()
    {
        if(!addMaterialButton.transform.parent.gameObject.activeSelf)
        {
            if (this.transform.childCount < 5)
            {
                addMaterialButton.transform.parent.gameObject.SetActive(true);
            }
        }
        else if(this.transform.childCount == 5)
        {
            addMaterialButton.transform.parent.gameObject.SetActive(false);
        }
        if(!continueButton.interactable && this.transform.childCount > 1)
        {
            continueButton.interactable = true;
        }
        else if(continueButton.interactable && this.transform.childCount < 2)
        {
            continueButton.interactable = false;
        }
    }

    void ContinueToMethods()
    {
        materialsSelectedStream.OnNext(materialID);
        this.transform.parent.gameObject.SetActive(false);
    }

    void AddNewMaterial()
    {
        if(this.transform.childCount < 5)
        {
            addMaterialButton.transform.parent.gameObject.SetActive(true);
            materialList.SetActive(true);
        }
    }

    void Add96WellPlate()
    {
        materialList.SetActive(false);

        LabMaterial newWellplate = new Wellplate(materialID, "wellplate96", 96);
        SessionState.Materials.Add(newWellplate);
        materialID++;
        numMaterials++;

        var newMaterialVC = Instantiate(materialViewPrefab, this.transform).GetComponent<MaterialDisplayViewController>();
        newMaterialVC.InitDisplay("96-Well Plate", sprite96);
        newMaterialVC.trashButton.onClick.AddListener(delegate
        {
            materialID = newWellplate.id;
            SessionState.Materials.Remove(newWellplate);
            numMaterials--;
        });
    }

    void Add384WellPlate()
    {
        materialList.SetActive(false);

        LabMaterial newWellplate = new Wellplate(materialID, "wellplate384", 384);
        SessionState.Materials.Add(newWellplate);
        materialID++;
        numMaterials++;

        var newMaterialVC = Instantiate(materialViewPrefab, this.transform).GetComponent<MaterialDisplayViewController>();
        newMaterialVC.InitDisplay("384-Well Plate", sprite384);
        newMaterialVC.trashButton.onClick.AddListener(delegate
        {
            materialID = newWellplate.id;
            SessionState.Materials.Remove(newWellplate);
            numMaterials--;
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialDisplayViewController : MonoBehaviour
{
    public TextMeshProUGUI materialType;
    public TextMeshProUGUI materialName;
    public TMP_InputField nameInput;
    public TextMeshProUGUI errorText;
    public Image materialImage;
    public Button editButton;
    public Button rotateButton;
    public Button trashButton;

    void Start()
    {
        rotateButton.onClick.AddListener(RotateMaterial);
        trashButton.onClick.AddListener(DeleteMaterial);
    }
    
    public void InitDisplay(string inputType, Sprite inputSprite, int id)
    {
        materialType.text = inputType;
        materialName.text = "plate " + (id + 1);
        materialImage.sprite = inputSprite;
    }

    public void EditMaterial()
    {
        nameInput.gameObject.SetActive(true);
        nameInput.Select();
    }

    void RotateMaterial()
    {
        materialImage.GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 90));
    }

    void DeleteMaterial()
    {
        Destroy(this.gameObject);
    }
}

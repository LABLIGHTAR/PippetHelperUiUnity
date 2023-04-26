using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialDisplayViewController : MonoBehaviour
{
    public TextMeshProUGUI materialType;
    public Image materialImage;
    public Button editButton;
    public Button rotateButton;
    public Button trashButton;

    void Start()
    {
        rotateButton.onClick.AddListener(RotateMaterial);
        trashButton.onClick.AddListener(DeleteMaterial);
    }
    
    public void InitDisplay(string inputType, Sprite inputSprite)
    {
        materialType.text = inputType;
        materialImage.sprite = inputSprite;
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
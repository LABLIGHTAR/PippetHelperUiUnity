using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UniRx;

public class UiInteraction : MonoBehaviour
{
    public GameObject canvas;

    GraphicRaycaster raycaster;
    PointerEventData clickData;
    List<RaycastResult> clickResults;

    public static Subject<GameObject> uiClickStream = new Subject<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        clickData = new PointerEventData(EventSystem.current);
        clickResults = new List<RaycastResult>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame && !SessionState.FormActive)
        {
            var selectedObject = GetUiElementsClicked();
            if (selectedObject != null)
            {
                uiClickStream.OnNext(selectedObject);
            }
        }
    }

    GameObject GetUiElementsClicked()
    {
        clickData.position = Mouse.current.position.ReadValue();
        clickResults.Clear();

        raycaster.Raycast(clickData, clickResults);
        
        if(clickResults.Count > 0)
        {
            GameObject uiElement = clickResults[0].gameObject;
            return uiElement;
        }
        return null;
    }
}

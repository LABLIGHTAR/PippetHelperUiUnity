using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ActionItemViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler/*, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IInitializePotentialDragHandler */
{
    LabAction associatedAction;
    public Canvas canvas;
    public TextMeshProUGUI actionText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startingPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
/*
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = startingPosition;
    }*/

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        SessionState.SetFocusedAction(associatedAction);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SessionState.SetFocusedAction(null);
        Debug.Log("Exit");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down");
    }

    public void InitActionItem(LabAction action)
    {
        associatedAction = action;
        actionText.text = action.GetActionString();
    }
}

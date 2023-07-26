using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;

public class ActionItemViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public LabAction associatedAction;
    public Canvas canvas;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI actionNum;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startingPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        SessionState.CurrentStep.actionRemovedStream.Subscribe(action => UpdateActionNum(action));
    }

    void OnDestroy()
    {
        SessionState.SetFocusedAction(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SessionState.SetFocusedAction(associatedAction);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SessionState.SetFocusedAction(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && !SessionState.FormActive)
        {
            SessionState.CurrentStep.RemoveAction(associatedAction);
        }
    }

    public void InitActionItem(LabAction action)
    {
        associatedAction = action;
        actionText.text = action.GetActionString();
        actionNum.text = (SessionState.Steps[SessionState.ActiveStep].actions.IndexOf(action) + 1).ToString();
    }

    private void UpdateActionNum(LabAction action)
    {
        if (SessionState.CurrentStep.actions.IndexOf(action) < SessionState.CurrentStep.actions.IndexOf(associatedAction))
        {
            actionNum.text = (SessionState.Steps[SessionState.ActiveStep].actions.IndexOf(associatedAction)).ToString();
        }
    }
}

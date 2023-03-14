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

    public static Subject<GameObject> uiClickStream = new Subject<GameObject>();

    public Camera Camera;
    public RectTransform SelectionBox;
    public float DragDelay = 0.00001f;
    
    private LayerMask WellLayers;
    private float MouseDownTime;
    private Vector2 StartMousePosition;

    GraphicRaycaster raycaster;
    PointerEventData clickData;
    List<RaycastResult> clickResults;



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
        Debug.Log(SessionState.SelectionActive);
/*        if (Mouse.current.leftButton.wasReleasedThisFrame && !SessionState.FormActive)
        {
            var selectedObject = GetUiElementsClicked();
            if (selectedObject != null)
            {
                uiClickStream.OnNext(selectedObject);
            }
        }*/
        HandleSelectionInput();
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

    void HandleSelectionInput()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame && SessionState.ActiveTool.name == "micropipette")
        {
            //set up selection box
            SelectionBox.sizeDelta = Vector2.zero;
            SelectionBox.gameObject.SetActive(true);
            StartMousePosition = Mouse.current.position.ReadValue();
            MouseDownTime = Time.time;
        }
        else if(Mouse.current.leftButton.isPressed && MouseDownTime + DragDelay < Time.time && SessionState.ActiveTool.name == "micropipette")
        {
            //rezize box if holding and dragging
            ResizeSelectionBox();
            if(!SelectionManager.Instance.SelectionIsEmpty())
            {
                SessionState.SelectionActive = true;
            }
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            //deactive selection box
            SelectionBox.sizeDelta = Vector2.zero;
            SelectionBox.gameObject.SetActive(false);

            if(SelectionManager.Instance.SelectionIsEmpty())
            {
                SessionState.SelectionActive = false;
            }
            else
            {
                SessionState.SelectionActive = true;
            }

            //check what was clicked on
            RaycastHit2D hit = Physics2D.Raycast(Camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

            if (hit.collider != null && hit.collider.TryGetComponent<WellViewController>(out WellViewController well))
            {
                //if the shift key is held down edit selection
                if(Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
                {
                    if(SelectionManager.Instance.IsSelected(well))
                    {
                        SelectionManager.Instance.Deselect(well);
                    }
                    else
                    {
                        SelectionManager.Instance.Select(well);
                        SessionState.SelectionActive = true;
                    }
                }
                //if the shift key is not held and the selection is active deselect and add sample to all wells
                else if (MouseDownTime + DragDelay > Time.time && SessionState.SelectionActive)
                {
                    SelectionManager.Instance.DeselectAllAndAdd();
                }
                //else there is no group selection and we are selecting a single well
                else 
                {
                    if (SessionState.ActiveTool != null && SessionState.ActiveSample != null && SessionState.SelectionActive == false)
                    {
                        if (SessionState.ActiveTool.name == "micropipette")
                        {
                            //add active sample to well single
                            if (SessionState.AddActiveSampleToWell(well.name, false, false, false))
                            {
                                well.UpdateVisualState();
                            }
                        }
                        else
                        {
                            //add active sample to well
                            well.AddSampleMultichannel(SessionState.ActiveTool.numChannels);
                        }
                    }
                }
            }
            else if (MouseDownTime + DragDelay > Time.time && SessionState.SelectionActive)
            {
                if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed)
                {
                    SelectionManager.Instance.DeselectAllAndAdd();
                }
            }

            MouseDownTime = 0;
        }
        else if(Mouse.current.rightButton.wasReleasedThisFrame)
        {
            SelectionManager.Instance.DeselectAll();
        }
    }

    private void ResizeSelectionBox()
    {
        //set width and height from mouse position
        float width = Mouse.current.position.ReadValue().x - StartMousePosition.x;
        float height = Mouse.current.position.ReadValue().y - StartMousePosition.y;

        ///resize
        SelectionBox.anchoredPosition = StartMousePosition + new Vector2(width / 2, height / 2);
        SelectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        //create selection bounds
        Bounds bounds = new Bounds(SelectionBox.anchoredPosition, SelectionBox.sizeDelta);

        //select all wells in bounds
        for(int i=0; i<SelectionManager.Instance.AvailableWells.Count; i++)
        {
            if (WellIsInSelectionBox(Camera.WorldToScreenPoint(SelectionManager.Instance.AvailableWells[i].transform.position), bounds))
            {
                SelectionManager.Instance.Select(SelectionManager.Instance.AvailableWells[i]);
            }
            else
            {
                SelectionManager.Instance.Deselect(SelectionManager.Instance.AvailableWells[i]);
            }
        }
    }

    private bool WellIsInSelectionBox(Vector2 position, Bounds bounds)
    {
        return position.x > bounds.min.x && position.x < bounds.max.x
            && position.y > bounds.min.y && position.y < bounds.max.y;
    }
}

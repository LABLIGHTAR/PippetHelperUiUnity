using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SessionState : MonoBehaviour
{
    public static SessionState Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        //initalize state variables
        steps = new List<WellPlate>();
        SetStep(0);
        steps.Add(new WellPlate());
        availableLiquids = new List<Liquid>();
    }

    //class definitions
    public class Liquid
    {
        public string name;
        public string abreviation;
        public Color color;

        public Liquid(string name, string abreviation, Color color)
        {
            this.name = name;
            this.abreviation = abreviation;
            this.color = color;
        }
    }

    public class Well
    {
        public List<Liquid> liquids;

        public Well()
        {
            liquids = new List<Liquid>();
        }
    }

    public class WellPlate
    {
        public Dictionary<string, Well> wells;

        public WellPlate()
        {
            wells = new Dictionary<string, Well>();
        }
    }

    //state variables
    public static List<WellPlate> steps;
    public static int step;
    public static List<Liquid> availableLiquids;
    public static Liquid activeLiquid;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var selectedObject = GetMouseSelection();
            if (selectedObject != null)
            {
                if(selectedObject.GetComponent<LiquidSwatchViewController>())
                {
                    //set the active liquid to the selected liquid
                    activeLiquid = availableLiquids.Where(x => x.abreviation.Equals(selectedObject.GetComponent<LiquidSwatchViewController>().abreviation.GetComponent<TMP_Text>().text)).FirstOrDefault();
                    Debug.Log("Active Liquid: " + activeLiquid.name);
                }
                else if(selectedObject.GetComponent<WellViewController>())
                {
                    var selectedObjectController = selectedObject.GetComponent<WellViewController>();
                    Debug.Log(selectedObjectController.name);
                    //check if well is in current wellplate dictionary
                    var currentStep = steps[step];
                    if (currentStep.wells.ContainsKey(selectedObjectController.name) & activeLiquid != null)
                    {
                        //add the active liquid to the selected well
                        if (currentStep.wells[selectedObjectController.name].liquids.Count < 3)
                            currentStep.wells[selectedObjectController.name].liquids.Add(activeLiquid);
                    }
                    else
                    {
                        //if this well is not in the dictionary add it
                        currentStep.wells.Add(selectedObjectController.name, new Well());
                        if(activeLiquid != null)
                        {
                            //add the active liquid to the selected well
                            if (currentStep.wells[selectedObjectController.name].liquids.Count < 3)
                                currentStep.wells[selectedObjectController.name].liquids.Add(activeLiquid);
                        }
                    }
                }
            }
        }
    }

    //returns selected object or null if no object is selected
    GameObject GetMouseSelection()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitData = Physics2D.Raycast(new Vector2(mousePosition.x, mousePosition.y), Vector2.zero, 0);
        if (hitData)
        {
            var selectedObject = hitData.transform.gameObject;
            Debug.Log(selectedObject.name);
            return selectedObject;
        }
        return null;
    }

    //adds new liquid to the available liquids list
    public static void AddNewLiquid(string name, string abreviation, Color color)
    {
        availableLiquids.Add(new Liquid(name, abreviation, color));
    }

    public static void SetStep(int value)
    {
        if(value < steps.Count)
        {
            step = value;
        }
    }

    public static void AddNewStep()
    {
        steps.Add(new WellPlate());
    }
}

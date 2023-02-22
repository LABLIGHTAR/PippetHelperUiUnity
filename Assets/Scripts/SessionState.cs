using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UniRx;

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
        usedColors = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var selectedObject = GetMouseSelection();
        }
    }

    //class definitions
    public class Liquid
    {
        public string name;
        public string abreviation;
        public string colorName;
        public Color color;

        public Liquid(string name, string abreviation, string colorName, Color color)
        {
            this.name = name;
            this.abreviation = abreviation;
            this.colorName = colorName;
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

    public class Colors
    {
        public enum ColorNames
        {
            Lime,
            Green,
            Olive,
            Brown,
            Aqua,
            Blue,
            Navy,
            Slate,
            Purple,
            Plum,
            Pink,
            Salmon,
            Red,
            Orange,
            Yellow,
            Khaki
        }

        private static Hashtable colorValues = new Hashtable{
             {  ColorNames.Lime,    new Color32( 166 , 254 , 0, 255 ) },
             {  ColorNames.Green,   new Color32( 0 , 254 , 111, 255 ) },
             {  ColorNames.Olive,   new Color32( 85, 107, 47, 255 ) },
             {  ColorNames.Brown,   new Color32( 139, 69, 19, 255 ) },
             {  ColorNames.Aqua,    new Color32( 0 , 201 , 254, 255 ) },
             {  ColorNames.Blue,    new Color32( 0 , 122 , 254, 255 ) },
             {  ColorNames.Navy,    new Color32( 60 , 0 , 254, 255 ) },
             {  ColorNames.Slate,   new Color32( 72, 61, 139, 255 ) },
             {  ColorNames.Purple,  new Color32( 143 , 0 , 254, 255 ) },
             {  ColorNames.Plum,    new Color32( 221, 160, 221, 255 ) },
             {  ColorNames.Pink,    new Color32( 232 , 0 , 254, 255 ) },
             {  ColorNames.Salmon,  new Color32( 255, 160, 122, 255 ) },
             {  ColorNames.Red,     new Color32( 254 , 9 , 0, 255 ) },
             {  ColorNames.Orange,  new Color32( 254 , 161 , 0, 255 ) },
             {  ColorNames.Yellow,  new Color32( 254 , 224 , 0, 255 ) },
             {  ColorNames.Khaki,   new Color32( 240,230,140, 255 ) },
        };

        public static Color32 ColorValue(ColorNames color)
        {
            return (Color32)colorValues[color];
        }
    }

    //state variables
    private static List<WellPlate> steps;
    private static int step;
    private static List<Liquid> availableLiquids;
    private static Liquid activeLiquid;
    private static bool formActive;
    private static List<string> usedColors;

    //data streams
    public static Subject<int> stepStream = new Subject<int>();
    public static Subject<Liquid> activeLiquidStream = new Subject<Liquid>();
    public static Subject<Liquid> newLiquidStream = new Subject<Liquid>();
    public static Subject<GameObject> clickStream = new Subject<GameObject>();

    //setters
    public static List<WellPlate> Steps
    {
        set
        {
            if(steps != value)
            {
                steps = value;
            }
        }
        get
        {
            return steps;
        }
    }

    public static int Step
    {
        set
        {
            if(step != value)
            {
                step = value;
            }
            stepStream.OnNext(step);
        }
        get
        {
            return step;
        }
    }

    public static List<Liquid> AvailableLiquids
    {
        set
        {
            if(availableLiquids != value)
            {
                availableLiquids = value;
            }
        }
        get
        {
            return availableLiquids;
        }
    }

    public static Liquid ActiveLiquid
    {
        set
        {
            if(activeLiquid != value)
            {
                activeLiquid = value;
            }
            activeLiquidStream.OnNext(activeLiquid);
        }
        get
        {
            return activeLiquid;
        }
    }

    public static bool FormActive
    {
        set
        {
            if(formActive != value)
            {
                formActive = value;
            }
        }
        get
        {
            return formActive;
        }
    }

    public static List<string> UsedColors
    {
        set
        {
            if(usedColors != value)
            {
                usedColors = value;
            }
        }
        get
        {
            return usedColors;
        }
    }

    public static void SetStep(int value)
    {
        if(value < 0 || value > Steps.Count)
        {
            return;
        }
        else
        {
            Step = value;
        }
    }


    public static void AddNewStep()
    {
        Steps.Add(new WellPlate());
    }

    //adds new liquid to the available liquids list
    public static void AddNewLiquid(string name, string abreviation, string colorName, Color color)
    {
        Liquid newLiquid = new Liquid(name, abreviation, colorName, color);
        if (AvailableLiquids.Exists(x => x.name == name || x.abreviation == abreviation || x.colorName == colorName || x.color == color))
        {
            Debug.LogWarning("Liquid already exists");
            return;
        }
        else
        {
            AvailableLiquids.Add(newLiquid);
            newLiquidStream.OnNext(newLiquid);
        }
    }

    public static void SetActiveLiquid(Liquid selected)
    {
        if (!AvailableLiquids.Contains(selected))
        {
            Debug.LogWarning("Selected liquid is not available");
            return;
        }
        else
        {
            ActiveLiquid = selected;
        }
    }

    public static void AddActiveLiquidToWell(string wellName)
    {
        if(Steps[Step].wells.ContainsKey(wellName))
        {
            if(!Steps[Step].wells[wellName].liquids.Contains(ActiveLiquid))
            {
                //if the well exists and does not already have the active liquid add it
                Steps[Step].wells[wellName].liquids.Add(ActiveLiquid);
            }
            else
            {
                Debug.LogWarning("Well already contains the active liquid");
            }
        }
        else
        {
            //if the well does not exist create it
            Steps[Step].wells.Add(wellName, new Well());
            //add the active liquid to the new well
            Steps[Step].wells[wellName].liquids.Add(ActiveLiquid);
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
            clickStream.OnNext(selectedObject);
            return selectedObject;
        }
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;
using static SessionState.Well;

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

    //class definitions
    public class Liquid
    {
        public string name;
        public string abreviation;
        public string colorName;
        public Color color;
        public float volume;

        public Liquid(string name, string abreviation, string colorName, Color color, float volume)
        {
            this.name = name;
            this.abreviation = abreviation;
            this.colorName = colorName;
            this.color = color;
            this.volume = volume;
        }
    }

    public class Well
    {
        public struct LiquidGroup
        {
            public int groupId;
            public bool isStart;
            public bool isEnd;
            public Liquid liquid;

            public LiquidGroup(int groupId, bool isStart, bool isEnd, Liquid liquid)
            {
                this.groupId = groupId;
                this.isStart = isStart;
                this.isEnd = isEnd;
                this.liquid = liquid;
            }
        }

        public List<Liquid> liquids;
        public List<LiquidGroup> groups;

        public Well()
        {
            liquids = new List<Liquid>();
            groups = new List<LiquidGroup>();
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

    public class Tool
    {
        public string name;
        public int numChannels;
        public string orientation;

        public Tool(string name, int numChannels, string orientation)
        {
            this.name = name;
            this.numChannels = numChannels;
            this.orientation = orientation;
        }
    }

    //state variables
    private static List<WellPlate> steps;
    private static int step;
    private static List<Liquid> availableLiquids;
    private static Liquid activeLiquid;
    private static bool formActive;
    private static List<string> usedColors;
    private static Tool activeTool;
    private static int groupId;

    //data streams
    public static Subject<int> stepStream = new Subject<int>();
    public static Subject<Liquid> activeLiquidStream = new Subject<Liquid>();
    public static Subject<Liquid> newLiquidStream = new Subject<Liquid>();
    public static Subject<string> liquidRemovedStream = new Subject<string>();

    //setters
    public static List<WellPlate> Steps
    {
        set
        {
            if (steps != value)
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
            if (step != value)
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
            if (availableLiquids != value)
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
            if (activeLiquid != value)
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
            if (formActive != value)
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
            if (usedColors != value)
            {
                usedColors = value;
            }
        }
        get
        {
            return usedColors;
        }
    }

    public static Tool ActiveTool
    {
        set
        {
            if (activeTool != value)
            {
                activeTool = value;
            }
        }
        get
        {
            return activeTool;
        }
    }

    public static int GroupId
    {
        set
        {
            if(groupId != value)
            {
                groupId = value;
            }
        }
        get 
        { 
            return groupId; 
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
    public static void AddNewLiquid(string name, string abreviation, string colorName, Color color, float volume)
    {
        Liquid newLiquid = new Liquid(name, abreviation, colorName, color, volume);
        
        //return if the liquid already exists
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

    public static bool AddActiveLiquidToWell(string wellName, bool inGroup, bool isStart, bool isEnd)
    {
        if(Steps[Step].wells.ContainsKey(wellName))
        {
            if(!Steps[Step].wells[wellName].liquids.Contains(ActiveLiquid))
            {
                //if the well exists and does not already have the active liquid add it
                Steps[Step].wells[wellName].liquids.Add(ActiveLiquid);
                //if this liquid is grouped add it to the group list
                if(inGroup)
                {
                    Steps[Step].wells[wellName].groups.Add(new LiquidGroup(GroupId, isStart, isEnd, ActiveLiquid));
                    //if this is the last well in the group increment the group id for the next group
                    if(isEnd)
                    {
                        GroupId++; 
                    }
                }
                return true;
            }
            else
            {
                Debug.LogWarning("Well already contains the active liquid");
                return false;
            }
        }
        else
        {
            //if the well does not exist create it
            Steps[Step].wells.Add(wellName, new Well());
            //add the active liquid to the new well
            Steps[Step].wells[wellName].liquids.Add(ActiveLiquid);
            //if this liquid is grouped add it to the group list
            if (inGroup)
            {
                Steps[Step].wells[wellName].groups.Add(new LiquidGroup(GroupId, isStart, isEnd, ActiveLiquid));
                //if this is the last well in the group increment the group id for the next group
                if (isEnd)
                {
                    GroupId++;
                }
            }
            return true;
        }
    }

    public static bool RemoveActiveLiquidFromWell(string wellName)
    {
        if (Steps[Step].wells.ContainsKey(wellName))
        {
            if (Steps[Step].wells[wellName].liquids.Contains(ActiveLiquid))
            {
                //if the well exists and already has the active liquid remove it
                Steps[Step].wells[wellName].liquids.Remove(ActiveLiquid);

                //if the liquid being removed is part of a group remove the group everywhere
                if(Steps[Step].wells[wellName].groups != null)
                {
                    foreach(LiquidGroup group in Steps[Step].wells[wellName].groups)
                    {
                        if(group.liquid == ActiveLiquid)
                        {
                            int IdForRemoval = group.groupId;
                            //go through each well and remove all liquids in this group
                            RemoveAllLiquidsInGroup(IdForRemoval);
                            //break since the active liquid cannot be in a well mroe than once
                            break;
                        }
                    }
                }
                return true;
            }
            else
            {
                Debug.LogWarning("Well does not contain the active liquid");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Well is empty");
            return false;
        }
    }

    static void RemoveAllLiquidsInGroup(int removalID)
    {
        List<LiquidGroup> groupsToRemove = new List<LiquidGroup>();
        //iterate through all wells
        foreach (var well in Steps[Step].wells)
        {
            //iterate through each well group
            foreach(LiquidGroup group in well.Value.groups)
            {
                if(group.groupId == removalID)
                {
                    //add the group to a list for removal (cannot modify list in foreach loop)
                    groupsToRemove.Add(group);
                    //remove the active liquid
                    well.Value.liquids.Remove(ActiveLiquid);
                    //notify well
                    liquidRemovedStream.OnNext(well.Key);
                }
            }
            //remove groups
            well.Value.groups.RemoveAll(item => groupsToRemove.Contains(item));
            groupsToRemove.Clear();
        }
    }
}

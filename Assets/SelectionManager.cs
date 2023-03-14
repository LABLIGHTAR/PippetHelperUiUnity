using System.Collections.Generic;

public class SelectionManager
{
    private static SelectionManager _instance;

    public static SelectionManager Instance
    {
        get 
        { 
            if (_instance == null)
            {
                _instance = new SelectionManager();
            }
            return _instance; 
        }
        private set
        { 
            _instance = value; 
        }
    }
    
    public HashSet<WellViewController> SelectedWells = new HashSet<WellViewController>();
    public List<WellViewController> AvailableWells = new List<WellViewController>();

    public void Select(WellViewController well)
    {
        well.OnSelected();
        SelectedWells.Add(well);
    }

    public void Deselect(WellViewController well)
    {
        well.OnDeselected();
        SelectedWells.Remove(well);
    }

    public void DeselectAll()
    {
        foreach(WellViewController well in SelectedWells)
        {
            SessionState.RemoveActiveSampleFromWell(well.name, SessionState.Steps[SessionState.Step]);
            well.OnDeselected();
        }
        SelectedWells.Clear();
        SessionState.SelectionActive = false;
    }

    public void DeselectAllAndAdd()
    {
        foreach (WellViewController well in SelectedWells)
        {
            well.OnDeselected();
            if (SessionState.ActiveTool != null && SessionState.ActiveTool.name == "micropipette" && SessionState.ActiveSample != null)
            {
                if (SessionState.AddActiveSampleToWell(well.name, false, false, false))
                {
                    well.UpdateVisualState();
                }
            }
        }
        SelectedWells.Clear();
        SessionState.SelectionActive = false;
    }

    public bool IsSelected(WellViewController well)
    {
        return SelectedWells.Contains(well);
    }

    public bool SelectionIsEmpty()
    {
        if(SelectedWells.Count > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

using UnityEngine;
using UniRx;

public class WellPlateViewController : MonoBehaviour
{
    public Transform wells;
    public int id;

    // Start is called before the first frame update
    void Start()
    {
        AddWellsToDictionary();
        SessionState.newStepStream.Subscribe(_ => AddWellsToDictionary());
    }

    void AddWellsToDictionary()
    {
        foreach (Transform child in wells)
        {
            if(SessionState.Steps[SessionState.ActiveStep].materials[id] is Wellplate)
            {
                if (!SessionState.Steps[SessionState.ActiveStep].materials[id].ContainsWell(child.gameObject.name))
                {
                    SessionState.Steps[SessionState.ActiveStep].materials[id].AddWell(child.gameObject.name, new Well(child.name, id));
                }
            }
        }
    }
}

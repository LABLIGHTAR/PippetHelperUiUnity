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
            child.GetComponent<WellViewController>().plateId = id;

            if (SessionState.CurrentStep.materials[id] is Wellplate)
            {
                if (!SessionState.CurrentStep.materials[id].ContainsWell(child.gameObject.name))
                {
                    SessionState.CurrentStep.materials[id].AddWell(child.gameObject.name, new Well(child.name, id));
                }
            }
        }
    }
}

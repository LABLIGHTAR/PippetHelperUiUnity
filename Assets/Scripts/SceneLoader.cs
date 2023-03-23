using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public void LoadNewProcedureScene()
    {
        StartCoroutine(TransitionToNewProcedure());
    }

    public void LoadExistingProcedureScene()
    {
        StartCoroutine(TransitionToExistingProcedure());
    }


    IEnumerator TransitionToNewProcedure()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(1);
    }

    IEnumerator TransitionToExistingProcedure()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(2);
    }
}

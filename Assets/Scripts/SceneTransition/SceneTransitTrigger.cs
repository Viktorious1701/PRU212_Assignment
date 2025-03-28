using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitTrigger : MonoBehaviour
{
    public SceneTransitionController transitionController;
    public string targetSceneName;

    private void Start()
    {
        transitionController = FindAnyObjectByType<SceneTransitionController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            transitionController = FindAnyObjectByType<SceneTransitionController>();
            Debug.Log("Player entered trigger");
            GoToNextLevel();
        }
    }

    void GoToNextLevel()
    {
        Debug.Log("Going to next level");
        transitionController.TransitionToScene(targetSceneName);
    }
}

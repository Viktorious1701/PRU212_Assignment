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
            GoToNextLevel();
        }
    }

    void GoToNextLevel()
    {
        transitionController.TransitionToScene(targetSceneName);
    }
}

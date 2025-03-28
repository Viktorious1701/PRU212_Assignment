using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitTrigger : MonoBehaviour
{
    public SceneTransitionController transitionController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GoToNextLevel();
        }
    }

    void GoToNextLevel()
    {
        transitionController.TransitionToScene("SCENE1.1_Dungeon");
    }
}

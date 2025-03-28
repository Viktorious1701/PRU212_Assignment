using UnityEngine;

public class Lever : MonoBehaviour
{
    public int leverIndex;
    private LeverPuzzleSolver puzzleSolver;
    private Animator animator;
    private bool isOn = false;

    void Start()
    {
        puzzleSolver = GetComponentInParent<LeverPuzzleSolver>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        // Toggle the lever
        isOn = !isOn;

        // Update visual representation
        UpdateLeverVisuals();

        // Notify the puzzle solver about this lever activation
        puzzleSolver.UpdateSequence(leverIndex);
    }

    public void ResetLever()
    {
        if(isOn)
        {
            isOn = false;
            UpdateLeverVisuals();
        }
       
    }

    private void UpdateLeverVisuals()
    {
        // Implement lever visual state change
        // This could be a rotation, color change, or animation
        animator.SetTrigger("Turn");
    }
}
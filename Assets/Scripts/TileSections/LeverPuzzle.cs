using Cinemachine;
using System.Collections;
using UnityEngine;

public class LeverPuzzleSolver : MonoBehaviour
{
    [SerializeField] private Lever[] levers;
    [SerializeField] private GameObject ladderObject;
    [SerializeField] private float ladderDropSpeed = 2f;
    [SerializeField] private float ladderDropDistance = 5f;
    [SerializeField] private float resetDelay = 0.5f;

    // The correct sequence of lever activations
    [SerializeField] private int[] correctSequence;

    // Track the current sequence of lever activations
    private int[] currentSequence;

    void Start()
    {
        // Initialize the current sequence tracking
        currentSequence = new int[correctSequence.Length];
        ResetSequence();
    }

    public void UpdateSequence(int leverIndex)
    {
        // Shift the current sequence
        ShiftSequence(leverIndex);

        // Check if the current sequence matches the correct sequence
        if (CheckSequenceMatch())
        {
            SolvePuzzle();
        }
        else if (IsSequenceFull())
        {
            // If sequence is full and incorrect, reset all levers
            StartCoroutine(ResetPuzzle());
        }
    }

    private void ShiftSequence(int newLeverIndex)
    {
        // Move all elements left and add the new lever index
        for (int i = 0; i < currentSequence.Length - 1; i++)
        {
            currentSequence[i] = currentSequence[i + 1];
        }
        currentSequence[currentSequence.Length - 1] = newLeverIndex;
    }

    private bool CheckSequenceMatch()
    {
        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (currentSequence[i] != correctSequence[i])
            {
                return false;
            }
        }
        return true;
    }

    private void SolvePuzzle()
    {
       
        // Start the ladder drop coroutine
        StartCoroutine(DropLadder());
    }

    private System.Collections.IEnumerator DropLadder()
    {
         CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        Vector3 startPosition = ladderObject.transform.position;
        Vector3 endPosition = startPosition - Vector3.up * ladderDropDistance;

        float elapsedTime = 0;
        while (elapsedTime < ladderDropDistance / ladderDropSpeed)
        {
            ladderObject.transform.position = Vector3.Lerp(startPosition, endPosition,
                elapsedTime / (ladderDropDistance / ladderDropSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the ladder reaches exactly the end position
        ladderObject.transform.position = endPosition;

        // Trigger camera shake (assumes you'll set this up with Cinemachine)
    
    }
    private IEnumerator ResetPuzzle()
    {
        // Wait a short moment to let player see the incorrect sequence
        yield return new WaitForSeconds(resetDelay);

        // Reset all levers
        foreach (Lever lever in levers)
        {
            lever.ResetLever();
        }

        // Reset the sequence tracking
        ResetSequence();
    }

    private bool IsSequenceFull()
    {
        // Check if the sequence is completely filled
        for (int i = 0; i < currentSequence.Length; i++)
        {
            if (currentSequence[i] == -1)
            {
                return false;
            }
        }
        return true;
    }
    private void ResetSequence()
    {
        // Reset the sequence tracking
        for (int i = 0; i < currentSequence.Length; i++)
        {
            currentSequence[i] = -1;
        }
    }
}

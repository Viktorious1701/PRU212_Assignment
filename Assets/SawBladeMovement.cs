using UnityEngine;
using System.Collections;

public class SawbladeMovement : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float waitTime = 5f;

    private bool isMoving = false;

    // This public method is called from SawbladeTriggerZone
    public void BeginMovement()
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            // Move from current position to pointB
            yield return StartCoroutine(MoveToPoint(pointB.position));
            yield return new WaitForSeconds(waitTime);

            // Move back to pointA
            yield return StartCoroutine(MoveToPoint(pointA.position));
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        while (Vector2.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }
}

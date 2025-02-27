using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPos, length, height, startPosY;
    public GameObject cam;
    public float parallaxEffect; // The speed of the background move relatively with the camera


    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position.x;
        startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    
    void FixedUpdate()
    {
        //Calculate distance background move base on cam movement
        float distance = (cam.transform.position.x * parallaxEffect);
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        float distanceY = (cam.transform.position.y * parallaxEffect);
        float movementY = cam.transform.position.y * (1 - parallaxEffect);

        transform.position = new Vector3(startPos + distance, startPosY + distanceY, transform.position.z);

        if(movement > startPos + length)
        {
            startPos += length;
        }
        else if(movement < startPos - length)
        {
            startPos -= length;
        }
        
        if(movementY > startPosY + height)
        {
            startPosY += height;
        }
        else if(movementY < startPosY - height)
        {
            startPosY -= height;
        }
    }
}

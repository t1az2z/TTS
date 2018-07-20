using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour {

    [SerializeField] GameObject platform;
    Vector3 point1;
    [SerializeField] Transform point2;
    [SerializeField] float movementSpeed;
    bool movingToPoint2 = true;

    private void Start()
    {
        point1 = platform.transform.position;
    }

    private void FixedUpdate()
    {
        float step = movementSpeed * Time.fixedDeltaTime;
        if (movingToPoint2)
        {
            if (platform.transform.position != point2.position)
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, point2.position, step);
            }
            else
            {
                movingToPoint2 = false;
            }
        }
        else
        {
            if(platform.transform.position != point1)
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, point1, step);
            }
            else
            {
                movingToPoint2 = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        //gameObject.transform.position = point1.transform.position;
        if (point1 != null && point2 != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(platform.transform.position, point2.position);
        }
    }


}

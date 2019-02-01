using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycledMovement : MonoBehaviour {

    [SerializeField] Transform movingObject;
    Vector3 point1;
    public bool isMoving = false;
    public Collider2D trigger;
    [SerializeField] Transform point2;
    [SerializeField] float movementSpeed;
    bool movingToPoint2 = true;
    public float waitTimeUntilReset = .025f;
    private WaitForSeconds waitTime;
    public float waitTimeForCycle = .5f;
    private WaitForSeconds cycleWaitTime;
    
    private void Start()
    {
        point1 = movingObject.transform.position;
        waitTime = new WaitForSeconds(waitTimeUntilReset);
        cycleWaitTime = new WaitForSeconds(waitTimeForCycle);
    }

    private void Update()
    {
        if (isMoving) //&& GameController.Instance.player._currentState != PlayerState.Dead)
            StartCoroutine(Move(cycleWaitTime));
        if (GameController.Instance.player._currentState == PlayerState.Dead)
        {
            if (waitTimeUntilReset == 0)
                Reset();
            else
                StartCoroutine(Reset(waitTime));
        }
            
        
    }

    private IEnumerator Move(WaitForSeconds waitForSeconds)
    {
        float step = movementSpeed * Time.deltaTime;
        if (movingToPoint2)
        {
            if (movingObject.position != point2.position)
            {
                movingObject.position = Vector3.MoveTowards(movingObject.position, point2.position, step);
            }
            else
            {
                yield return waitForSeconds;
                movingToPoint2 = false;
            }
        }
        else
        {
            if (movingObject.transform.position != point1)
            {
                movingObject.position = Vector3.MoveTowards(movingObject.position, point1, step);
            }
            else
            {
                yield return waitForSeconds;
                movingToPoint2 = true;
            }
        }
    }

    public IEnumerator Reset(WaitForSeconds waitForSeconds)
    {
        yield return waitForSeconds;
        movingObject.position = point1;
    }

    public void Reset()
    {
        movingObject.position = point1;
    }


    void OnDrawGizmos()
    {

        if (point2 != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(movingObject.transform.position, point2.position);
        }
    }


}

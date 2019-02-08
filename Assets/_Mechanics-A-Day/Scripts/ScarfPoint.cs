using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarfPoint : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 offset = Vector3.zero;
    private Vector3 velocity;
    public float smthDampTime = .2f;
    bool playerFacingLeft = false;
    Vector2 playerVelocity;
    public Vector3 offsetVector;

    static float t = 0f;
    [Range(.005f, .3f)]
    public float tIncrement = .05f;

    void Update()
    {
        playerVelocity = GameController.Instance.player.velocity;
        if (playerVelocity.x <= .002f && playerVelocity.x >= -.002f)
        {
            playerFacingLeft = GameController.Instance.player.isFacingLeft;
            if (playerFacingLeft)
                offsetVector.x = Mathf.Abs(offset.x);
            else
                offsetVector.x = Mathf.Abs(offset.x) * -1;
        }
        else
        {
            offsetVector.x = Mathf.Sign(playerVelocity.x) * -1 * Mathf.Abs(offset.x);
        }

        if ((playerVelocity.y < .02f && playerVelocity.y > -.02f) && playerVelocity.y != 0)
        {
            offsetVector.y = Mathf.Lerp(offsetVector.y, offset.y, t);
            t += tIncrement * Time.deltaTime;
            if (t > .5f)
                t = 0;
        }
        else
        {
            offsetVector.y = 0; //todo find way to change this logic
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetToFollow.position + offsetVector, ref velocity, smthDampTime);
    }
}

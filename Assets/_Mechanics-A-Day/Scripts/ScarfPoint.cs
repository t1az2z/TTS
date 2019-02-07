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

    void Update()
    {
        playerVelocity = GameController.Instance.player.velocity;
        if (playerVelocity.x == 0)
        {
            playerFacingLeft = GameController.Instance.player.isFacingLeft;
            if (playerFacingLeft)
                offsetVector.x = Mathf.Abs(offset.x);
            else
                offsetVector.x = Mathf.Abs(offset.x) * -1;
        }
        else
        {
            offsetVector.x = Mathf.Sign(playerVelocity.x) * Mathf.Abs(offset.x)*-1;
        }
        if (playerVelocity.y <= .002f && playerVelocity.y >= -.002f)
            offsetVector.y = Mathf.Abs(offset.y) * -1;
        else
            offsetVector.y = 0;


        //offsetVector = new Vector3(
        //    Mathf.Sign(playerVelocity.x)*offset.x,
        //Mathf.Sign(-playerVelocity.y)*offset.y,
        //0
        //);
        //dotPosition = new Vector3(
        //    targetToFollow.position.x + dotPosition.x * -Mathf.Clamp(playerVelocity.x, -1, 1),
        //    targetToFollow.position.y + dotPosition.y * -Mathf.Clamp(playerVelocity.y, -1, 1),
        //    0
        //    );


        transform.position = Vector3.SmoothDamp(transform.position, targetToFollow.position + offsetVector, ref velocity, smthDampTime);
    }
}

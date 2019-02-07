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

    void Update()
    {
        playerFacingLeft = GameController.Instance.player.isFacingLeft;
        if (playerFacingLeft)
            offset.x = Mathf.Abs(offset.x);
        else
            offset.x = Mathf.Abs(offset.x) * -1;
        transform.position = Vector3.SmoothDamp(transform.position, targetToFollow.position + offset, ref velocity, smthDampTime);
    }
}

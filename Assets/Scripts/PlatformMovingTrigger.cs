using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovingTrigger : MonoBehaviour {

    [SerializeField] PlatformMovement platform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        platform.isMoving = true;
    }
}

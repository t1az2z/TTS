using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleHazard : MonoBehaviour
{
    public float radius = .5f;
    public LayerMask playerLayer;
    Collider2D[] circleOverlap = new Collider2D[1];
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        int circleOverlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, circleOverlap, playerLayer);
        if (circleOverlapCount != 0 && GameController.Instance.player._currentState != PlayerState.Dead)
            StartCoroutine(GameController.Instance.DeathCoroutine());
    }
}

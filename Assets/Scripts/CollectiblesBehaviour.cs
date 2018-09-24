using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiblesBehaviour : MonoBehaviour {

    private Transform targetToFollow;
    [SerializeField] float smthDampTime = .3f;
    private Vector3 velocity = Vector3.zero;
    public Vector3 followingOffset = new Vector3(-1, 1, 0);

    bool isPlayerGrounded;

	void Start () {
		
	}

    private void FixedUpdate()
    {
        if (targetToFollow != null)
        {
            FollowTarget(targetToFollow);
        }
    }

    private void FollowTarget(Transform target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetToFollow.position + followingOffset, ref velocity, smthDampTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        targetToFollow = collision.gameObject.transform;
        
    }
}

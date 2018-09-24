using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiblesBehaviour : MonoBehaviour {

    private Animator animator;
    private Transform targetToFollow;
    private PlayerController player;
    private Vector3 initialPosition;
    [SerializeField] float smthDampTime = .3f;
    [SerializeField] float timeToCollect = .25f;
    private Vector3 velocity = Vector3.zero;
    public Vector3 followingOffset = new Vector3(-1, 1, 0);
    [SerializeField] float destroyDelay = .5f;
    private GameController gc;
    bool collected = false;

	void Start ()
    {
        gc = FindObjectOfType<GameController>();
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
	}

    private void FixedUpdate()
    {
        if (player != null)
        {
            if (!player.isDead && !player.isGrounded && collected)
            {
                FollowTarget(player);
            }
            else if (player.isDead)
            {
                StartCoroutine(ResetParametersOnPlayerDeath());
            }
            else if(!player.isDead)
            {
                FollowTarget(player);
                StartCoroutine(CollectCollectible());
            }

        }
    }

    private IEnumerator ResetParametersOnPlayerDeath()
    {
        animator.Play("Disappear");
        StopCoroutine(CollectCollectible());
        print("cour stopped");
        yield return new WaitForSeconds(.75f);
        player = null;
        collected = false;
        transform.position = initialPosition;
        GetComponent<BoxCollider2D>().enabled = true;

        animator.Play("Idle");
    }

    private IEnumerator CollectCollectible()
    {
        if (collected && player.isGrounded)
        {
            collected = false;
            yield return new WaitForSeconds(timeToCollect);
            animator.Play("Collect");
            Destroy(gameObject, destroyDelay);
            gc.applesCollected++;
            print(gc.applesCollected);
        }
        print("Coroutie running");

    }

    private void FollowTarget(PlayerController target)
    {
        if (!player.isFacingLeft)
            transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + followingOffset, ref velocity, smthDampTime);
        else if (player.isFacingLeft)
            transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + (new Vector3(followingOffset.x *-1, followingOffset.y, followingOffset.z)), ref velocity, smthDampTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<PlayerController>();
        collected = true;
        GetComponent<BoxCollider2D>().enabled = false;
    }
}

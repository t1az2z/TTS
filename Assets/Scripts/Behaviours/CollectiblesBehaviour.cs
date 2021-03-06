﻿using System;
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
    [SerializeField] float destroyDelay = .26f;
    bool collected = false;
    GameObject lightObject;

	void Start ()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
        lightObject = transform.GetChild(0).gameObject;
	}

    private void Update()
    {
        if (player != null)
        {
            if (player._currentState != PlayerState.Grounded && player._currentState != PlayerState.Dead)//(!player.isDead && !player.isGrounded && collected)
            {
                FollowTarget(player);
            }
            else if (player._currentState == PlayerState.Dead)
            {
                StartCoroutine(ResetParametersOnPlayerDeath());
            }
            else if(player._currentState == PlayerState.Grounded)
            {
                FollowTarget(player);
                CollectCollectible();
            }

        }
    }

    private IEnumerator ResetParametersOnPlayerDeath()
    {
        animator.Play("Disappear");
        //StopCoroutine(CollectCollectible());
        yield return new WaitForSeconds(1F);
        player = null;
        collected = false;
        transform.position = initialPosition;
        GetComponent<BoxCollider2D>().enabled = true;

        animator.Play("Idle");
    }

    private void CollectCollectible()
    {
        if (player._currentState == PlayerState.Grounded && collected)
        {
            timeToCollect -= Time.deltaTime;
            if (timeToCollect <= Mathf.Epsilon)
            {
                animator.Play("Collect");
                lightObject.SetActive(false);
                Destroy(gameObject, destroyDelay);
                GameController.Instance.CollectiblesUpdate();
                collected = false;
            }
        }
        else
        {
            timeToCollect = .25f;
        }
        /*if (collected && player.isGrounded)
        {
            collected = false;

            animator.Play("Collect");
            Destroy(gameObject, destroyDelay);
            GameController.Instance.applesCollected++;
            print(GameController.Instance.applesCollected);
        }
        print("Coroutie running");*/


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

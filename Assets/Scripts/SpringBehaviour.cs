﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringBehaviour : MonoBehaviour {

    Collider2D col;
    [SerializeField] Vector2 springVector = new Vector2(0, 10f);
    PlayerController player;
	void Start ()
    {
        col = GetComponent<Collider2D>();
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player == null)
        {
            player = collision.gameObject.GetComponent<PlayerController>();
        }

        if (collision.relativeVelocity.y <= Mathf.Epsilon)
        {
            player.rb.velocity = springVector;
            player.jumpsCount = 2;

            player.springJumping = true;
        }
    }
}
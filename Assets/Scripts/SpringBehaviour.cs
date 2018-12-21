﻿
using UnityEngine;
using Cinemachine;

public class SpringBehaviour : MonoBehaviour {

    [SerializeField] Vector2 springVector = new Vector2(0, 10f);
    PlayerController player;
    CinemachineImpulseSource impulse;
    Animator anim;

    private void Start()
    {
        impulse = GetComponent<CinemachineImpulseSource>();
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player == null)
        {
            player = collision.gameObject.GetComponent<PlayerController>();
        }

        if (collision.relativeVelocity.y <= Mathf.Epsilon)
        {
            //player.isDashing = false;
            //player.dashExpireTime = 0;
            player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            anim.Play("Spring");
            player.dashRequest = false;
            player.currentState = PlayerState.SpringJump;

            player.rb.velocity = springVector;
            player.dustParticles.Play();
            player.jumpsCount = 2;
            player.dashAlow = true;
            impulse.GenerateImpulse();

        }
    }
}

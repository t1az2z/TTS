using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowGravityArea : MonoBehaviour
{
    public float gravityInArea;
    PlayerController player;
    private float playerGravity;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.GetComponent<PlayerController>();
            player.currentGravity = gravityInArea;
            playerGravity = player.gravity;
            player.gravity = gravityInArea;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        player.gravity = gravityInArea;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player.gravity = playerGravity;
    }

}

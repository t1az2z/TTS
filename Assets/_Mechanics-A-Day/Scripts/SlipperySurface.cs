using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipperySurface : MonoBehaviour
{
    public float slipperyDamping = 2;
    private float playerGroundDamping;
    public PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerGroundDamping = player.groundDamping;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (player._currentState == PlayerState.Grounded || player._currentState == PlayerState.Dash));
            player.groundDamping = slipperyDamping;  
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player._currentState == PlayerState.Grounded)
            player.groundDamping = slipperyDamping;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player.groundDamping = playerGroundDamping;
    }
}

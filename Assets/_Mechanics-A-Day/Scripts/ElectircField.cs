using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kills player if it have battery charge.
/// If not - changes color recharges battery on exit.
/// </summary>
public class ElectircField : MonoBehaviour
{
    PlayerController player;
    Animator anim;
    public int batteryRestore = 2;
    public int batteryThreshold = 0;
    private bool hazard = true;
    private bool playerInsideAndAlive = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            player = GameController.Instance.player;

        if (player != null)
        {
            if (!playerInsideAndAlive)
            {
                hazard = player.batteryCapacity - player.batterySpent <= batteryThreshold ? false : true;
                anim.SetBool("Hazard", hazard);
            }
            else if (playerInsideAndAlive)
            {
                anim.SetBool("Hazard", false);
                hazard = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (hazard)
                StartCoroutine(GameController.Instance.DeathCoroutine());
            else
            {
                player.batterySpent = 0;
                playerInsideAndAlive = true;
            }
        }   
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
       if (collision.CompareTag("Player"))
        {
            if (player._currentState != PlayerState.Dead)
            {
                playerInsideAndAlive = true;
            }
            else
            {
                playerInsideAndAlive = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hazard)
            playerInsideAndAlive = false;
    }
}

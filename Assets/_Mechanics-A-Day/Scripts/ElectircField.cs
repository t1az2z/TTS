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
            hazard = player.batteryCapacity - player.batterySpent <= batteryThreshold ? false : true;
            anim.SetBool("Hazard", hazard);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && hazard)
            StartCoroutine(GameController.Instance.DeathCoroutine());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hazard)
            player.batterySpent = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesAfterTouching : MonoBehaviour
{
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (GameController.Instance.player._currentState == PlayerState.Dead)
            anim.Play("No_spikes");
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.Play("Spikes");
        }

    }
}

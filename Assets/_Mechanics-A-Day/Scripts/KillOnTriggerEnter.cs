using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnTriggerEnter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            StartCoroutine(GameController.Instance.DeathCoroutine());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            StartCoroutine(GameController.Instance.DeathCoroutine());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBehaviour : MonoBehaviour
{
    private DoorBehaviour doorBehaviour;
    private Collider2D collider;

    public void Init(DoorBehaviour doorBehaviour)
    {
        this.doorBehaviour = doorBehaviour;
        collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //play activated animation & sound
            doorBehaviour.ActivateKey();
            collider.enabled = false;
        }
    }

    public void KeyReset()
    {
        collider.enabled = true;
    }
}

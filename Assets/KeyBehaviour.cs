using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBehaviour : MonoBehaviour
{
    private DoorBehaviour doorBehaviour;
    private Collider2D col;

    public void Init(DoorBehaviour doorBehaviour)
    {
        this.doorBehaviour = doorBehaviour;
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //play activated animation & sound
            doorBehaviour.ActivateKey();
            col.enabled = false;
        }
    }

    public void KeyReset()
    {
        col.enabled = true;
    }
}

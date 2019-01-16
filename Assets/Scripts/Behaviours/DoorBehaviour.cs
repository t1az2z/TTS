using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] KeyBehaviour[] keys;
    Animator anim;
    int keysAmmount;

    private void Start()
    {
        keysAmmount = keys.Length;
        anim = GetComponent<Animator>();
        foreach(var key in keys)
        {
            key.Init(this);
        }
    }

    private void Update()
    {
        if (GameController.Instance.player.currentState == PlayerState.Dead)
            DoorReset();
    }
    public void ActivateKey()
    {
        keysAmmount--;
        if (keysAmmount == 0)
            DoorOpen();
    }

    void DoorOpen()
    {
        anim.Play("Door_Open");
    }

    public void DoorReset()
    {
        keysAmmount = keys.Length;
        foreach(var key in keys)
        {
            key.KeyReset();
        }
        anim.Play("Door_Idle");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateVirtualCamera : MonoBehaviour {

    GameObject vcam;
    GameController gameController;

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        vcam = transform.GetChild(0).gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameController.SwitchCamera(vcam);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!vcam.activeSelf)
        {
            gameController.SwitchCamera(vcam);
        }
    }
}

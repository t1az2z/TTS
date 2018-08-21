using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateVirtualCamera : MonoBehaviour {

    public GameObject vcam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        vcam.SetActive(true);
    }
}

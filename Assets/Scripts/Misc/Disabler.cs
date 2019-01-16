using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disabler : MonoBehaviour {

    SpriteRenderer sr;
	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        sr.enabled = false;
    }
}

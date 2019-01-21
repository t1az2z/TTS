using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickering : MonoBehaviour {

    Animator anim;

    
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        anim.Play(0, 0, Random.Range(0f, 1f));
	}



    private void OnTriggerEnter2D(Collider2D collision)
    {
        anim.SetTrigger("Walk Near");
        if (collision.gameObject.GetComponent<PlayerController>()._currentState == PlayerState.Dash)
            anim.Play("Off");
    }

}

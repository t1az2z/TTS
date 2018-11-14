using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour {

    [SerializeField] float deactTime = 2.5f;
    [SerializeField] float reactTime = 3f;
    Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        anim.SetFloat("DeactTime", deactTime);
        anim.SetFloat("ReactTime", reactTime);
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        anim.SetBool("Active", false);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        anim.SetBool("Active", false);

    }
}

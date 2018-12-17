using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour {

    [SerializeField] float deactTime = 2.5f;
    [SerializeField] float reactTime = 3f;
    Animator anim;
    [Tooltip("Tile for disappearing block. Should be the first child of an object")]
    public Transform visuals;
    [Tooltip("Particle system for disappearing block. Should be the second child of an object")]
    public ParticleSystem particles;
    GameController gc;

	// Use this for initialization
	void Start () {
        if (visuals == null)
        {
            Debug.Log("Visuals reference not set");
            visuals =  transform.GetChild(0);
        }
        gc = FindObjectOfType<GameController>();
        if (particles == null)
        {
            Debug.Log("Particles reference not set");
            particles = transform.GetChild(1).GetComponent<ParticleSystem>();
        }
        anim = GetComponent<Animator>();
        anim.SetFloat("DeactTime", deactTime);
        anim.SetFloat("ReactTime", reactTime);
	}
    void Update()
    {
        if (gc != null && gc.player.isDead)
        {
            anim.SetTrigger("Reset");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.GetComponent<PlayerController>().currentState == PlayerState.Grounded)// || collision.collider.gameObject.GetComponent<PlayerController>().wallSliding)
            anim.SetBool("Active", false);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.GetComponent<PlayerController>().currentState == PlayerState.Grounded)// || collision.collider.gameObject.GetComponent<PlayerController>().wallSliding)
            anim.SetBool("Active", false);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReseterBehaviour : MonoBehaviour {

    PlayerController player;
    [SerializeField] bool resetDash = true;
    [SerializeField] bool resetJumpCounter = true;
    SpriteRenderer sr;
    bool isActive = true;

    [SerializeField] float reactivateTime = 3f;
	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.GetComponent<PlayerController>();

        if (isActive)
        {
            if (resetDash)
            {
                player.dashAlow = true;
                print(player.dashAlow);
            }
            if (resetJumpCounter)
            {
                player.jumpsCount = 0;
                print(player.jumpsCount);
            }

            isActive = false;
        }

        StartCoroutine(TemporarlyDeactivate());


    }

    private IEnumerator TemporarlyDeactivate()
    {
        //deactive animation
        sr.enabled = false;
        yield return new WaitForSeconds(reactivateTime);
        //activate animation
        sr.enabled = true;
        isActive = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReseterBehaviour : MonoBehaviour {

    PlayerController player;
    [SerializeField] bool resetDash = true;
    [SerializeField] bool resetJumpCounter = true;
    SpriteRenderer sr;
    bool isActive = true;
    float waitTime = .15f;
    GameObject light;




    [SerializeField] float reactivateTime = 3f;

	void Start () {
        sr = GetComponent<SpriteRenderer>();
        light = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.isDead)
            {
                StopAllCoroutines();
                sr.enabled = true;
                light.SetActive(true);
                isActive = true;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.GetComponent<PlayerController>();

        if (isActive)
        {
            if (resetDash)
            {
                if (!player.isDashing)
                {
                    player.dashAlow = true;
                    print(player.dashAlow);
                }
                else if (player.isDashing)
                {
                    StartCoroutine(ResetDashAfterDelay());
                }
            }
            if (resetJumpCounter)
            {
                player.jumpsCount = 0;
            }

            isActive = false;
        }

        StartCoroutine(TemporarlyDeactivate());

    }

    private IEnumerator ResetDashAfterDelay()
    {
        yield return new WaitForSeconds(waitTime);
        player.dashAlow = true;

    }

    private IEnumerator TemporarlyDeactivate()
    {
        //deactive animation
        sr.enabled = false;
        light.SetActive(false);
        yield return new WaitForSeconds(reactivateTime);
        //activate animation
        sr.enabled = true;
        light.SetActive(true);
        isActive = true;
    }
    private IEnumerator ReactivateAdPlayersDeath()
    {
        yield return new WaitForSeconds(.05f);
        
    }
}

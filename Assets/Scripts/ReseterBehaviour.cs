using System.Collections;
using UnityEngine;
using Cinemachine;

public class ReseterBehaviour : MonoBehaviour {

    PlayerController player;
    [SerializeField] bool resetDash = true;
    [SerializeField] bool resetJumpCounter = true;
    SpriteRenderer sr;
    bool isActive = true;
    float waitTime = .5f;
    float resetTime;
    GameObject light;
    CinemachineImpulseSource impulse;



    [SerializeField] float reactivateTime = 3f;

	void Start () {
        sr = GetComponent<SpriteRenderer>();
        light = transform.GetChild(0).gameObject;
        impulse = GetComponent<CinemachineImpulseSource>();
        resetTime = waitTime;
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
                }
                else if (player.isDashing)
                {
                    StartCoroutine(ResetDashAfterDelay(waitTime));
                }
            }
            if (resetJumpCounter)
            {
                player.jumpsCount = 0;
            }
            impulse.GenerateImpulse();
            isActive = false;
        }

        StartCoroutine(TemporarlyDeactivate());

    }

    private IEnumerator ResetDashAfterDelay(float time)
    {
        while (time > 0)
        {
            if (player.isDashing)
            {
                yield return new WaitForSeconds(.01f);
            }
            else
            {
                player.dashAlow = true;
                break;
            }
            time -= Time.deltaTime;
        }

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
    private IEnumerator ReactivateAtPlayersDeath(float timeDelay)
    {
        yield return new WaitForSeconds(.05f);
        sr.enabled = true;
        light.SetActive(true);
        isActive = true;
    }
}

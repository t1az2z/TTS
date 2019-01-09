using System.Collections;
using UnityEngine;
using Cinemachine;

public class ReseterBehaviour : MonoBehaviour {

    PlayerController player;
    [SerializeField] bool resetDash = true;
    [SerializeField] bool resetJumpCounter = true;
    Collider2D col;
    SpriteRenderer sr;
    bool isActive = true;
    float waitTime = .5f;
    GameObject lightObject;
    CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem particles;


    [SerializeField] float reactivateTime = 3f;

	void Start () {

        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        lightObject = transform.GetChild(0).gameObject;
        impulse = GetComponent<CinemachineImpulseSource>();
        if (particles == null)
        {
            Debug.Log("Particles reference not set");
            particles = transform.GetChild(1).GetComponent<ParticleSystem>();
        }
    }

    /*private void Update()
    {
        if (player != null)
        {
            if (player.currentState == PlayerState.Dead)
            {
                StopAllCoroutines();
                sr.enabled = true;
                lightObject.SetActive(true);
                col.enabled = true;
                isActive = true;
            }
        }
    }*/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.GetComponent<PlayerController>();

        if (isActive && player.currentState != PlayerState.Dead)
        {
            particles.Play();
            if (resetDash)
            {
                if (player.currentState != PlayerState.Dash)
                {
                    player.dashAlow = true;
                }
                else if (player.currentState == PlayerState.Dash)
                {
                    StartCoroutine(ResetDashAfterDelay(waitTime));
                }
            }
            if (resetJumpCounter)
            {
                player.jumpsCount = 0;
            }
            impulse.GenerateImpulse();
            StartCoroutine(GameController.Instance.FreezeTime(.06f));
            isActive = false;
        }

        StartCoroutine(TemporarlyDeactivate());

    }

    private IEnumerator ResetDashAfterDelay(float time)
    {
        WaitForSeconds delay = new WaitForSeconds(.01f);
        while (time > 0)
        {
            if (player.currentState == PlayerState.Dash)
            {
                yield return delay;
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
        WaitForSeconds reactTime = new WaitForSeconds(reactivateTime);
        //deactive animation
        col.enabled = false;
        sr.enabled = false;
        lightObject.SetActive(false);
        yield return reactTime;
        //activate animation
        sr.enabled = true;
        col.enabled = true;
        lightObject.SetActive(true);
        isActive = true;
    }
    public void ReactivateAtPlayersDeath()
    {
        StopAllCoroutines();
        sr.enabled = true;
        lightObject.SetActive(true);
        col.enabled = true;
        isActive = true;
    }
}

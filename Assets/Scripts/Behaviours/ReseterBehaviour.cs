using System.Collections;
using UnityEngine;
using Cinemachine;

public class ReseterBehaviour : MonoBehaviour {

    PlayerController player;
    [SerializeField] bool resetDash = true;
    [SerializeField] bool resetJumpCounter = true;
    Collider2D col;
    [SerializeField] SpriteRenderer sr;
    bool isActive = true;
    float waitTime = .25f;
    GameObject lightObject;
    CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem particles;
    [SerializeField] float freezeTime = .02f;

    [SerializeField] float reactivateTime = 3f;

	void Start () {

        col = GetComponent<Collider2D>();
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
            if (player._currentState == PlayerState.Dead)
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

        if (isActive && player._currentState != PlayerState.Dead)
        {
            particles.Play();
            /*if (resetDash)
            {
                if (player._currentState != PlayerState.Dash)
                {
                    player.dashAlow = true;
                }
                else if (player._currentState == PlayerState.Dash)
                {
                    StartCoroutine(ResetDashAfterDelay(waitTime));
                }
            }
            if (resetJumpCounter)
            {
                player.batterySpent = 0;
            }*/
            if (player._currentState != PlayerState.Dash)
                player.ResetVariablesAndRequests();
            else if (player._currentState == PlayerState.Dash)
            {
                StartCoroutine(ResetDashAfterDelay(waitTime));
            }
            impulse.GenerateImpulse();
            StartCoroutine(GameController.Instance.FreezeTime(freezeTime));
            isActive = false;
        }

        StartCoroutine(TemporarlyDeactivate());

    }

    private IEnumerator ResetDashAfterDelay(float time)
    {
        WaitForSeconds delay = new WaitForSeconds(.01f);
        while (time > 0)
        {
            if (player._currentState == PlayerState.Dash)
            {
                yield return delay;
            }
            else
            {
                player.ResetVariablesAndRequests();
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

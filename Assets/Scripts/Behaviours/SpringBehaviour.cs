
using UnityEngine;
using Cinemachine;

public class SpringBehaviour : MonoBehaviour {

    [SerializeField] Vector2 springVector = new Vector2(0, 10f);
    PlayerController player;
    CinemachineImpulseSource impulse;
    Animator anim;
    [SerializeField] int jumpsAfterSpring = 2;

    private void Start()
    {
        impulse = GetComponent<CinemachineImpulseSource>();
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player == null)
        {
            player = collision.gameObject.GetComponent<PlayerController>();
        }

        if (collision.relativeVelocity.y <= Mathf.Epsilon)
        {
            //player.isDashing = false;
            //player.dashExpireTime = 0;
            player.gravityActive = true;
            anim.Play("Spring");
            player.dashRequest = false;
            player._currentState = PlayerState.SpringJump;

            player.velocity = springVector;
            player.dustParticles.Play();
            player.jumpsCount = jumpsAfterSpring;
            player.dashAlow = true;
            impulse.GenerateImpulse();

        }
    }
}


using UnityEngine;
using Cinemachine;

public class SpringBehaviour : MonoBehaviour {

    [SerializeField] Vector2 springVector = new Vector2(0, 10f);
    PlayerController player;
    CinemachineImpulseSource impulse;
    [SerializeField] float timeToStop = .025f;
    GameController gc;

    private void Start()
    {
        gc = FindObjectOfType<GameController>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player == null)
        {
            player = collision.gameObject.GetComponent<PlayerController>();
        }

        if (collision.relativeVelocity.y <= Mathf.Epsilon)
        {
            StartCoroutine(gc.FreezeTime(timeToStop));
            player.rb.velocity = springVector;
            player.jumpsCount = 2;
            player.dashAlow = true;
            impulse.GenerateImpulse();
            player.springJumping = true;
        }
    }
}

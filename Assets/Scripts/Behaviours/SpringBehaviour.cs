
using UnityEngine;
using Cinemachine;

public class SpringBehaviour : MonoBehaviour {

    public Vector2 springVector = new Vector2(0, 10f);
    public CinemachineImpulseSource impulse;
    public Animator anim;
    public int springJumpCost = 1;
    public bool activated = false;
    public float inactiveTime = .4f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        if (activated)
        {
            impulse.GenerateImpulse();
            anim.Play("Spring");

            activated = false;
        }
    }
}

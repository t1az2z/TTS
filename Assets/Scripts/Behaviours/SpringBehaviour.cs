
using UnityEngine;
using Cinemachine;

public class SpringBehaviour : MonoBehaviour {

    public Vector2 springVector = new Vector2(0, 10f);
    public CinemachineImpulseSource impulse;
    public Animator anim;
    public int springJumpCost = 1;
    public bool activated = false;
    public bool deactivateControlls = false;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        if (springVector.x != 0)
            deactivateControlls = true;
        else if (springVector.x == 0)
            deactivateControlls = false;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, springVector/3);
    }
}

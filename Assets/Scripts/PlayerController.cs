using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody2D rb;
    [SerializeField] float jumpForce = 5.25f;
    private bool isGrounded;
    public bool jumpRequest = false;
    [SerializeField] int allowedJumps = 2;
    int jumpsCount = 0;

    [SerializeField] Transform groundCheck;
    const float groundedRadius = .2f;
    [SerializeField] LayerMask whatIsGround;

    void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
	}
	
	void Update ()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
        
	}

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;

            }


            if (jumpRequest && (jumpsCount < allowedJumps))
            {
                Jump();
                jumpRequest = false;
                jumpsCount++;
            }
        }
    }

    private void Jump()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = jumpForce;
        rb.velocity = velocity;
    }
}

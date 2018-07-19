using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody2D rb;
    [Header("Jump parameters:")]
    [SerializeField] float jumpForce = 5.25f;
    public bool isGrounded;
    bool jumpRequest = false;
    bool holdJumpButton = false;
    [SerializeField] int allowedJumps = 2;
    public int jumpsCount;

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 2.7f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Ground parameters")]
    [SerializeField] Transform groundCheck;
    const float groundedRadius = .1f;
    [SerializeField] LayerMask whatIsGround;

    [Header("Spin parameters:")]
    [SerializeField] float spinTime = 2f;

    public bool spinAllow = true;
    public bool spinRequest = false;

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


    public void OnPressSpin()
    {
        spinRequest = true;
        Spin();
    }
    public void OnReleaseSpin()
    {
        spinRequest = false;
        Spin();
    }


    private void FixedUpdate()
    {
        GroundCheck();
        MultipleJumpProcessing();
        GravityScaleChange();

    }

    public void Spin()
    {
        if (spinRequest && spinAllow)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        }
        else if (!spinRequest)
        {
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            spinAllow = false;
        }
    }

    public void JumpRequest()
    {
        jumpRequest = true;
    }

    private void GravityScaleChange()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }

        else
        {
            rb.gravityScale = 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrounded)
        {
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        transform.SetParent(null);
    }


    private void MultipleJumpProcessing()
    {
        if (jumpRequest && (jumpsCount < allowedJumps))
        {
            jumpsCount++;
            Jump();
            jumpRequest = false;

        }
        else
        {
            jumpRequest = false;
        }
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;


        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                jumpsCount = 1;
                spinAllow = true;
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

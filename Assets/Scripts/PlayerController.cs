using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;

    [Header("Run parameters")]
    [SerializeField] float runSpeed;
    bool isFacingLeft;
    bool isRunning = false;
    float xMovement;
    
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
    const float groundedRadius = .3f;
    [SerializeField] LayerMask whatIsGround;

    [Header("Spin parameters:")]
    [SerializeField] float spinTime = 2f;

    public bool spinAllow = true;
    public bool spinRequest = false;
    bool isSpinning = false;

    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        if (CrossPlatformInputManager.GetButton("Spin"))
        {
            spinRequest = true;
        }
        else if (CrossPlatformInputManager.GetButtonUp("Spin"))
        {
            spinRequest = false;
        }
        RunInputProcessing();
        SetAnimationsParameters();
    }

    void RunInputProcessing()
    {
        xMovement = CrossPlatformInputManager.GetAxis("Horizontal");
        if (xMovement < 0)
        {
            isFacingLeft = true;
            isRunning = true;
        }

        else if (xMovement > 0)
        {
            isFacingLeft = false;
            isRunning = true;
        }
        else
            isRunning = false;
            
    }

    void Run()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = xMovement * runSpeed * Time.deltaTime;
        print(velocity.x);
        rb.velocity = velocity;
    }

    private void SetAnimationsParameters()
    {
        animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("JumpRequest", jumpRequest);
        animator.SetBool("isSpinning", isSpinning);
        animator.SetBool("isFacingLeft", isFacingLeft);
        animator.SetFloat("xSpeed", xMovement);
        animator.SetBool("isRunning", isRunning);
    }

    public void OnPressSpin()
    {
        spinRequest = true;
    }
    public void OnReleaseSpin()
    {
        spinRequest = false;
    }


    private void FixedUpdate()
    {
        Run();
        Spin();
        GroundCheck();
        MultipleJumpProcessing();
        GravityScaleChange();

    }

    public void Spin()
    {
        if (spinRequest)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            isSpinning = true;
        }
        else if (!spinRequest)
        {
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            isSpinning = false;
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
        //todo пофиксить и добавить isGrounded, найти баг
        transform.SetParent(collision.transform);
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
        if(transform.parent && transform.parent.GetComponent<Rigidbody2D>())
        {
            velocity.x = transform.parent.GetComponent<Rigidbody2D>().velocity.y;
        }
        rb.velocity = velocity;
    }
}
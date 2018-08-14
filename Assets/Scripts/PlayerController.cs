using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;
    Animator animator;
    [SerializeField] GameObject visuals;

    [Header("Run parameters")]
    [SerializeField] float runSpeed;
    bool isFacingLeft;
    bool isRunning;
    float xInput;
    [Space(8)]

    [Header("Jump parameters:")]
    [SerializeField] float jumpMaxForce = 5.25f;
    [SerializeField] float jumpMinForce = 3f;
    public bool isGrounded;
    bool jumpRequest = false;
    public bool jumpCancel = false;
    [SerializeField] int allowedJumps = 2;
    public int jumpsCount = 1;
    private const float velocityTopSlice = 3f;
    [Space(8)]

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 2.7f;
    [Space(8)]

    [Header("Ground parameters")]
    [SerializeField] Transform[] groundChecks;
    public float groundedRadius = .03125f;
    [SerializeField] LayerMask whatIsGround;

    [Space(8)]

    [Header("Dash parameters:")]
    [SerializeField] float dashTime = 2f;
    public bool dashAlow = true;
    public bool dashRequest = false;
    public bool isDashing = false;
    public float dashExpireTime;
    [Space(8)]

    [Header("Wall jump parameters")]
    [SerializeField] Transform[] wallChecksLeft;
    [SerializeField] Transform[] wallChecksRight;
    public float wallCheckRadius = .03125f;
    [SerializeField] LayerMask whatIsWalls;
    public bool wallJumped;
    public bool wallJumping;
    bool wallSliding = false;
    public float wallJumpXVelocity = 5f;
    int wallDirX;
    [SerializeField] float wallSlidingSpeed = 3f;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        visuals = transform.Find("Visuals").gameObject;
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        PcControlls();

        VariablesResetOnGround();
        
        isRunning = Mathf.Abs(rb.velocity.x)>Mathf.Epsilon;
        if (isRunning)
            visuals.transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        //SetAnimationsParameters();


    }

    private void VariablesResetOnGround()
    {
        if (isGrounded)
        {
            dashAlow = true;
            wallJumping = false;
        }

        bool OldIsGrounded = isGrounded;
        isGrounded = GroundCheck();
        if (OldIsGrounded != isGrounded && isGrounded)
        {
            jumpsCount = 0;
            jumpCancel = false;
        }
    }



    private void PcControlls()
    {
        if (SimpleInput.GetButtonDown("Jump"))
        {
            OnPressJump();
        }
        else if (SimpleInput.GetButtonUp("Jump"))
        {
            OnReleaseJump();
        }

        if (SimpleInput.GetButtonDown("Dash"))
        {
            OnPressDash();
        }
        else if (SimpleInput.GetButtonUp("Dash"))
        {
            OnReleaseDash();
        }
    }

    private void FixedUpdate()
    {
        Run();
        Dash();
        GroundCheck();
        JumpLogicProcessing();
        HandleWallSliding();
        GravityScaleChange();
    }

    private void HandleWallSliding()
    {
        wallDirX = WallHit();
        wallSliding = false;
        if ((wallDirX == -1 || wallDirX == 1)  && Mathf.Sign(xInput) ==wallDirX && xInput != 0)
        {
            wallSliding = true;

            if (rb.velocity.y < -wallSlidingSpeed  && rb.velocity.y < 0)
            {
                var velocity = rb.velocity;
                velocity.y = -wallSlidingSpeed;
                rb.velocity = velocity;
            }

            jumpsCount = 0;
            jumpCancel = false;

            /* if (timeToWallUnstick > 0f)
             {
                 var velocity = rb.velocity;
                 velocity.x = 0f;
                 rb.velocity = velocity;
                 if ((int)Mathf.Sign(xInput) == -wallDirX)
                 {
                     timeToWallUnstick -= Time.fixedDeltaTime;
                 }
                 else
                 {
                     timeToWallUnstick = wallStickTime;
                 }
             }
             else
             {
                 wallSliding = false;
                 rb.AddForce(Vector2.right * (-wallDirX));
                 timeToWallUnstick = wallStickTime;
             }*/
        }
  
    }

    void Run()
    {
        xInput = SimpleInput.GetAxis("Horizontal");

        if (!wallJumping)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = xInput * runSpeed * 100 * Time.deltaTime;
            rb.velocity = velocity;
        }
    }

    private void SetAnimationsParameters()
    {
        animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isSpinning", isDashing);
        animator.SetBool("isFacingLeft", isFacingLeft);
        animator.SetFloat("xSpeed", xInput);
        animator.SetBool("isRunning", isRunning);
    }

    public void OnPressDash()
    {
        dashRequest = true;
        dashExpireTime = Time.time + dashTime;
    }
    public void OnReleaseDash()
    {
        dashRequest = false;
        dashExpireTime = 0f;
        dashAlow = false;
    }
    public void Dash()
    {
        if (!isGrounded)
        {
            if (dashRequest && dashExpireTime >= Time.time && dashAlow)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                isDashing = true;
                Vector2 velocity = rb.velocity;
                velocity.x = Mathf.Sign(xInput)*runSpeed*300*Time.fixedDeltaTime;
                rb.velocity = velocity;
            }
            else if (!dashRequest || dashExpireTime < Time.time)
            {
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                isDashing = false;
            }
        }
    }

    public void OnPressJump()
    {
        jumpRequest = true;
        if (wallSliding && jumpRequest && !isGrounded && Mathf.Sign(xInput) == wallDirX)
        {
            wallJumped = true;
        }
    }

    public void OnReleaseJump()
    {
        jumpRequest = false;
        //wallJumping = false; m.b. use for "meatboy" style wall jumps 
        if (!isGrounded)
            jumpCancel = true;
    }

    private void JumpLogicProcessing()
    {
        Vector2 velocity = rb.velocity;

        if (jumpRequest && (jumpsCount < allowedJumps))
        {
            jumpCancel = false;
            jumpsCount++;
            if (wallJumped)
            {
                rb.velocity = new Vector2(wallJumpXVelocity * (-wallDirX), jumpMaxForce);
                wallJumping = true;
                wallJumped = false;
            }
            else
            {
                Jump(velocity);
            }
            jumpRequest = false;
        }
        else
        {
            jumpRequest = false;
        }

        if (jumpCancel && rb.velocity.y >= velocityTopSlice) //todo сделать красиво (хотя и так работает)
        {
            velocity.y = velocityTopSlice;
            rb.velocity = velocity;
            jumpCancel = false;
        }
        if (rb.velocity.y < 0)
            wallJumping = false;

    }

    private void Jump( Vector2 vel)
    {
        vel.y = jumpMaxForce;
        rb.velocity = vel;
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
    private bool GroundCheck()
    {
        bool isGrnd = false;
        foreach (var groundch in groundChecks)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundch.position, groundedRadius, whatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    isGrnd = true;
                }
            }
        }
        return isGrnd;
    }

    private int WallHit()
    {
        foreach (var wallCheck in wallChecksLeft)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheck.position, groundedRadius, whatIsWalls);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    return -1;
                }
            }
        }
        foreach (var wallCheck in wallChecksRight)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheck.position, groundedRadius, whatIsWalls);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var groundch in groundChecks)
        {
            if (groundch != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundch.position, groundedRadius);
            }
        }
        foreach (var wallCH in wallChecksLeft)
        {
            if (wallCH != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(wallCH.position, groundedRadius);
            }
        }foreach (var wallCH in wallChecksRight)
        {
            if (wallCH != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(wallCH.position, groundedRadius);
            }
        }
    }
}
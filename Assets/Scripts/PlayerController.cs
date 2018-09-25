using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameController gc;
    public Rigidbody2D rb;
    public Animator animator;
    SpriteRenderer spriteRenderer;



    public bool isDead = false;
    public bool controllsEnabled = true;

    [Header("Run parameters")]
    [SerializeField] float runSpeed;
    public bool isFacingLeft;
    bool isRunning;
    float xInput;
    [Space(8)]

    [Header("Jump parameters:")]
    [SerializeField] float jumpMaxForce = 9.3f;
    public bool isGrounded;
    bool jumpRequest = false;
    public bool jumpCancel = false;
    public bool springJumping = false;
    [SerializeField] int allowedJumps = 2;
    public int jumpsCount = 1;
    private const float velocityTopSlice = 3f;
    [SerializeField] float maxFallVelocity = -10f;
    [Space(8)]

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 1.2f;
    [Space(8)]

    [Header("Ground parameters")]
    [SerializeField] Transform[] groundChecks;
    public float groundedRadius = .03125f;
    [SerializeField] LayerMask whatIsGround;

    [Space(8)]

    [Header("Dash parameters:")]
    //public GameObject dashDenyIndicator;
    [SerializeField] float dashTime = 2f;
    bool dashAlow = true;
    bool dashRequest = false;
    bool isDashing = false;
    float dashExpireTime;

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
    bool isWallHit = false;
    [SerializeField] float wallSlidingSpeed = 3f;

    private void Awake()
    {
        gc = FindObjectOfType<GameController>();
        animator = GetComponent<Animator>();

    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
            if(controllsEnabled)
                PcControlls();

            VariablesResetOnGround();

            FlipSprite();

            SetAnimationsParameters();

        if (isDead)
        {
            dashRequest = false;
            jumpRequest = false;
            dashAlow = false;
        }
    }



    private void FlipSprite()
    {
        isRunning = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (isRunning)
        {
            if (rb.velocity.x < Mathf.Epsilon)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (rb.velocity.x > Mathf.Epsilon)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
    }

    private void VariablesResetOnGround()
    {
        if (isGrounded)
        {
            dashAlow = true;

            wallJumping = false;
            springJumping = false;
            //if (dashDenyIndicator.activeSelf)
            //    dashDenyIndicator.SetActive(false);
        }

        bool OldIsGrounded = isGrounded;
        isGrounded = GroundCheck();
        if (OldIsGrounded != isGrounded && isGrounded)
        {
            jumpsCount = 0;
            jumpCancel = false;

        }
    }
    private void VariablesResetOnWallHit()
    {
        bool OldWallHit = isWallHit;
        isWallHit = wallSliding;
        if (OldWallHit != isWallHit && isWallHit)
        {
            jumpsCount = 0;
            jumpCancel = false;
            springJumping = false;
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
            //OnReleaseDash();
        }
    }

    private void FixedUpdate()
    {
        if (controllsEnabled)
        {
            Run();
            Dash();
            GroundCheck();
            JumpLogicProcessing();
            HandleWallSliding();
            GravityScaleChange();
        }
        if(isDead)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX| RigidbodyConstraints2D.FreezeRotation;
        }

    }

    private void HandleWallSliding()
    {
        VariablesResetOnWallHit();

        wallDirX = WallHit();
        wallSliding = false;
        if ((wallDirX == -1 || wallDirX == 1)  && Mathf.Sign(xInput) == wallDirX && xInput != 0 && rb.velocity.y <= 0)
        {
            wallSliding = true;
            if (rb.velocity.y < -wallSlidingSpeed  && rb.velocity.y < 0)
            {
                var velocity = rb.velocity;
                velocity.y = -wallSlidingSpeed;
                rb.velocity = velocity;
            }
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
        if (!isDead)
        {

            if (isDashing)
                animator.Play("Dash");
            else if (isGrounded && rb.velocity.x == 0)
            {
                animator.Play("Idle");
            }
            else if (isGrounded && rb.velocity.x != 0)
            {
                animator.Play("Walk");
            }
            else if ((!isGrounded && !wallSliding) || wallJumping)
            {
                if (rb.velocity.y > 0 && jumpsCount <= 1)
                    animator.Play("Jump");
                else if (jumpsCount >= 2 && !isDashing)
                {
                    if (rb.velocity.y < -4)
                        animator.Play("Fall");
                    else
                        animator.Play("Jump2");
                }
                else if (rb.velocity.y < -.5f)
                    animator.Play("Fall");

            }
            else if (!wallJumping && !isGrounded && wallSliding && rb.velocity.y <= 0)
                animator.Play("Climb");

        }
    }

    public void OnPressDash()
    {
        if (!dashRequest)
        {
            dashRequest = true;
            dashExpireTime = dashTime;
        }
    }
    public void OnReleaseDash()
    {
        dashAlow = false;
        
    }
    public void Dash()
    {
        if (!dashAlow)
            dashRequest = false;
        if (dashRequest)
        {
            if (dashExpireTime > Mathf.Epsilon && dashAlow)
            {
                int dashDirection = isFacingLeft ? -1 : 1;
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                isDashing = true;
                Vector2 velocity = rb.velocity;
                velocity.x = dashDirection * runSpeed * 300 * Time.fixedDeltaTime;
                dashExpireTime -= Time.fixedDeltaTime;
                rb.velocity = velocity;

                //dashDenyIndicator.SetActive(true);
                //todo stop-frame
                //todo screen shake
            }
            else if (dashExpireTime <= Mathf.Epsilon)
            {
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                isDashing = false;
                dashRequest = false;
                dashExpireTime = 0f;
                dashAlow = false;
            }
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            isDashing = false;
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
            /*else if (isDashing)
            {
                isDashing = false;
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                rb.velocity = new Vector2(rb.velocity.x*.6f, jumpMaxForce *.7f); //todo rework
            }*/
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

        if (jumpCancel && rb.velocity.y >= velocityTopSlice && !springJumping) //todo сделать красиво (хотя и так работает)
        {
            velocity.y = velocityTopSlice;
            rb.velocity = velocity;
            jumpCancel = false;
        }
        if (rb.velocity.y < 1)
            wallJumping = false;

    }

    private void Jump( Vector2 vel)
    {
        vel.y = jumpMaxForce;
        rb.velocity = vel;
    }
    private void GravityScaleChange()
    {
        if (!isDead)
        {
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = fallMultiplier;
                if (rb.velocity.y <= maxFallVelocity)
                {
                    rb.velocity = new Vector2(rb.velocity.x, maxFallVelocity);
                }
            }
            else
            {
                rb.gravityScale = 1;
            }
        }
        else
        {
            rb.gravityScale = 0;
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
        if (rb.velocity.y <= Mathf.Epsilon)
            return isGrnd;
        else return false;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("CheckPoint"))
        {
            gc.SetChekpoint(collision.transform.position);
        }
        else if(collision.CompareTag("Hazards"))
        {
            StartCoroutine(gc.DeathCoroutine());
            dashRequest = false;
            isDashing = false;
            isDead = true;
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public GameController gc;
    public Rigidbody2D rb;
    public Animator animator;
    SpriteRenderer spriteRenderer;

    public GameObject wallslideParticles;
    private ParticleSystem.EmissionModule wsEmission;


    public bool isDead = false;
    public bool controllsEnabled = true;
    public ParticleSystem deathParticles;
    [Header("Run parameters")]
    [SerializeField] float runSpeed;
    public bool isFacingLeft;
    bool isRunning;
    float xInput;

    //[SerializeField] ParticleSystem runParticles;
    //private ParticleSystem.EmissionModule runEmission;

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
    [SerializeField] float groundCheckLength = .0625f;
    public ParticleSystem jumpParticles;
    [Space(8)]

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 1.2f;
    [Space(8)]

    [Header("WllCheck parameters")]
    [SerializeField] Transform[] groundChecks;
    public float wallcheckRadius = .03125f;
    [SerializeField] LayerMask whatIsGround;



    [Space(8)]

    [Header("Dash parameters:")]
    //public GameObject dashDenyIndicator;
    [SerializeField] float dashTime = .14f;
    [SerializeField] float dashFreezeTime = .02f;
    public bool dashAlow = true;
    bool dashRequest = false;
    public bool isDashing = false;
    [HideInInspector] public float dashExpireTime;
    public CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem dashParticles;
    public ParticleSystem dustParticles;
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
    [SerializeField] float initialWallSlidingVelocity = 3f;
    [SerializeField] float maxlWallSlidingVelocity = 3f;
    [SerializeField] float wallslidingVelocityStep = .3f;
    [SerializeField] float wallJumpForce = 9.76f;


    private void Awake()
    {
        gc = FindObjectOfType<GameController>();
        animator = GetComponent<Animator>();

    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        impulse = GetComponent<CinemachineImpulseSource>();
        wsEmission = wallslideParticles.GetComponent<ParticleSystem>().emission;
        //runEmission = runParticles.emission;


    }
    void Update()
    {
        if (controllsEnabled)
            PcControlls();

        GroundInteractionLogic();

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
        /*else
            runEmission.enabled = false;*/
    }

    private void GroundInteractionLogic()
    {
        if (isGrounded)
        {
            dashAlow = true;

            wallJumping = false;
            //if (dashDenyIndicator.activeSelf)
            //    dashDenyIndicator.SetActive(false);
        }

        bool OldIsGrounded = isGrounded;
        isGrounded = GroundCheck();
        if (OldIsGrounded != isGrounded && isGrounded)
        {
            jumpsCount = 0;
            jumpCancel = false;
            springJumping = false;

            if (!isDead && controllsEnabled)
                jumpParticles.Play();
        }
    }
    private void WallsInteractionLogic()
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
            HorizontalMovement();
            Dash();
            GroundCheck();
            JumpLogicProcessing();
            HandleWallSliding();
            GravityScaleChange();
        }
        if (isDead)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

    }

    public IEnumerator FreezePlayer(float time)
    {
        /*var vel = rb.velocity;
        animator.speed = 0;
        yield return new WaitForSeconds(time/2);
        controllsEnabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(time/2);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = vel;
        animator.speed = 1;
        controllsEnabled = true;*/
        yield return new WaitForEndOfFrame();
    }

    private void HandleWallSliding()
    {
        WallsInteractionLogic();

        wallDirX = WallHit();
        wallSliding = false;
        if (wallDirX == -1)
            wallslideParticles.transform.localPosition = new Vector2(-.14f, wallslideParticles.transform.localPosition.y);
        else
            wallslideParticles.transform.localPosition = new Vector2(.14f, wallslideParticles.transform.localPosition.y);
        if ((wallDirX == -1 || wallDirX == 1) && Mathf.Sign(xInput) == wallDirX && xInput != 0 && rb.velocity.y <= 0)
        {
            wallSliding = true;
            if (rb.velocity.y < -initialWallSlidingVelocity && rb.velocity.y < 0)
            {
                var velocity = rb.velocity;
                velocity.y = -maxlWallSlidingVelocity;
                rb.velocity = velocity;
            }
        }
        wsEmission.enabled = (wallSliding && !isGrounded);

        


    }

    void HorizontalMovement()
    {
        xInput = SimpleInput.GetAxis("Horizontal");
        Vector2 velocity = rb.velocity;
        if (!wallJumping && controllsEnabled)
        {
            velocity.x = xInput * runSpeed * 100 * Time.deltaTime;
            rb.velocity = velocity;
        }
        else if (wallJumping)
        {
            rb.AddForce(new Vector2(xInput * runSpeed * 450 * Time.fixedDeltaTime, 0), ForceMode2D.Force);
            if (rb.velocity.x > 5f)
            {
                velocity.x = 5f;
                rb.velocity = velocity;
            }
            else if (rb.velocity.x <= -5f)
            {
                velocity.x = -5f;
                rb.velocity = velocity;
            }
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
                //runEmission.enabled = true;
            }
            else if ((!isGrounded && !wallSliding) || wallJumping)
            {
                //runEmission.enabled = false;
                if (rb.velocity.y > 0 && jumpsCount <= 1)
                    animator.Play("Jump");
                else if (jumpsCount >= 2 && !isDashing)
                {
                    if (rb.velocity.y < -6)
                        animator.Play("Fall");
                    else
                        animator.Play("Jump2");
                }
                else if (rb.velocity.y < -.5f)
                    animator.Play("Fall");

            }
            else if (!wallJumping && !isGrounded && wallSliding && rb.velocity.y <= 0)
            {
                animator.Play("Climb");
            }

        }
    }

    public void OnPressDash()
    {
        if (!dashRequest)
        {
            StartCoroutine(gc.FreezeTime(dashFreezeTime));
            dashRequest = true;
            dashExpireTime = dashTime;
        }
    }
    public void OnReleaseDash()
    {
        //dashAlow = false;
        
    }
    public void Dash()
    {
        if (!dashAlow)
            dashRequest = false;
        if (dashRequest)
        {
            dashParticles.Play();
            if (dashExpireTime > Mathf.Epsilon && dashAlow)
            {
                dustParticles.Play();
                int dashDirection = isFacingLeft ? -1 : 1;
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                isDashing = true;
                Vector2 velocity = rb.velocity;
                if (xInput == 0)
                    velocity.x = dashDirection * runSpeed * 300 * Time.fixedDeltaTime;
                else
                    velocity.x = Mathf.Sign(xInput) * runSpeed * 300 * Time.fixedDeltaTime;

                dashExpireTime -= Time.fixedDeltaTime;
                rb.velocity = velocity;
                impulse.GenerateImpulse(new Vector3(0, dashDirection, 0));

                //dashDenyIndicator.SetActive(true);

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
        if (!isGrounded && !springJumping)
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
                rb.velocity = new Vector2(wallJumpXVelocity * (-wallDirX), wallJumpForce);
                wallJumping = true;
                wallJumped = false;
            }
            else
            {
                jumpParticles.Play();
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
        if (rb.velocity.y < 0 || Mathf.Sign(xInput) == -wallDirX || xInput == 0)
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
            if (rb.velocity.y < 0 && !wallSliding)
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

        Vector2 direction = Vector2.down;
        foreach (var groundch in groundChecks)
        {
            RaycastHit2D hit = Physics2D.Raycast(groundch.position, direction, groundCheckLength, whatIsGround);
            if (hit.collider != null && rb.velocity.y <= 0)
                isGrnd = true;
        }
        return isGrnd;
    }

    private int WallHit()
    {
        foreach (var wallCheck in wallChecksLeft)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheck.position, wallcheckRadius, whatIsWalls);
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
            Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheck.position, wallcheckRadius, whatIsWalls);
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
                Gizmos.DrawRay(groundch.position, new Vector2(0, -groundCheckLength));
            }
        }
        foreach (var wallCH in wallChecksLeft)
        {
            if (wallCH != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(wallCH.position, wallcheckRadius);
            }
        }foreach (var wallCH in wallChecksRight)
        {
            if (wallCH != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(wallCH.position, wallcheckRadius);
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
            wallSliding = false;
            wsEmission.enabled = false;
            isDead = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazards") && rb.velocity.y <= 0)
        {
            StartCoroutine(gc.DeathCoroutine());
            dashRequest = false;
            isDashing = false;
            wallSliding = false;
            wsEmission.enabled = false;
            isDead = true;
        }
        
    }
}
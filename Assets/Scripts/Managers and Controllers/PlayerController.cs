using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Homebrew;

public class PlayerController : MonoBehaviour
{
    #region Properties
    public PlayerState currentState;
    private PlayerState prevFrameState;
    private PlayerState prevState;
    [Foldout("Setup", true)]
    //public GameController gc;
    public Rigidbody2D rb;
    public Animator animator;
    SpriteRenderer spriteRenderer;
    private float timeInState = 0f;
    private float timeInPrevState;
    public GameObject wallslideParticles;
    private ParticleSystem.EmissionModule wsParticlesEmissionModule;
    public float controllsDisabledTimer = .2f;
    private Vector2 velocity;

    public bool isDead = false;
    public bool controllsEnabled = true;
    public ParticleSystem deathParticles;
    [Space(8)]
    [Foldout("Run parameters", true)]
    public float runSpeed;
    public bool isFacingLeft;
    bool isRunning;
    float xInput;

    //[SerializeField] ParticleSystem runParticles;
    //private ParticleSystem.EmissionModule runEmission;

    [Space(8)]

    [Foldout("Jump parameters", true)]
    [SerializeField] float jumpMaxForce = 9.3f;
    public bool isGrounded;
    public bool jumpRequest = false;
    public bool jumpCancel = false;
    public bool springJumping = false;
    [SerializeField] int allowedJumps = 2;
    public int jumpsCount = 1;
    private const float velocityTopSlice = 3f;
    [SerializeField] float maxFallVelocity = -10f;
    [SerializeField] float groundCheckLength = .0625f;
    [SerializeField] float landingTime = .21f;
    public ParticleSystem jumpParticles;
    private bool reachedMaxFallVelocity = false;
    [Space(8)]

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 1.2f;
    [Space(8)]

    [Foldout("Wall check parameters", true)]
    [SerializeField] Transform[] groundChecks;
    public float wallCheckLength = .0625f;
    [SerializeField] LayerMask whatIsGround;



    [Space(8)]

    [Foldout("Dash parameters", true)]
    public bool dashEnabled = false;
    int dashDirection = 0;
    [SerializeField] float dashTime = .14f;
    [SerializeField] float dashFreezeTime = .17f;
    [SerializeField] float dashSpeed = 200;
    public bool dashAlow = true;
    public bool dashRequest = false;
    [SerializeField] Vector2 destroyWallsKnockback = new Vector2(2, 2);
    [HideInInspector] public float dashExpireTime;
    public CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem dashParticles;
    public ParticleSystem dustParticles;

    [Space(8)]

    [Foldout("Wall jump/slide parameters", true)]
    int wallDir;
    [SerializeField] Transform[] wallChecksLeft;
    [SerializeField] Transform[] wallChecksRight;
    [SerializeField] LayerMask whatIsWalls;
    public bool wallJumped;
    public bool wallJumping;
    public float wallJumpXVelocity = 5f;
    [SerializeField] float wallJumpForce = 9.76f;

    bool isWallHit = false;
    [SerializeField] float initialWallSlidingVelocity = 1.5f;
    [SerializeField] float maxlWallSlidingVelocity = 3f;
    [SerializeField] float wallSlideTimeToReachMaxVelocity = 1f;
    private float wsTimeTRMV;



    Vector3 scaleChange = new Vector3(1, 1, 1);
    #endregion

    #region MonoBehaviour Events
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        impulse = GetComponent<CinemachineImpulseSource>();
        wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
        wsParticlesEmissionModule.enabled = false;
        currentState = PlayerState.Grounded;
        wsTimeTRMV = wallSlideTimeToReachMaxVelocity;
    }
    void Update()
    {
        SetAnimationsParameters();
        if (controllsEnabled)
            PcControlls();

        FlipSprite();
        
        xInput = SimpleInput.GetAxisRaw("Horizontal");

        if (currentState == PlayerState.Dead)
        {
            dashRequest = false;
            jumpRequest = false;
            dashAlow = false;
        }
        if (currentState == PlayerState.Fall || currentState == PlayerState.Grounded || currentState == PlayerState.WallSlide || currentState == PlayerState.SpringJump)
            GravityScaleChange();


        /*if (Mathf.Abs(rb.velocity.x) > 5.5)
        {
            scaleChange.x = Mathf.Lerp(scaleChange.x, 1.2f, .1f);
            scaleChange.y = Mathf.Lerp(scaleChange.y, .8f, .1f);
        }
        else
            scaleChange.x = Mathf.Lerp(scaleChange.x, 1, .1f);
        if (Mathf.Abs(rb.velocity.y) > 5.5f)
        {
            scaleChange.x = Mathf.Lerp(scaleChange.x, .8f, .1f);
            scaleChange.y = Mathf.Lerp(scaleChange.y, 1.2f, .1f);
        }
        else
            scaleChange.y = Mathf.Lerp(scaleChange.y, 1, .1f);

        transform.localScale = scaleChange;

      
        print(rb.velocity);*/
    }
    private void FixedUpdate()
    {
        velocity = rb.velocity;

        if (prevState != prevFrameState && prevFrameState != currentState)
            prevState = currentState;
        prevFrameState = currentState;
        if (currentState != PlayerState.WallSlide)
            wsTimeTRMV = wallSlideTimeToReachMaxVelocity;
        switch (currentState)
        {
            case PlayerState.Grounded:
                HorizontalMovement(xInput);
                GroundInteractionLogic();
                if (jumpRequest)
                {
                    currentState = PlayerState.Jump;
                }

                else if (dashRequest)
                {
                    currentState = PlayerState.Dash;
                }
                else if (rb.velocity.y < -Mathf.Epsilon)
                {
                    currentState = PlayerState.Fall;
                }
                break;

            case PlayerState.Jump:

                HorizontalMovement(xInput);
                JumpLogicProcessing();
                wallDir = WallHitCheck();
                if (dashRequest)
                    currentState = PlayerState.Dash;
                else if (rb.velocity.y < 0f && jumpsCount < 2)
                    currentState = PlayerState.Fall;
                else if (rb.velocity.y < -3f && jumpsCount == 2)
                    currentState = PlayerState.Fall;
                else if (wallDir != 0 && Mathf.Sign(xInput) == wallDir && xInput != 0)
                    currentState = PlayerState.WallSlide;
                else if (jumpRequest)
                    JumpLogicProcessing();
                else if (GroundCheck())
                {
                    currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }


                break;

            case PlayerState.Dash:
                reachedMaxFallVelocity = false;
                Dash();
                break;

            case PlayerState.Fall:
                wallDir = WallHitCheck();
                HorizontalMovement(xInput);
                if (dashRequest)
                {
                    currentState = PlayerState.Dash;
                }
                if (jumpRequest)
                {
                    currentState = PlayerState.Jump;
                    JumpLogicProcessing();
                }
                else if (GroundCheck())
                {
                    currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                else if (wallDir != 0 && Mathf.Sign(xInput) == wallDir && xInput != 0)
                {
                    WallsInteractionLogic();
                    currentState = PlayerState.WallSlide;
                }
                break;

            case PlayerState.WallSlide:
                var wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
                reachedMaxFallVelocity = false;
                wallDir = WallHitCheck();
                WallSlideLogicProcessing(wallDir, wsParticlesEmissionModule);

                if (jumpRequest)
                {
                    wsParticlesEmissionModule.enabled = false;
                    if (Mathf.Sign(xInput) == wallDir)
                        currentState = PlayerState.WallJump;
                    else
                        currentState = PlayerState.Jump;
                }
                else if (dashRequest)
                {
                    wsParticlesEmissionModule.enabled = false;
                    currentState = PlayerState.Dash;
                }
                else if (GroundCheck())
                {
                    wsParticlesEmissionModule.enabled = false;
                    currentState = PlayerState.Grounded;
                }
                else if (wallDir == 0 && rb.velocity.y > 0)
                {
                    currentState = PlayerState.Jump;
                    jumpsCount = 1;
                }
                break;

            case PlayerState.WallJump:

                wallDir = WallHitCheck();
                if (jumpsCount < 1 && jumpRequest)
                    WallJumpLogicProcessing(wallDir);
                else if (jumpsCount >= 1 && jumpRequest)
                    currentState = PlayerState.Jump;
                if (jumpCancel && rb.velocity.y >= velocityTopSlice)
                {

                    velocity.y = velocityTopSlice;
                    rb.velocity = velocity;
                    jumpCancel = false;
                }
                WallJumpingHorizontalMovemetn();

                if (dashRequest)
                    currentState = PlayerState.Dash;

                else if (rb.velocity.y <= -.5f)
                    currentState = PlayerState.Fall;
                else if (wallDir != 0 && rb.velocity.y <= .1)
                {
                    WallsInteractionLogic();
                    currentState = PlayerState.WallSlide;
                }
                else if (GroundCheck())
                {
                    currentState = PlayerState.Grounded;
                }

                break;

            case PlayerState.SpringJump:
                HorizontalMovement(xInput);
                if (jumpsCount < 2)
                    JumpLogicProcessing();
                if (dashRequest)
                    currentState = PlayerState.Dash;
                else if (rb.velocity.y < -3)
                    currentState = PlayerState.Fall;
                else if (GroundCheck())
                {
                    currentState = PlayerState.Grounded;
                }
                break;

            case PlayerState.WallBreak:
                dashExpireTime = 0;
                dashRequest = false;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                dashParticles.Stop();
                WallJumpingHorizontalMovemetn();
                if (controllsDisabledTimer > 0)
                {

                    velocity = new Vector2(wallJumpXVelocity * (-dashDirection) * destroyWallsKnockback.x, destroyWallsKnockback.y * Mathf.Abs(dashDirection));

                    rb.velocity = velocity;
                    controllsDisabledTimer -= Time.fixedDeltaTime;
                    dashAlow = true;
                    jumpsCount = 0;
                }
                else
                {
                    if (GroundCheck())
                    {
                        currentState = PlayerState.Grounded;
                    }
                    else
                    {
                        currentState = PlayerState.Fall;
                    }
                }
                break;

            case PlayerState.Dead:
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                reachedMaxFallVelocity = false;
                GameController.Instance.DeathCoroutine();
                break;

            case PlayerState.Disabled:

                break;

        }
        if (prevFrameState != currentState)
        {
            if (currentState != PlayerState.Dead)
            {
                timeInPrevState = timeInState;
                timeInState = 0;
            }
        }
        else
            timeInState += Time.fixedDeltaTime;

    }
    #endregion

    #region Controlls
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

        if (SimpleInput.GetButtonDown("Dash") && dashEnabled)
        {
            OnPressDash();
        }
    }

    public void OnPressJump()
    {
        jumpRequest = true;
        jumpCancel = false;
    }

    public void OnReleaseJump()
    {
        jumpRequest = false;
        if (!isGrounded && !springJumping)
            jumpCancel = true;
    }

    public void OnPressDash()
    {
        if (!dashRequest)
        {
            dashRequest = true;
            dashExpireTime = dashTime;
            if (dashDirection == 0)
                dashDirection = isFacingLeft ? -1 : 1;
        }
    }

    #endregion

    #region Gameplay Actions Logic

    public void Dash()
    {
        if (dashRequest && !dashAlow)
            dashRequest = false;

        else if (dashRequest)
        {
            dashParticles.Play();


            if (dashExpireTime == dashTime)
            {
                StartCoroutine(GameController.Instance.FreezeTime(dashFreezeTime));
                dashDirection = isFacingLeft ? -1 : 1;
                impulse.GenerateImpulse();
            }

            if (dashExpireTime > Mathf.Epsilon && dashAlow)
            {
                dustParticles.Play();
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

                velocity.x = dashDirection * runSpeed * dashSpeed * Time.fixedDeltaTime;
                dashExpireTime -= Time.fixedDeltaTime;
                rb.velocity = velocity;
            }
            else if (dashExpireTime <= Mathf.Epsilon)
            {
                dashDirection = 0;
                if (GroundCheck())
                    currentState = PlayerState.Grounded;
                else if (WallHitCheck() != 0)
                {
                    currentState = PlayerState.WallSlide;
                }
                else
                    currentState = PlayerState.Fall;
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                dashRequest = false;
                dashExpireTime = 0f;
                dashAlow = false;
            }
        }
        else
        {
            currentState = PlayerState.Fall;
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
        }

    }

    private void JumpLogicProcessing()
    {

        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpCancel = false;
            jumpsCount++;
            jumpParticles.Play();
            Jump(velocity);

            jumpRequest = false;
        }
        else
        {
            jumpRequest = false;
        }

        if (jumpCancel && rb.velocity.y >= velocityTopSlice)
        {
            velocity.y = velocityTopSlice;
            rb.velocity = velocity;
            jumpCancel = false;
        }
    }

    private void WallJumpLogicProcessing(int wallDirX)
    {

        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpsCount++;
            velocity = new Vector2(wallJumpXVelocity * (-wallDirX), wallJumpForce);
            rb.velocity = velocity;
            jumpRequest = false;
        }
        else
            jumpRequest = false;
    }

    private void WallSlideLogicProcessing(int wallDirX, ParticleSystem.EmissionModule wsEmission)
    {
        WallsInteractionLogic();
        if (wallDirX == -1)
            wallslideParticles.transform.localPosition = new Vector2(-.14f, wallslideParticles.transform.localPosition.y);
        else if (wallDirX == 1)
            wallslideParticles.transform.localPosition = new Vector2(.14f, wallslideParticles.transform.localPosition.y);
        if ((wallDirX == -1 || wallDirX == 1) && Mathf.Sign(xInput) == wallDirX && xInput != 0)
        {
            wsEmission.enabled = true;
            if (rb.velocity.y <= 0)
            {
                if (wsTimeTRMV > 0)
                {
                    velocity.y = -initialWallSlidingVelocity * (wallSlideTimeToReachMaxVelocity / wsTimeTRMV);
                    if (velocity.y <= -maxlWallSlidingVelocity)
                        velocity.y = -maxlWallSlidingVelocity;

                    wsTimeTRMV -= Time.fixedDeltaTime;
                    rb.velocity = velocity;
                }
                else if (wsTimeTRMV <= 0 || jumpCancel)
                {
                    velocity.y = -maxlWallSlidingVelocity;
                    rb.velocity = velocity;
                }
            }

        }
        else
        {
            wsEmission.enabled = false;
            wsTimeTRMV = initialWallSlidingVelocity;
            currentState = PlayerState.Fall;
        }
    }

    private void WallJumpingHorizontalMovemetn()
    {

        rb.AddForce(new Vector2(xInput * runSpeed * 480 * Time.fixedDeltaTime, 0), ForceMode2D.Force);
        if (rb.velocity.x > wallJumpXVelocity)
        {
            velocity.x = 5f;
            rb.velocity = velocity;
        }
        else if (rb.velocity.x < -wallJumpXVelocity)
        {
            velocity.x = -5f;
            rb.velocity = velocity;
        }
        if (xInput == 0)
        {
            velocity.x = 0;
            rb.velocity = velocity;
        }
    }

    #endregion

    #region Basic Interactions
    private void HorizontalMovement(float xInput)
    {

        if (!wallJumping && controllsEnabled)
        {
            velocity.x = xInput * runSpeed * 100 * Time.deltaTime;
            rb.velocity = velocity;
        }

    }

    private void Jump(Vector2 vel)
    {
        vel.y = jumpMaxForce;
        rb.velocity = vel;
    }

    private void GravityScaleChange()
    {
        if (currentState == PlayerState.Fall)
        {
            rb.gravityScale = fallMultiplier;
            if (rb.velocity.y <= maxFallVelocity)
            {
                if (rb.velocity.y <= -10f)
                    reachedMaxFallVelocity = true;
                rb.velocity = new Vector2(rb.velocity.x, maxFallVelocity); 
            }
        }
        else if (currentState == PlayerState.WallSlide)
        {

        }
        else
        {
            rb.gravityScale = 1;
        }
    }

    private void FlipSprite()
    {
        isRunning = Mathf.Abs(rb.velocity.x) > .01f;
        if (currentState != PlayerState.WallSlide && currentState != PlayerState.WallBreak && prevFrameState != PlayerState.WallBreak && isRunning)
        {
            if (rb.velocity.x < .01f && xInput < .01f)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (rb.velocity.x > .01f && xInput > .01f)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
        else if (currentState == PlayerState.WallSlide && Mathf.RoundToInt(xInput) == wallDir)
        {
            if (wallDir == -1)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (wallDir == 1)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }

    }

    private bool GroundCheck()
    {
        bool isGrnd = false;

        foreach (var groundch in groundChecks)
        {
            if (RaycastCheck(groundch.position, Vector2.down, groundCheckLength, whatIsGround)) isGrnd = true;
        }

        return isGrnd && rb.velocity.y <= Mathf.Epsilon;
    }

    private int WallHitCheck()
    {
        int wallHitDir = 0;
        bool whLeft = false;
        bool whRight = false;

        foreach (var wallCheck in wallChecksLeft)
        {
            if (RaycastCheck(wallCheck.position, Vector2.left, wallCheckLength, whatIsWalls))
                whLeft = true;
        }
        foreach (var wallCheck in wallChecksRight)
        {
            if (RaycastCheck(wallCheck.position, Vector2.right, wallCheckLength, whatIsWalls))
                whRight = true;
        }
        if (!whLeft && !whRight) wallHitDir = 0;
        else if (whLeft && !whRight) wallHitDir = -1;
        else if (!whLeft && whRight) wallHitDir = 1;
        else wallHitDir = (int)Mathf.Sign(xInput);
        return wallHitDir;
    }

    private bool RaycastCheck(Vector2 pointOfRaycast, Vector2 direction, float length, LayerMask layer)
    {
        if (Physics2D.Raycast(pointOfRaycast, direction, length, layer).collider != null) return true;
        else return false;
    }

    private void GroundInteractionLogic()
    {

        dashAlow = true;

        if (GroundCheck() && prevFrameState != currentState)
        {
            jumpsCount = 0;
            jumpCancel = false;
            jumpParticles.Emit(8);
        }
    }

    private void WallsInteractionLogic()
    {
        jumpsCount = 0;
    }
    #endregion

    private void SetAnimationsParameters()
    {

        if (currentState == PlayerState.Dash && dashAlow)
            animator.Play("Dash");
        else if (currentState == PlayerState.WallBreak)
            animator.Play("Fall");
        else if (currentState == PlayerState.Grounded && Mathf.Abs(rb.velocity.x) <= Mathf.Epsilon)
        {
            if (timeInState <= landingTime)
            {
                if (timeInPrevState > .45f && reachedMaxFallVelocity)
                    animator.Play("Landing2");
                else
                    animator.Play("Landing");
            }
            else
            {
                reachedMaxFallVelocity = false;
                animator.Play("Idle");
            }
        }
        else if (currentState == PlayerState.Grounded && rb.velocity.x != 0)
        {
            if (timeInState <= landingTime)
            {
                if (timeInPrevState > .45f && reachedMaxFallVelocity)
                    animator.Play("Landing2");
                else
                    animator.Play("Landing");
            }
            else
            {
                reachedMaxFallVelocity = false;
                animator.Play("Walk");
            }
            //runEmission.enabled = true;
        }
        else if (currentState == PlayerState.Jump || currentState == PlayerState.WallJump || currentState == PlayerState.SpringJump)
        {
            if (rb.velocity.y > 0 && jumpsCount <= 1)
                animator.Play("Jump");
            else if (jumpsCount >= 2)
            {
                animator.Play("Jump2");
            }
        }
        else if (currentState == PlayerState.Fall)
            animator.Play("Fall");
        else if (currentState == PlayerState.WallSlide)
        {
            animator.Play("Climb");
        }

    }

    #region Triggers/Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("CheckPoint"))
        {
            GameController.Instance.SetChekpoint(collision.transform.position);
        }
        else if(collision.CompareTag("Hazards"))
        {
            StartCoroutine(GameController.Instance.DeathCoroutine());
            dashRequest = false;
            wsParticlesEmissionModule.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazards") && rb.velocity.y <= 0)
        {
            StartCoroutine(GameController.Instance.DeathCoroutine());
            dashRequest = false;
            wsParticlesEmissionModule.enabled = false;
        }
        /*else if (collision.collider.CompareTag("Destructibles") && currentState == PlayerState.Dash)
        {
            currentState = PlayerState.WallBreak;
        }*/
    }

    /*private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Destructibles") && currentState == PlayerState.Dash)
        {
            currentState = PlayerState.WallBreak;
        }
    }*/
    #endregion

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
                Gizmos.DrawRay(wallCH.position, new Vector2(wallCheckLength, 0) * Vector2.left);
            }
        }
        foreach (var wallCH in wallChecksRight)
        {
            if (wallCH != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(wallCH.position, new Vector2(wallCheckLength, 0) * Vector2.right);
            }
        }
    }
}



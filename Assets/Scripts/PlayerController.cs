﻿using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Homebrew;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState;
    private PlayerState prevState;
    [Foldout("Setup", true)]
    public GameController gc;
    public Rigidbody2D rb;
    public Animator animator;
    SpriteRenderer spriteRenderer;

    public GameObject wallslideParticles;
    private ParticleSystem.EmissionModule wsParticlesEmissionModule;

    public bool isDead = false;
    public bool controllsEnabled = true;
    public ParticleSystem deathParticles;
    [Space(8)]
    [Foldout("Run parameters", true)]
    [SerializeField] float runSpeed;
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
    public ParticleSystem jumpParticles;
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
    //public GameObject dashDenyIndicator;
    [SerializeField] float dashTime = .14f;
    [SerializeField] float dashFreezeTime = .02f;
    public bool dashAlow = true;
    public bool dashRequest = false;
    public bool isDashing = false;
    [HideInInspector] public float dashExpireTime;
    public CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem dashParticles;
    public ParticleSystem dustParticles;
    [Space(8)]

    [Foldout("Wall jump parameters", true)]
    int wallDir;
    [SerializeField] Transform[] wallChecksLeft;
    [SerializeField] Transform[] wallChecksRight;
    [SerializeField] LayerMask whatIsWalls;
    public bool wallJumped;
    public bool wallJumping;
    //public bool wallSliding = false;
    public float wallJumpXVelocity = 5f;

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
        wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
        wsParticlesEmissionModule.enabled = false;
        currentState = PlayerState.Grounded;
        //runEmission = runParticles.emission;


    }
    void Update()
    {
        if (controllsEnabled)
            PcControlls();

        //GroundInteractionLogic();

        FlipSprite();

        SetAnimationsParameters();

        xInput = SimpleInput.GetAxis("Horizontal");

        if (currentState == PlayerState.Dead)
        {
            dashRequest = false;
            jumpRequest = false;
            dashAlow = false;
        }
        if (currentState == PlayerState.Fall || currentState == PlayerState.Grounded|| currentState == PlayerState.WallSlide || currentState == PlayerState.SpringJump)
            GravityScaleChange();

    }



    private void FlipSprite()
    {
        isRunning = Mathf.Abs(rb.velocity.x) > .01f;
        if (currentState != PlayerState.WallSlide && isRunning)
        {
            if (rb.velocity.x < .01f)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (rb.velocity.x > .01f)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
        else if (currentState == PlayerState.WallSlide)
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

    private void GroundInteractionLogic()
    {

        dashAlow = true;

        //wallJumping = false;
        //if (dashDenyIndicator.activeSelf)
        //    dashDenyIndicator.SetActive(false);


        //bool OldIsGrounded = isGrounded;
        //isGrounded = GroundCheck();

        if (GroundCheck() && prevState != currentState)
        {
            jumpsCount = 0;
            jumpCancel = false;
            //springJumping = false;
            jumpParticles.Emit(8);
        }
    }
    private void WallsInteractionLogic()
    {
        jumpsCount = 0;
        jumpCancel = false;
        /*int OldWallDir = wallDirX;
        isWallHit = wallSliding;
        if (OldWallHit != isWallHit && isWallHit)
        {
            jumpsCount = 0;
            jumpCancel = false;
            springJumping = false;
        }
        */
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
    }

    private void FixedUpdate()
    {
        prevState = currentState;
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
                    Dash();
                }
                else if (rb.velocity.y < -Mathf.Epsilon)
                {
                    currentState = PlayerState.Fall;
                    //horizontal movement
                }
                break;

            case PlayerState.Jump:

                HorizontalMovement(xInput);
                JumpLogicProcessing();

                if (dashRequest)
                    currentState = PlayerState.Dash;
                else if (rb.velocity.y < 0f && jumpsCount < 2)
                    currentState = PlayerState.Fall;
                else if (rb.velocity.y < -3f && jumpsCount == 2)
                    currentState = PlayerState.Fall;
                else if (jumpRequest)
                    JumpLogicProcessing();
                else if (GroundCheck())
                {
                    currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }


                break;

            case PlayerState.Dash:
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
                else if (wallDir != 0 && rb.velocity.y <= 0 && Mathf.Sign(xInput) == wallDir && xInput != 0)
                {
                    WallsInteractionLogic();
                    currentState = PlayerState.WallSlide;
                }
                break;

            case PlayerState.WallSlide:
                var wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
                wallDir = WallHitCheck();
                HandleWallSliding(wallDir, wsParticlesEmissionModule);

                if (jumpRequest)
                {
                    wsParticlesEmissionModule.enabled = false;
                    currentState = PlayerState.WallJump;
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
                break;

            case PlayerState.WallJump:
                wallDir = WallHitCheck();
                WallJumpLogicProcessing(wallDir);
                WallJumpingHorizontalMovemetn();

                if (dashRequest)
                    currentState = PlayerState.Dash;
                else if (rb.velocity.y <= -.5f)
                    currentState = PlayerState.Fall;
                else if (jumpRequest)
                    currentState = PlayerState.Jump;
                else if (wallDir != 0 && rb.velocity.y <= 0)
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
                if (dashRequest)
                    currentState = PlayerState.Dash;
                else if (rb.velocity.y < -3f)
                {
                    currentState = PlayerState.Fall;
                }

                break;
            case PlayerState.Dead:
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                gc.DeathCoroutine();
                break;

            case PlayerState.Disabled:

                break;

        }
        if (prevState != currentState)
        {
            print(currentState);
        }

        /*if (currentState != State.Disabled)
        {
            HorizontalMovement(xInput);
            Dash();
            JumpLogicProcessing();
            HandleWallSliding(WallHitCheck());
            GravityScaleChange();
        }
        if (currentState == State.Dead)
        {
            
        }
        */
    }

    private void WallJumpingHorizontalMovemetn()
    {
        var velocity = rb.velocity;
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

    private void HandleWallSliding(int wallDirX, ParticleSystem.EmissionModule wsEmission)
    {
        WallsInteractionLogic();
        //wallSliding = false;
        if (wallDirX == -1)
            wallslideParticles.transform.localPosition = new Vector2(-.14f, wallslideParticles.transform.localPosition.y);
        else if (wallDirX == 1)
            wallslideParticles.transform.localPosition = new Vector2(.14f, wallslideParticles.transform.localPosition.y);
        if ((wallDirX == -1 || wallDirX == 1) && Mathf.Sign(xInput) == wallDirX && xInput != 0 && rb.velocity.y <= 0)
        {
            wsEmission.enabled = true;
            if (rb.velocity.y < -initialWallSlidingVelocity && rb.velocity.y < 0)
            {
                var velocity = rb.velocity;
                velocity.y = -maxlWallSlidingVelocity;
                rb.velocity = velocity;
            }
        }
        else
        {
            wsEmission.enabled = false;
            currentState = PlayerState.Fall;
        }
        //hack
        /*if (currentState == State.WallSlide)
            wsEmission.enabled = true;
        else
            wsEmission.enabled = false;*/
    }

    void HorizontalMovement(float xInput)
    {
        Vector2 velocity = rb.velocity;
        if (!wallJumping && controllsEnabled)
        {
            velocity.x = xInput * runSpeed * 100 * Time.deltaTime;
            rb.velocity = velocity;
        }

    }

    private void SetAnimationsParameters()
    {

        if (currentState == PlayerState.Dash && dashAlow)
            animator.Play("Dash");
        else if (currentState == PlayerState.Grounded && Mathf.Abs(rb.velocity.x) <= Mathf.Epsilon)
        {
            animator.Play("Idle");
        }
        else if (currentState == PlayerState.Grounded && rb.velocity.x != 0)
        {
            animator.Play("Walk");
            //runEmission.enabled = true;
        }
        else if (currentState == PlayerState.Jump || currentState == PlayerState.WallJump || currentState == PlayerState.SpringJump)
        {
            //runEmission.enabled = false;
            if (rb.velocity.y > 0 && jumpsCount <= 1)
                animator.Play("Jump");
            else if (jumpsCount >= 2)
            {
                animator.Play("Jump2");
            }
        }
        else if (currentState == PlayerState.Fall)
            animator.Play("Fall");
        else if (currentState == PlayerState.WallSlide) //!wallJumping && !isGrounded && wallSliding && rb.velocity.y <= 0)
        {
            animator.Play("Climb");
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


    public void Dash()
    {
        if (dashRequest && !dashAlow)
            dashRequest = false;

        else if (dashRequest)
        {
            dashParticles.Play();
            if (dashExpireTime > Mathf.Epsilon && dashAlow)
            {
                dustParticles.Play();
                int dashDirection = isFacingLeft ? -1 : 1;
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                //isDashing = true;
                Vector2 velocity = rb.velocity;
                if (xInput == 0)
                    velocity.x = dashDirection * runSpeed * 300 * Time.fixedDeltaTime;
                else
                    velocity.x = Mathf.Sign(xInput) * runSpeed * 300 * Time.fixedDeltaTime;

                dashExpireTime -= Time.fixedDeltaTime;
                rb.velocity = velocity;
                impulse.GenerateImpulse(new Vector3(0, dashDirection, 0));

                if (jumpRequest)
                {
                    rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                    rb.velocity = new Vector2(xInput * runSpeed * 300 * Time.deltaTime, jumpMaxForce * 2);
                }

            }
            else if (dashExpireTime <= Mathf.Epsilon)
            {
                currentState = PlayerState.Fall;
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;

                //isDashing = false;
                dashRequest = false;
                dashExpireTime = 0f;
                dashAlow = false;
            }
        }
        else
        {
            currentState = PlayerState.Fall;
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            //isDashing = false;
        }


    }

    public void OnPressJump()
    {
        jumpRequest = true;
        /*if (wallSliding && jumpRequest && !isGrounded && Mathf.Sign(xInput) == wallDirX)
        {
            wallJumped = true;
        }*/
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

        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpCancel = false;
            jumpsCount++;
            /*if (wallJumped)
            {
                rb.velocity = new Vector2(wallJumpXVelocity * (-wallDirX), wallJumpForce);
                wallJumping = true;
                wallJumped = false;
            }
            else
            {*/
            jumpParticles.Play();
            Jump(velocity);

            jumpRequest = false;
        }
        else
        {
            jumpRequest = false;
        }

        if (jumpCancel && rb.velocity.y >= velocityTopSlice) // && !springJumping) //todo сделать красиво (хотя и так работает)
        {
            velocity.y = velocityTopSlice;
            rb.velocity = velocity;
            jumpCancel = false;
        }
        /*if (rb.velocity.y < 0 || Mathf.Sign(xInput) == -wallDirX || xInput == 0)
            wallJumping = false;*/

    }

    private void WallJumpLogicProcessing(int wallDirX)
    {
        Vector2 velocity = rb.velocity;
        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpCancel = false;
            jumpsCount++;
            rb.velocity = new Vector2(wallJumpXVelocity * (-wallDirX), wallJumpForce);
            jumpRequest = false;
        }
        else
            jumpRequest = false;

        if (jumpCancel && rb.velocity.y >= velocityTopSlice) //&& !springJumping) //todo сделать красиво (хотя и так работает)
        {
            velocity.y = velocityTopSlice;
            rb.velocity = velocity;
            jumpCancel = false;
        }
    }

    private void Jump( Vector2 vel)
    {
        vel.y = jumpMaxForce;
        rb.velocity = vel;
    }
    private void GravityScaleChange()
    {
        if (currentState == PlayerState.Fall )  
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
    private bool GroundCheck()
    {
        bool isGrnd = false;

        foreach (var groundch in groundChecks)
        {
            if (RaycastCheck(groundch.position, Vector2.down, groundCheckLength, whatIsGround)) isGrnd = true;
        }

        return isGrnd&&rb.velocity.y <= Mathf.Epsilon;
    }

    private int WallHitCheck()
    {
        int wallHitDir = 0;
        bool whLeft = false;
        bool whRight = false;
        foreach (var wallCheck in wallChecksLeft)
        {
            whLeft = RaycastCheck(wallCheck.position, Vector2.left, wallCheckLength, whatIsWalls);
        }
        foreach (var wallCheck in wallChecksRight)
        {
            whRight = RaycastCheck(wallCheck.position, Vector2.right, wallCheckLength, whatIsWalls);
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
            //isDashing = false;
            //wallSliding = false;
            wsParticlesEmissionModule.enabled = false;
            //isDead = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazards") && rb.velocity.y <= 0)
        {
            StartCoroutine(gc.DeathCoroutine());
            dashRequest = false;
            //isDashing = false;
            //wallSliding = false;
            wsParticlesEmissionModule.enabled = false;
            //isDead = true;
        }



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


public enum PlayerState
{
    Grounded,
    Jump,
    Dash,
    WallSlide,
    WallJump,
    Fall,
    Dead,
    Disabled,

    SpringJump

}
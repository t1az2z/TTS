using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;
    Animator animator;

    [Header("Run parameters")]
    [SerializeField] float runAcceleration;
    bool isFacingLeft;
    bool isRunning = false;
    float xInput;
    public float maxSpeed = 10f;
    
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
    [SerializeField] float fallGravity = 2.7f;
    [Space(8)]

    [Header("Ground parameters")]
    [SerializeField] Transform[] groundCheck;
    const float groundedRadius = 0.03125f;
    [SerializeField] LayerMask whatIsGround;
    [Space(8)]

    [Header("Walljump parameters")]
    [SerializeField] Transform[] wallCheck;
    const float wallRadius = 0.03125f;
    [SerializeField] LayerMask WhatIsWalls;
    bool wallHit = false;
    bool wallStack = false;
    [SerializeField] float wallStackTime = .2f;
    [SerializeField] float wallJumpXVelocity = 3f;

    [Space(8)]
    [Header("Spin parameters:")]
    [SerializeField] float spinTime = 2f;
    public bool spinAllow = true;
    public bool spinRequest = false;
    public bool isSpinning = false;
    public float spinExpireTime;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        PcControlls();

        RunInputProcessing();

        VariablesResetOnGround();
        SetAnimationsParameters();


    }

    private void VariablesResetOnGround()
    {
        if (isGrounded)
        {
            spinAllow = true;
        }

        bool OldIsGrounded = isGrounded;
        isGrounded = GroundCheck();
        if (OldIsGrounded != isGrounded && isGrounded)
        {
            jumpsCount = 0;
            jumpCancel = false;
        }
    }

    private void ResetJumpsCountOnWallEnter()
    {

        bool oldWallHit = wallHit;
        wallHit = WallCheck();
        if (oldWallHit != wallHit && wallHit)
        {
            jumpsCount = 0;
            jumpCancel = false;
            wallStack = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;

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

        if (SimpleInput.GetButtonDown("Spin"))
        {
            OnPressSpin();
        }
        else if (SimpleInput.GetButtonUp("Spin"))
        {
            OnReleaseSpin();
        }
    }

    private void FixedUpdate()
    {
        HorizontalMovementProcessing();
        Spin();

        JumpLogicProcessing();
        GravityScaleChange();

        if (wallHit && !isGrounded && rb.velocity.y <0)
            wallStack = true;
        else
            wallStack = false;

    }




    void RunInputProcessing()
    {
        xInput = SimpleInput.GetAxis("Horizontal");
        isRunning = Mathf.Abs(xInput) > Mathf.Epsilon;
    }

    void HorizontalMovementProcessing()
    {
        if (!wallStack)
        {
            GroundedOrAirborneMovement();
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            float currentWallStackTime = wallStackTime;
            if (Mathf.Sign(xInput) != Mathf.Sign(transform.localScale.x) && Mathf.Abs(xInput) > 0)
            {
                print("trying to unstack");

                currentWallStackTime -= Time.fixedDeltaTime;
                print(currentWallStackTime);
                if (currentWallStackTime <= 0)
                {
                    rb.AddForce(new Vector2(Mathf.Sign(-transform.localScale.x)*runAcceleration, 0f), ForceMode2D.Force);
                    print("unstacking");
                    rb.gravityScale = fallGravity;
                }
            }

            //place animation trigger
            //wall stick time
            
            //if pressing oposite from wall => wallsticktime -= time.deltatime
            //if not
            //slowly decrease y speed(or increase gravity?) to fall speed
            //if jump pressed
            //velocity = jumpforce, wallJumpXSpeed

        }
    }

    private void GroundedOrAirborneMovement()
    {
        if (xInput < -.5)
        {
            if (rb.velocity.x > -maxSpeed)
            {
                rb.AddForce(new Vector2(-runAcceleration, 0f), ForceMode2D.Force);
            }
            else
            {
                rb.velocity = new Vector2(-maxSpeed * 100 * Time.fixedDeltaTime, rb.velocity.y);
            }
        }
        else if (xInput > .5)
        {
            if (rb.velocity.x < maxSpeed)
            {
                rb.AddForce(new Vector2(runAcceleration, 0f), ForceMode2D.Force);
            }
            else
            {
                rb.velocity = new Vector2(maxSpeed * 100 * Time.fixedDeltaTime, rb.velocity.y);
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }




    public void OnPressSpin()
    {
        spinRequest = true;
        spinExpireTime = Time.time + spinTime;
    }
    public void OnReleaseSpin()
    {
        spinRequest = false;
        spinExpireTime = 0f;
        spinAllow = false;
    }
    public void Spin()
    {
        if (!isGrounded)
        {
            if (spinRequest && spinExpireTime >= Time.time && spinAllow)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                isSpinning = true;
                Vector2 velocity = rb.velocity;
                velocity.x = 4*transform.localScale.x * maxSpeed;
                rb.velocity = velocity;
            }
            else if (!spinRequest || spinExpireTime < Time.time)
            {
                rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                isSpinning = false;
            }
        }
    }

    public void OnPressJump()
    {
        jumpRequest = true;
    }

    public void OnReleaseJump()
    {
        jumpRequest = false;
        if (!isGrounded)
            jumpCancel = true;
    }

    private void JumpLogicProcessing()
    {
        Vector2 velocity = rb.velocity;
        ResetJumpsCountOnWallEnter();

        if (jumpRequest && (jumpsCount < allowedJumps))
        {
            jumpCancel = false;
            jumpsCount++;
            if (wallStack)
            {
                WallJump(velocity);
            }
            else
            Jump(velocity);

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

    }

    private void Jump( Vector2 vel)
    {
        vel.y = jumpMaxForce;
        rb.velocity = vel;
    }

    private void WallJump(Vector2 vel)
    {
        //if (Mathf.Sign(xInput) == Mathf.Sign(transform.localScale.x))
        rb.AddForce(new Vector2(-transform.localScale.x * wallJumpXVelocity, jumpMaxForce), ForceMode2D.Impulse);
        //rb.velocity = new Vector2(wallJumpXVelocity * Mathf.Sign(-transform.localScale.x), jumpMaxForce);
    }

    private void GravityScaleChange()
    {
        if (rb.velocity.y < 0)
        {

            rb.gravityScale = fallGravity;
        }
        else
        {
            rb.gravityScale = 1;
        }
        
    }
    private bool GroundCheck()
    {
        bool isGrnd = false;
        foreach (var groundch in groundCheck)
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

    private bool WallCheck()
    {
        bool wHit = false;
        foreach (var wallCh in wallCheck)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCh.position, groundedRadius, whatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject && !isGrounded)
                {
                    wHit = true;
                }
            }
        }
        return wHit;
    }

    private void SetAnimationsParameters()
    {
        /*animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isSpinning", isSpinning);
        animator.SetBool("isFacingLeft", isFacingLeft);
        animator.SetFloat("xSpeed", xMovement);
        animator.SetBool("isRunning", isRunning);*/



        //todo refactoring
        //fliping object. it is also used for wallJumps. so need to be redone 
        isRunning = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (isRunning)
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
    }
    private void OnDrawGizmosSelected()
    {
        foreach (var groundch in groundCheck)
        {
            if (groundch != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundch.position, groundedRadius);
            }
        }

        foreach (var wallch in wallCheck)
        {
            if (wallch != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(wallch.position, groundedRadius);
            }
        }
    }
}
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;
    Animator animator;

    [Header("Run parameters")]
    [SerializeField] float runSpeed;
    bool isFacingLeft;
    bool isRunning = false;
    float xMovement;
    [Space(8)]

    [Header("Jump parameters:")]
    [SerializeField] float jumpMaxForce = 5.25f;
    [SerializeField] float jumpMinForce = 3f;
    public bool isGrounded;
    bool jumpRequest = false;
    public bool jumpCancel = false;
    [SerializeField] int allowedJumps = 2;
    public int jumpsCount = 1;
    /*float jumpHoldingTime = 0f;
    float jumpMaxHoldingTime = .5f;*/
    [Space(8)]

    [Header("Jump gravity variables:")]
    [SerializeField] float fallMultiplier = 2.7f;
    [SerializeField] float lowJumpMultiplier = 2f;
    [Space(8)]

    [Header("Ground parameters")]
    [SerializeField] Transform groundCheck;
    const float groundedRadius = .2f;
    [SerializeField] LayerMask whatIsGround;
    [Space(8)]

    [Header("Spin parameters:")]
    [SerializeField] float spinTime = 2f;
    bool spinAllow = true;
    bool spinRequest = false;
    bool isSpinning = false;
    float spinExpireTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        PcControlls();

        RunInputProcessing();

        VariablesResetOnGround();
        //SetAnimationsParameters();
    }

    private void VariablesResetOnGround()
    {
        if (isGrounded)
        {
            jumpsCount = 1;
            spinAllow = true;
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

        if (SimpleInput.GetButtonDown("Spin"))
        {
            spinRequest = true;
            spinExpireTime = Time.time + spinTime;
        }
        else if (SimpleInput.GetButtonUp("Spin"))
        {
            spinRequest = false;
            spinExpireTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        Run();
        Spin();
        GroundCheck();
        JumpLogicProcessing();
        GravityScaleChange();

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.SetParent(collision.transform);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        transform.SetParent(null);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        transform.SetParent(collision.transform);
    }


    void RunInputProcessing()
    {
        xMovement = SimpleInput.GetAxis("Horizontal");
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
        velocity.x = xMovement * runSpeed *100  * Time.deltaTime;
        rb.velocity = velocity;
    }

    private void SetAnimationsParameters()
    {
        animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isSpinning", isSpinning);
        animator.SetBool("isFacingLeft", isFacingLeft);
        animator.SetFloat("xSpeed", xMovement);
        animator.SetBool("isRunning", isRunning);
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
    }
    public void Spin()
    {
        if (spinRequest && spinExpireTime >= Time.time)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            isSpinning = true;
        }
        else if (!spinRequest || spinExpireTime < Time.time)
        {
            rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            isSpinning = false;
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
        MultipleJumpProcessing();
    }

    private void MultipleJumpProcessing()
    {
        Vector2 velocity = rb.velocity;

        if (jumpRequest && (jumpsCount <= allowedJumps))
        {
            jumpsCount++;
            Jump(velocity);
            jumpRequest = false;
        }
        else
        {
            jumpRequest = false;
        }

        if (jumpCancel && rb.velocity.y >=0) //todo сделать красиво (хотя и так работает)
        {
            velocity.y = 0;
            rb.velocity = velocity;
            jumpCancel = false;
            print(velocity.y);

        }
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
            }
        }
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundedRadius);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;
using System;

[RequireComponent(typeof(CharacterController2D))]
public class NewPlayerController : MonoBehaviour
{

    #region Properties
    //generals
    SpriteRenderer spriteRenderer;
    public Animator animator;
    private CharacterController2D controller;
    public Vector3 velocity;
    private RaycastHit2D _lastControllerColliderHit;
    public bool controllsEnabled = true;
    //gravity
    public float gravity = -25f;
    private float currentGravity;
    public float fallGravityMultiplier = 1.2f;

    //states
    public PlayerState currentState;
    private PlayerState prevFrameState;
    private PlayerState prevState;

    //movement
    float xInput;
    bool isRunning;
    bool isFacingLeft;
    public float runSpeed = 3f;
    public float groundDamping = 50f;

    //jumps
    public float jumpMaxSpeed = 9.3f;
    public bool jumpRequest;
    public bool jumpCancel;
    public float inAirDumping = 50;
    public int allowedJumps = 2;
    public int jumpsCount = 0;
    private const float velocityTopSlice = 3f;
    [SerializeField] float landingTime = .21f;

    #endregion

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentGravity = gravity;
        //events listening
        controller.onControllerCollidedEvent += onControllerCollider;
        controller.onTriggerEnterEvent += onTriggerEnterEvent;
        controller.onTriggerStayEvent += onTriggerStayEvent;
        controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    private void Update()
    {
        //debugging
        print(controller.isGrounded);
        if (controllsEnabled)
            PcControlls();
        xInput = SimpleInput.GetAxisRaw("Horizontal");



        if(prevState != prevFrameState && prevFrameState != currentState)
            prevState = currentState;
        prevFrameState = currentState;
        switch (currentState)
        {
            case PlayerState.Grounded:
                //velocity.y = 0;
                GroundInteractionLogic();
                HorizontalMovement(xInput);
                if (jumpRequest)
                    currentState = PlayerState.Jump;
                break;

            case PlayerState.Jump:
                HorizontalMovement(xInput);
                JumpLogicProcessing();
                if (velocity.y < 0f && jumpsCount < 2)
                    currentState = PlayerState.Fall;
                else if (velocity.y < -3f && jumpsCount == 2)
                    currentState = PlayerState.Fall;
                else if (controller.isGrounded)
                    currentState = PlayerState.Grounded;
                break;

            case PlayerState.Fall:
                HorizontalMovement(xInput);
                if (controller.isGrounded)
                {
                    currentState = PlayerState.Jump;
                    JumpLogicProcessing();
                }
                else if(jumpRequest)
                    currentState = PlayerState.Grounded;
                break;
        }
        GravityScaleChange();
        velocity.y += currentGravity * Time.deltaTime;
        FlipSprite();
        controller.move(velocity * Time.deltaTime);

        velocity = controller.velocity;
    }

    private void GroundInteractionLogic()
    {

        //dashAlow = true;

        if (controller.isGrounded) // && prevFrameState != currentState)
        {
            jumpsCount = 0;
            jumpCancel = false;
            //jumpParticles.Emit(8);
        }
    }

    private void JumpLogicProcessing()
    {
        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpCancel = false;
            jumpsCount++;
            //jumpParticles.Play();
            velocity.y = jumpMaxSpeed;

            jumpRequest = false;
        }
        else
        {
            jumpRequest = false;
        }

        if (jumpCancel && velocity.y >= velocityTopSlice)
        {
            velocity.y = velocityTopSlice;
            jumpCancel = false;
        }
    }


    void GravityScaleChange()
    {
        if (currentState == PlayerState.Fall)
            currentGravity = gravity * fallGravityMultiplier;
        else if (currentState == PlayerState.Grounded)
            currentGravity = gravity;
    }
    void FlipSprite()
    {
        isRunning = Mathf.Abs(velocity.x) > .01f;
        if (currentState != PlayerState.WallSlide && currentState != PlayerState.WallBreak && prevFrameState != PlayerState.WallBreak && isRunning)
        {
            if (velocity.x < .01f && xInput < .01f)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (velocity.x > .01f && xInput > .01f)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
    }
    void HorizontalMovement(float input)
    {
        if (controllsEnabled)
        {
            var smoothMovementFactor = controller.isGrounded ? groundDamping : inAirDumping;
            velocity.x = Mathf.Lerp(velocity.x, input * runSpeed, Time.deltaTime * smoothMovementFactor);
        }
    }


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

        /*if (SimpleInput.GetButtonDown("Dash") && dashEnabled)
        {
            OnPressDash();
        }*/
    }

    public void OnPressJump()
    {
        jumpRequest = true;
        jumpCancel = false;
    }

    public void OnReleaseJump()
    {
        jumpRequest = false;
        if (!controller.isGrounded) //&& !springJumping)
            jumpCancel = true;
    }

    /*public void OnPressDash()
    {
        if (!dashRequest)
        {
            dashRequest = true;
            dashExpireTime = dashTime;
            if (dashDirection == 0)
                dashDirection = isFacingLeft ? -1 : 1;
        }
    }*/

    #endregion

    #region Events Listeners
    void onControllerCollider(RaycastHit2D hit)
    {
        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void onTriggerEnterEvent(Collider2D col)
    {
        //Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }

    void onTriggerStayEvent(Collider2D col)
    {
        //Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }

    void onTriggerExitEvent(Collider2D col)
    {
        //Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }
    #endregion
}
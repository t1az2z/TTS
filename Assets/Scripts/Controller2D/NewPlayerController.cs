using UnityEngine;
using Cinemachine;
using Prime31;
using Homebrew;

[RequireComponent(typeof(CharacterController2D))]
public class NewPlayerController : MonoBehaviour
{

    #region Properties
    //generals
    SpriteRenderer spriteRenderer;
    public Animator animator;
    private CharacterController2D controller;
    public Vector3 velocity;
    //private RaycastHit2D _lastControllerColliderHit;
    public bool controllsEnabled = true;
    public bool gravityActive = true;
    //gravity
    public float gravity = -25f;
    private float currentGravity;
    public float fallGravityMultiplier = 1.2f;
    bool isGrounded;
    public float maxFallVelocity = -10f;
    bool reachedMaxFallVelocity = false;
    //states
    public PlayerState _currentState;
    private PlayerState _prevFrameState;
    private PlayerState _prevState;
    private float timeInState = 0f;
    private float timeInPrevState;

    //movement
    public int xInput;
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
    public ParticleSystem jumpParticles;


    [Space(8)]

    [Foldout("Dash parameters", true)]
    public bool dashEnabled = false;
    int dashDirection = 0;
    [SerializeField] float dashTime = .2f;
    [SerializeField] float dashFreezeTime = .06f;
    [SerializeField] float dashSpeed = 250;
    public bool dashAlow = true;
    public bool dashRequest = false;
    [SerializeField] Vector2 destroyWallsKnockback = new Vector2(2, 2);
    [HideInInspector] public float dashExpireTime;
    public CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem dashParticles;
    public ParticleSystem dustParticles;
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
        if (controllsEnabled)
            PcControlls();
        xInput = (int)SimpleInput.GetAxisRaw("Horizontal");



        if(_prevState != _prevFrameState && _prevFrameState != _currentState)
            _prevState = _currentState;
        _prevFrameState = _currentState;

        switch (_currentState)
        {
            case PlayerState.Grounded:
                GroundInteractionLogic();
                HorizontalMovement(xInput);
                if (dashRequest)
                    _currentState = PlayerState.Dash;
                else if (jumpRequest)
                    _currentState = PlayerState.Jump;
                else if (velocity.y < -.01f)
                    _currentState = PlayerState.Fall;
                    break;
                

            case PlayerState.Jump:
                HorizontalMovement(xInput);
                JumpLogicProcessing();
                if (dashRequest)
                    _currentState = PlayerState.Dash;
                else if (velocity.y < 0f && jumpsCount < 2)
                    _currentState = PlayerState.Fall;
                else if (velocity.y < -3f && jumpsCount == 2)
                    _currentState = PlayerState.Fall;
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                    break;

            case PlayerState.Dash:
                reachedMaxFallVelocity = false;
                Dash();
                break;

            case PlayerState.Fall:
                HorizontalMovement(xInput);
                if (dashRequest)
                    _currentState = PlayerState.Dash;
                else if (jumpRequest)
                {
                    _currentState = PlayerState.Jump;
                    JumpLogicProcessing();
                }
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                break;
        }
        CountTimeInState();
        GravityScaleChange();
        if (gravityActive)
            velocity.y += currentGravity * Time.deltaTime;
        FlipSprite();
        controller.move(velocity * Time.deltaTime);

        velocity = controller.velocity;

        SetAnimations();
    }


    public void Dash()
    {
        if (dashRequest && !dashAlow)
            dashRequest = false;

        else if (dashRequest)
        {
            dashParticles.Play();
            gravityActive = false;

            if (dashExpireTime == dashTime)
            {
                //StartCoroutine(GameController.Instance.FreezeTime(dashFreezeTime));
                dashDirection = isFacingLeft ? -1 : 1;
                //impulse.GenerateImpulse();
            }

            if (dashExpireTime > Mathf.Epsilon && dashAlow)
            {
                dustParticles.Play();
                velocity.y = 0;

                velocity.x = dashDirection * dashSpeed;
                dashExpireTime -= Time.deltaTime;
            }
            else if (dashExpireTime <= Mathf.Epsilon)
            {
                dashDirection = 0;
                gravityActive = true;
                if (GroundCheck())
                    _currentState = PlayerState.Grounded;
                //else if (WallHitCheck() != 0)
                //{
                //    currentState = PlayerState.WallSlide;
                //}
                else
                    _currentState = PlayerState.Fall;
                //currentGravity = gravity;
                dashRequest = false;
                dashExpireTime = 0f;
                dashAlow = false;
            }
        }
        else
        {
            _currentState = PlayerState.Fall;
            gravityActive = true;
            currentGravity = gravity;
        }

    }
    private void CountTimeInState()
    {
        if (_prevFrameState != _currentState)
        {
            if (_currentState != PlayerState.Dead)
            {
                timeInPrevState = timeInState;
                timeInState = 0;
            }
        }
        else
            timeInState += Time.fixedDeltaTime;
    }

    private void SetAnimations()
    {

        if (_currentState == PlayerState.Dash && dashAlow)
            animator.Play("Dash");
        else if (_currentState == PlayerState.WallBreak)
            animator.Play("Fall");
        else if (_currentState == PlayerState.Grounded && Mathf.Abs(velocity.x) <= 0 && _prevState != PlayerState.Dash)
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
        else if (_currentState == PlayerState.Grounded && velocity.x != 0 && xInput != 0 && _prevState != PlayerState.Dash)
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
        else if (_currentState == PlayerState.Jump || _currentState == PlayerState.WallJump || _currentState == PlayerState.SpringJump)
        {
            if (velocity.y > 0 && jumpsCount <= 1)
                animator.Play("Jump");
            else if (jumpsCount >= 2)
            {
                animator.Play("Jump2");
            }
        }
        else if (_currentState == PlayerState.Fall)
            animator.Play("Fall");
        else if (_currentState == PlayerState.WallSlide)
        {
            animator.Play("Climb");
        }
        else
            animator.Play("Idle");

    }

    private void GroundInteractionLogic()
    {
        dashAlow = true;

        if (GroundCheck() && _prevFrameState != _currentState)
        {
            jumpsCount = 0;
            jumpCancel = false;
            jumpParticles.Play();
        }
    }

    private void JumpLogicProcessing()
    {
        if (jumpsCount < allowedJumps && jumpRequest)
        {
            jumpCancel = false;
            jumpsCount++;
            jumpParticles.Play();
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

    private bool GroundCheck()
    {
        return controller.isGrounded && velocity.y <= 0;
    }

    void GravityScaleChange()
    {
        if (_currentState == PlayerState.Fall)
        {
            currentGravity = gravity * fallGravityMultiplier;
            if (velocity.y <= maxFallVelocity)
            {
                if (velocity.y <= -10f)
                    reachedMaxFallVelocity = true;
                velocity = new Vector2(velocity.x, maxFallVelocity);
            }
        }
        else
            currentGravity = gravity;
    }
    void FlipSprite()
    {
        isRunning = Mathf.Abs(velocity.x) > .01f;
        if (_currentState != PlayerState.WallSlide && _currentState != PlayerState.WallBreak && _prevFrameState != PlayerState.WallBreak && isRunning)
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
        if (!controller.isGrounded) //&& !springJumping)
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
    WallBreak,
    SpringJump,
}
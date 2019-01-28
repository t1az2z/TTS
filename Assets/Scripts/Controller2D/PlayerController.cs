using UnityEngine;
using Cinemachine;
using Prime31;
using Homebrew;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerController : MonoBehaviour
{

    #region Properties
    
    //generals
    SpriteRenderer spriteRenderer;
    public Animator animator;
    private CharacterController2D controller;
    public Vector3 velocity;
    public float controllsDisabledTimer = .2f;
    //private RaycastHit2D _lastControllerColliderHit;
    public bool controllsEnabled = true;
    public bool gravityActive = true;
    //gravity
    public float gravity = -25f;
    public float currentGravity;
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
    public ParticleSystem deathParticles;

    //movement
    public int xInput;
    bool isRunning;
    public bool isFacingLeft;
    public float runSpeed = 3f;
    public float groundDamping = 50f;

    //jumps
    public int jumpCost = 1;
    public float jumpMaxSpeed = 9.3f;
    public bool jumpRequest;
    public bool jumpCancel;
    public float inAirDumping = 50;
    public int batteryCapacity = 2;
    public int batterySpent = 0;
    private const float velocityTopSlice = 3f;
    [SerializeField] float landingTime = .21f;
    public ParticleSystem jumpParticles;


    [Space(8)]

    [Foldout("Dash parameters", true)]
    public bool isDashing = false;
    public int dashCost = 1;
    public bool dashEnabled = false;
    int dashDirection = 0;
    [SerializeField] float dashTime = .2f;
    [SerializeField] float dashFreezeTime = .06f;
    [SerializeField] float dashSpeed = 250;
    //public bool dashAlow = true;
    public bool dashRequest = false;
    [SerializeField] Vector2 destroyWallsKnockback = new Vector2(2, 2);
    [HideInInspector] public float dashExpireTime;
    public CinemachineImpulseSource impulse;
    [SerializeField] ParticleSystem dashParticles;
    public ParticleSystem dustParticles;
    #endregion

    [Foldout("Wall jump/slide parameters", true)]
    int wallDirX;
    bool wallHit;
    public bool wallJumped;
    public bool wallJumping;
    public float wallJumpDumping = 9f;
    [SerializeField] float wallJumpYVelocity = 9.76f;
    [SerializeField] float wallJumpXVelocity = 5f;
    [SerializeField] float initialWallSlidingVelocity = 1.5f;
    [SerializeField] float maxlWallSlidingVelocity = 3f;
    [SerializeField] float wallSlideTimeToReachMaxVelocity = 1f;
    private float wsTimeTRMV;
    public GameObject wallslideParticles;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();

        currentGravity = gravity;
        //events listening
        controller.onControllerCollidedEvent += onControllerCollider;
        controller.onTriggerEnterEvent += onTriggerEnterEvent;
        controller.onTriggerStayEvent += onTriggerStayEvent;
        controller.onTriggerExitEvent += onTriggerExitEvent;
    }
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        impulse = GetComponent<CinemachineImpulseSource>();
        _currentState = PlayerState.Grounded;
        wsTimeTRMV = wallSlideTimeToReachMaxVelocity;
    }

    private void Update()
    {
        wallDirX = controller.collisionState.left ? -1 : 1;
        wallHit = controller.collisionState.left || controller.collisionState.right;
        //print(wallHit + "  " + velocity.x);
        if (controllsEnabled)
            PcControlls();
        xInput = (int)SimpleInput.GetAxisRaw("Horizontal");

        if (_currentState == PlayerState.Dead)
        {
            dashRequest = false;
            jumpRequest = false;
            //dashAlow = false;
        }
        if (_currentState == PlayerState.Fall || _currentState == PlayerState.Grounded || _currentState == PlayerState.WallSlide || _currentState == PlayerState.SpringJump)
            GravityScaleChange();
        if (_currentState != PlayerState.WallSlide)
            wsTimeTRMV = wallSlideTimeToReachMaxVelocity;

        if (_prevState != _prevFrameState && _prevFrameState != _currentState)
            _prevState = _currentState;
        _prevFrameState = _currentState;

        switch (_currentState)
        {
            case PlayerState.Grounded:
                if (controller.collisionState.becameGroundedThisFrame)
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
                else if (velocity.y < 0f && batterySpent < 2)
                    _currentState = PlayerState.Fall;
                else if (velocity.y < -3f && batterySpent == 2)
                    _currentState = PlayerState.Fall;
                else if (wallHit)
                    _currentState = PlayerState.WallSlide;
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                    break;

            case PlayerState.Dash:
                reachedMaxFallVelocity = false;
                if (dashRequest && batterySpent >= batteryCapacity)
                {
                    dashRequest = false;
                }

                else if (dashRequest && batterySpent < batteryCapacity)
                {
                    dashRequest = false;
                    isDashing = true;
                }
                

                if (isDashing && !dashRequest)
                    Dash();
                else if (!isDashing)
                {
                    _currentState = PlayerState.Fall;
                    gravityActive = true;
                    currentGravity = gravity;
                }
                
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
                else if (wallHit && xInput == wallDirX)
                    _currentState = PlayerState.WallSlide;
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                break;

            case PlayerState.WallSlide:
                var wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
                reachedMaxFallVelocity = false;
                WallSlideLogicProcessing(wallDirX, wsParticlesEmissionModule);

                if (jumpRequest)
                {
                    wsParticlesEmissionModule.enabled = false;
                    if (Mathf.Sign(xInput) == wallDirX)
                        _currentState = PlayerState.WallJump;
                    else
                        _currentState = PlayerState.Jump;
                }
                else if (dashRequest)
                {
                    wsParticlesEmissionModule.enabled = false;
                    _currentState = PlayerState.Dash;
                }
                else if (GroundCheck())
                {
                    wsParticlesEmissionModule.enabled = false;
                    _currentState = PlayerState.Grounded;
                }
                else if (!wallHit && velocity.y > 0)
                {
                    _currentState = PlayerState.Jump;
                    batterySpent = 1;
                }
                break;

            case PlayerState.WallJump:
                HorizontalMovement(xInput);
                if (batterySpent < 1 && jumpRequest)
                    WallJumpLogicProcessing(wallDirX);
                else if (batterySpent >= 1 && jumpRequest)
                    _currentState = PlayerState.Jump;
                if (jumpCancel && velocity.y >= velocityTopSlice)
                {

                    velocity.y = velocityTopSlice;
                    jumpCancel = false;
                }

                if (dashRequest)
                    _currentState = PlayerState.Dash;

                else if (velocity.y <= -.5f)
                    _currentState = PlayerState.Fall;
                else if (wallHit && velocity.y <= .1)
                {
                    WallsInteractionLogic();
                    _currentState = PlayerState.WallSlide;
                }
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                }
                break;

            case PlayerState.SpringJump:
                HorizontalMovement(xInput);
                if (batterySpent < 2)
                    JumpLogicProcessing();
                if (dashRequest)
                    _currentState = PlayerState.Dash;
                else if (velocity.y < -3)
                    _currentState = PlayerState.Fall;
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                }
                break;

            case PlayerState.WallBreak:
                dashExpireTime = 0;
                dashRequest = false;
                dashParticles.Stop();
                if (controllsDisabledTimer > 0)
                {

                    velocity = new Vector2(-dashDirection * destroyWallsKnockback.x, destroyWallsKnockback.y);

                    controllsDisabledTimer -= Time.fixedDeltaTime;
                    //dashAlow = true;
                    batterySpent = 0;
                }
                else
                {
                    if (GroundCheck())
                    {
                        _currentState = PlayerState.Grounded;
                    }
                    else
                    {
                        _currentState = PlayerState.Fall;
                    }
                }
                break;

            case PlayerState.Dead:
                gravityActive = false;
                velocity = Vector2.zero;
                reachedMaxFallVelocity = false;
                GameController.Instance.DeathCoroutine();
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


    private void WallSlideLogicProcessing(int wallDirX, ParticleSystem.EmissionModule wsEmission)
    {
        WallsInteractionLogic();
        //print(controller.wallDirX);

        if (wallDirX == -1 )
            wallslideParticles.transform.localPosition = new Vector2(-.14f, wallslideParticles.transform.localPosition.y);
        else if (wallDirX == 1)
            wallslideParticles.transform.localPosition = new Vector2(.14f, wallslideParticles.transform.localPosition.y);
        if (wallHit && Mathf.Sign(xInput) == wallDirX && xInput != 0)
        {
            wsEmission.enabled = true;
            if (velocity.y < 0)
            {
                if (wsTimeTRMV > 0)
                {
                    velocity.y = -initialWallSlidingVelocity * (wallSlideTimeToReachMaxVelocity / wsTimeTRMV);
                    if (velocity.y <= -maxlWallSlidingVelocity)
                        velocity.y = -maxlWallSlidingVelocity;

                    wsTimeTRMV -= Time.fixedDeltaTime;
                }
                else if (wsTimeTRMV <= 0 || jumpCancel)
                {
                    velocity.y = -maxlWallSlidingVelocity;
                }
            }

        }
        else
        {
            wsEmission.enabled = false;
            wsTimeTRMV = initialWallSlidingVelocity;
            _currentState = PlayerState.Fall;
        }
    }

    private void WallJumpLogicProcessing(int wallDirX)
    {

        if (batterySpent < batteryCapacity && jumpRequest)
        {
            batterySpent++;
            velocity = new Vector2(wallJumpXVelocity * (-wallDirX), wallJumpYVelocity);
            jumpRequest = false;
        }
        else
            jumpRequest = false;
    }

    private void WallsInteractionLogic()
    {
        batterySpent = 0;
    }

    public void Dash()
    {
        dashParticles.Play();
        gravityActive = false;

        if (dashExpireTime == dashTime)
        {
            StartCoroutine(GameController.Instance.FreezeTime(dashFreezeTime));
            dashDirection = isFacingLeft ? -1 : 1;
            impulse.GenerateImpulse();
        }

        if (dashExpireTime > 0 && batterySpent < batteryCapacity)
        {
            dustParticles.Play();
            velocity.y = 0;

            velocity.x = dashDirection * dashSpeed;
            dashExpireTime -= Time.deltaTime;
        }
        else if (dashExpireTime <= 0)
        {
            dashDirection = 0;
            gravityActive = true;
            batterySpent += dashCost;
            if (GroundCheck())
                _currentState = PlayerState.Grounded;
            else if (wallHit)
            {
                _currentState = PlayerState.WallSlide;
            }
            else
                _currentState = PlayerState.Fall;
            currentGravity = gravity;
            dashExpireTime = 0f;
            isDashing = false;
        }
        else
        {
            isDashing = false;
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

    private void GroundInteractionLogic()
    {
        //dashAlow = true;

        if (GroundCheck() && _prevFrameState != _currentState)
        {
            batterySpent = 0;
            jumpCancel = false;
            jumpParticles.Play();
        }
    }

    private void JumpLogicProcessing()
    {
        if (batterySpent < batteryCapacity && jumpRequest)
        {
            jumpCancel = false;
            batterySpent += jumpCost;
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


    private void SpringJumpLogicProcessing(SpringBehaviour spring)
    {
        gravityActive = true;
        dashRequest = false;
        _currentState = PlayerState.SpringJump;
        spring.activated = true;
        velocity = spring.springVector;
        dustParticles.Play();
        batterySpent = spring.springJumpCost;
        //dashAlow = true;
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
            if (controller.faceDir == -1 && xInput < .01f)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (controller.faceDir == 1 && xInput > .01f)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
        else if (_currentState == PlayerState.WallSlide)
        {
            if (wallDirX == -1)
            {
                spriteRenderer.flipX = true;
                isFacingLeft = true;
            }
            else if (wallDirX == 1)
            {
                spriteRenderer.flipX = false;
                isFacingLeft = false;
            }
        }
    }

    private void SetAnimations()
    {

        if (_currentState == PlayerState.Dash && batterySpent < batteryCapacity)
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
            if (velocity.y > 0 && batterySpent <= 1)
                animator.Play("Jump");
            else if (batterySpent >= 2)
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
        else if (_currentState == PlayerState.WallSlide)
        {
            animator.Play("Climb");
        }
        else if (_currentState != PlayerState.Dead)
            animator.Play("Idle");

    }

    void HorizontalMovement(float input)
    {
        if (controllsEnabled)
        {
            var smoothMovementFactor = groundDamping;
            if (_currentState != PlayerState.WallJump && _currentState != PlayerState.WallBreak)
                smoothMovementFactor = controller.isGrounded ? groundDamping : inAirDumping;
            else if (_currentState == PlayerState.WallJump || _currentState == PlayerState.WallBreak)
                smoothMovementFactor = wallJumpDumping;

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
        //Debug.Log( "flags: " + controller.collisionState + ", hit.normal: " + hit.normal );
        if (hit.collider.CompareTag("Hazards"))
        {
            StartCoroutine(GameController.Instance.DeathCoroutine());
            dashRequest = false;
            //wsParticlesEmissionModule.enabled = false;
        }
        if (hit.collider.CompareTag("Destructibles"))
        {
            if (_currentState == PlayerState.Dash)
                hit.collider.GetComponent<DestroyTilesOnCollision>().DestroyTiles(hit);
        }
        if (hit.collider.CompareTag("Disappearing") && (_currentState == PlayerState.Grounded || _currentState == PlayerState.WallSlide))
            hit.collider.GetComponent<DisappearingPlatform>().Disappear();

    }


    void onTriggerEnterEvent(Collider2D col)
    {
        //Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
        if (col.CompareTag("Spring") && velocity.y <= 0)
        {
            var spring = col.GetComponent<SpringBehaviour>();
            SpringJumpLogicProcessing(spring);
        }
        if (col.CompareTag("CheckPoint"))
        {
            GameController.Instance.SetChekpoint(col.transform.position);
        }

    }

    void onTriggerStayEvent(Collider2D col)
    {
        //Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
        if (col.CompareTag("Spring") && velocity.y <= 0)
        {
            var spring = col.GetComponent<SpringBehaviour>();
            SpringJumpLogicProcessing(spring);
        }
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
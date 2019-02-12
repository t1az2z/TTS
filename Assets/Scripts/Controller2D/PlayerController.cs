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
    public Vector3 additionalMovement = Vector3.zero;

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
    //super jumps
    public float superJumpUncontrolableTime = .3f;
    public float superJumpSpeedBoost = 1.4f;
    public float superJumpExtraBoost = 1.8f;
    public float superJumpDashTimeWindow = .34f;
    public float superJumpHeightMultiplier = .8f;


    [Space(8)]

    [Foldout("Dash parameters", true)]
    public bool isDashing = false;
    public int dashCost = 1;
    public bool dashEnabled = false;
    int dashDirection = 0;
    [SerializeField] float dashTime = .2f;
    [SerializeField] float dashFreezeTime = .06f;
    [SerializeField] float dashSpeed = 250;
    public bool dashRequest = false;
    [SerializeField] Vector2 destroyWallsKnockback = new Vector2(2, 2);
    public float dashExpireTime;
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

    public float springInactiveTime = .4f;
    bool deactivateControllsOnSpring = false;

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
        if (controllsEnabled)
            PcControlls();
        xInput = (int)SimpleInput.GetAxisRaw("Horizontal");

        if (_currentState == PlayerState.Dead)
        {
            dashRequest = false;
            jumpRequest = false;
        }
        if (_currentState == PlayerState.Fall || _currentState == PlayerState.Grounded || _currentState == PlayerState.WallSlide || _currentState == PlayerState.SpringJump || _currentState == PlayerState.SuperJump)
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
                if (dashRequest && batterySpent < batteryCapacity)
                    _currentState = PlayerState.Dash;
                else if (jumpRequest && batterySpent < batteryCapacity)
                    _currentState = PlayerState.Jump;
                else if (velocity.y < -.01f)
                    _currentState = PlayerState.Fall;
                    break;
                

            case PlayerState.Jump:
                HorizontalMovement(xInput);
                JumpLogicProcessing(jumpMaxSpeed);
                if (dashRequest && batterySpent < batteryCapacity)
                    _currentState = PlayerState.Dash;
                else if (velocity.y < 0f && batterySpent < 2)
                    _currentState = PlayerState.Fall;
                else if (velocity.y < -3f && batterySpent == 2)
                    _currentState = PlayerState.Fall;
                else if (wallHit && velocity.y <0)
                    _currentState = PlayerState.WallSlide;
                else if (dashRequest && jumpRequest)
                    _currentState = PlayerState.SuperJump;
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    GroundInteractionLogic();
                }
                    break;

            case PlayerState.SuperJump:

                gravityActive = true;
                currentGravity = gravity;
                isDashing = false;
                

                dashDirection = isFacingLeft ? -1 : 1;
                if (dashExpireTime == 0)
                {
                    print("superjump from place");
                    HorizontalMovement(dashDirection * superJumpSpeedBoost);
                    SuperJumpLogicProcessing(jumpMaxSpeed * superJumpHeightMultiplier);
                }

                else if (dashExpireTime < superJumpDashTimeWindow * 2 && dashExpireTime > superJumpDashTimeWindow)
                {
                    print("duperjump");

                    HorizontalMovement(dashDirection * superJumpExtraBoost);
                    SuperJumpLogicProcessing(jumpMaxSpeed);
                }
                else if (dashExpireTime <superJumpDashTimeWindow)
                {
                    print("jumpOut");

                    HorizontalMovement(dashDirection);
                    SuperJumpLogicProcessing(jumpMaxSpeed);
                }
                else
                {
                    print("superjump");

                    HorizontalMovement(dashDirection * superJumpSpeedBoost);
                    SuperJumpLogicProcessing(jumpMaxSpeed*superJumpHeightMultiplier);
                }

                if (timeInState > superJumpUncontrolableTime)
                {
                    HorizontalMovement(xInput * superJumpSpeedBoost);
                }

                if (dashRequest && batterySpent < batteryCapacity)
                    _currentState = PlayerState.Dash;
                else if (velocity.y < 0 && batterySpent >= batteryCapacity)
                {
                    _currentState = PlayerState.Fall;
                    dashExpireTime = 0;
                }
                else if (wallHit)
                {
                    _currentState = PlayerState.WallSlide;
                    dashExpireTime = 0;
                }
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    dashExpireTime = 0;
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
                if (dashRequest && batterySpent < batteryCapacity)
                    _currentState = PlayerState.Dash;
                else if (jumpRequest && batterySpent < batteryCapacity)
                {
                    _currentState = PlayerState.Jump;
                    JumpLogicProcessing(jumpMaxSpeed);
                }
                else if (wallHit && xInput == wallDirX && !GroundCheck())
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

                if (jumpRequest && batterySpent < batteryCapacity)
                {
                    wsParticlesEmissionModule.enabled = false;
                    if (Mathf.Sign(xInput) == wallDirX)
                        _currentState = PlayerState.WallJump;
                    else
                        _currentState = PlayerState.Jump;
                }
                else if (dashRequest && batterySpent < batteryCapacity)
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

                if (dashRequest && batterySpent < batteryCapacity)
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
                if (!deactivateControllsOnSpring || timeInState >= springInactiveTime)
                    HorizontalMovement(xInput);

                if (jumpRequest && batterySpent < batteryCapacity)
                {
                    _currentState = PlayerState.Jump;
                    deactivateControllsOnSpring = false;
                }
                else if (dashRequest && batterySpent < batteryCapacity)
                {
                    _currentState = PlayerState.Dash;
                    deactivateControllsOnSpring = false;
                }
                else if (velocity.y < 0)
                {
                    _currentState = PlayerState.Fall;
                    deactivateControllsOnSpring = false;
                }
                else if (GroundCheck())
                {
                    _currentState = PlayerState.Grounded;
                    deactivateControllsOnSpring = false;
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
        controller.move((velocity + additionalMovement) * Time.deltaTime);

        velocity = controller.velocity;

        SetAnimations();
    }


    private void WallSlideLogicProcessing(int wallDirX, ParticleSystem.EmissionModule wsEmission)
    {
        WallsInteractionLogic();

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

                    wsTimeTRMV -= Time.deltaTime;
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
        gravityActive = false;

        if (dashExpireTime == dashTime)
        {
            StartCoroutine(GameController.Instance.FreezeTime(dashFreezeTime));
            dashDirection = isFacingLeft ? -1 : 1;
            impulse.GenerateImpulse();
            velocity.y = 0;
            batterySpent += dashCost;

        }

        if (dashExpireTime > 0 && batterySpent <= batteryCapacity)
        {
            if (jumpRequest && batterySpent+jumpCost <= batteryCapacity)
            {
                GameController.Instance.ResetFreezeTime();
                _currentState = PlayerState.SuperJump;
            }
            dashParticles.Play();
            dustParticles.Play();

            velocity.x = dashDirection * dashSpeed;
            dashExpireTime -= Time.deltaTime;
            if (dashExpireTime <= Time.deltaTime *4)
                dashParticles.Stop();

        }

        else if (dashExpireTime <= 0)
        {
            dashDirection = 0;
            gravityActive = true;
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
            timeInState += Time.deltaTime;
    }

    private void GroundInteractionLogic()
    {
        if (GroundCheck() && _prevFrameState != _currentState)
        {
            ResetVariablesAndRequests();
            jumpParticles.Play();
        }
    }

    public void ResetVariablesAndRequests()
    {
        //jumpRequest = false;
        dashRequest = false;
        batterySpent = 0;
        jumpCancel = false;
    }

    private void JumpLogicProcessing(float jumpSpeed)
    {
        if (batterySpent < batteryCapacity && jumpRequest)
        {
            jumpCancel = false;
            batterySpent += jumpCost;
            jumpParticles.Play();
            velocity.y = jumpSpeed;

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

    private void SuperJumpLogicProcessing(float jumpSpeed)
    {
        if (batterySpent < batteryCapacity && jumpRequest)
        {
            jumpCancel = false;
            batterySpent += jumpCost+dashCost;
            jumpParticles.Play();
            velocity.y = jumpSpeed;

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
        _currentState = PlayerState.SpringJump;
        velocity = spring.springVector;
        deactivateControllsOnSpring = spring.deactivateControlls;
        gravityActive = true;
        dashRequest = false;
        spring.activated = true;
        velocity = spring.springVector;
        dustParticles.Play();
        batterySpent = spring.springJumpCost;
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
            if (xInput == 0)
            {
                if (controller.faceDir == -1)
                {
                    spriteRenderer.flipX = true;
                    isFacingLeft = true;
                }
                else if (controller.faceDir == 1)
                {
                    spriteRenderer.flipX = false;
                    isFacingLeft = false;
                }
            }
            else
            {
                if (xInput < 0)
                {
                    spriteRenderer.flipX = true;
                    isFacingLeft = true;
                }
                else if (xInput > 0)
                {
                    spriteRenderer.flipX = false;
                    isFacingLeft = false;
                }
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

        if (_currentState == PlayerState.Dash && batterySpent <= batteryCapacity)
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

    void HorizontalMovement(float direction)
    {
        if (controllsEnabled)
        {
            var smoothMovementFactor = groundDamping;
            if (_currentState != PlayerState.WallJump && _currentState != PlayerState.WallBreak)
                smoothMovementFactor = controller.isGrounded ? groundDamping : inAirDumping;
            else if (_currentState == PlayerState.WallJump || _currentState == PlayerState.WallBreak)
                smoothMovementFactor = wallJumpDumping;

            velocity.x = Mathf.Lerp(velocity.x, direction * runSpeed, Time.deltaTime * smoothMovementFactor);
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
        if (!controller.isGrounded)
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
            var wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
            wsParticlesEmissionModule.enabled = false; //todo remove from here
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
        if (col.CompareTag("Spring"))
        {
            var wsParticlesEmissionModule = wallslideParticles.GetComponent<ParticleSystem>().emission;
            wsParticlesEmissionModule.enabled = false;
            var spring = col.GetComponent<SpringBehaviour>();
            if (_currentState == PlayerState.Grounded)
            {
                SpringJumpLogicProcessing(spring);
            }
            else if (velocity.y <= 0 && spring.springVector.x == 0)
            {
                SpringJumpLogicProcessing(spring);
            }
            else if (spring.springVector.x != 0)
            {
                SpringJumpLogicProcessing(spring);
            }

        }
        if (col.CompareTag("CheckPoint"))
        {
            GameController.Instance.SetChekpoint(col.transform.position);
        }


    }

    void onTriggerStayEvent(Collider2D col)
    {
        
    }

    void onTriggerExitEvent(Collider2D col)
    {

    }
    #endregion
}

public enum PlayerState
{
    Grounded,
    Jump,
    Dash,
    SuperJump,
    WallSlide,
    WallJump,
    Fall,
    Dead,
    Disabled,
    WallBreak,
    SpringJump,
}
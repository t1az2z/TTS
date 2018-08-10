using UnityEngine;
using System.Collections;

public class pcmb : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float jumpForce = 4f;
    public float jumpPushForce = 10f;
    bool facingRight = true;
    bool doubleJump = true;
    bool wallJumped = false;
    bool wallJumping = false;

    bool grounded;
    bool touchingWall;
    float checkRadius = 0.2f;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask whatIsGround;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        touchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround);
        float move = Input.GetAxis("Horizontal");

        if (grounded)
            rb.velocity = new Vector2(move * maxSpeed, rb.velocity.y);
        else if ((move != 0 && !wallJumping))
            rb.velocity = new Vector2(move * maxSpeed, rb.velocity.y);



        if (wallJumped)
        {
            rb.velocity = new Vector2(jumpPushForce * (facingRight ? -1 : 1), jumpForce);
            wallJumping = true;
            wallJumped = false;
        }

        if (rb.velocity.y < 0)
        {
            Debug.Log("Falling!");
            wallJumping = false;
        }
        if (grounded)
        {
            doubleJump = true;
            wallJumping = false;
        }
        if (rb.velocity.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (rb.velocity.x < 0 && facingRight)
        {
            Flip();
        }
    }

    void Update()
    {
        bool jump = Input.GetButtonDown("Jump");
        if (jump)
        {
            if (grounded)
            {
                Debug.Log("Jump!");
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            else if (touchingWall)
            {
                Debug.Log("Wall jump!");
                wallJumped = true;
            }
            else if (doubleJump)
            {
                Debug.Log("Double jump!");
                rb.velocity = new Vector2(rb.velocity.x, jumpForce / 1.1f);
                doubleJump = false;
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
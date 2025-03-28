using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;
using static UnityEngine.EventSystems.EventTrigger;
using TMPro;
public class OrangeController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    [SerializeField] float sprintRechargeDelay = 2;
    float sprintRechargeTimer;
    float currentMoveSpeed;
    float desiredMoveSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Ground;
    bool grounded;
    public float groundDrag;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Timers")]
    public float maxSprintTime = 5;
    float sprintTime;

    [Header("Booleans")]
    public static bool sprinting, crouching, walking, canSprint, playerIsMoving;

    public Transform orientation;
    Vector3 spawnPoint;
    Rigidbody rb;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToJump = true;
        canSprint = true;
        desiredMoveSpeed = walkSpeed;
        sprinting = false;
        sprintRechargeTimer = sprintRechargeDelay;

        sprintTime = maxSprintTime;

        spawnPoint = transform.position;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //ground check -- 0.5f is half the players height and 0.2f is extra length
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground);

        walking = (Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.2f);

        playerIsMoving = (Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f);

        MyInput();
        speedControl();
        RechargeSprint();

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        if (gameObject.transform.position.y < -40f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        Debug.Log(currentMoveSpeed);
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    //Here are the controls that switch the player to different movement states
    //and toggle menus with buttons
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        StopAllCoroutines();
        StartCoroutine(SmoothlyLerpMoveSpeed());

        // Sprinting
        if (Input.GetKey(sprintKey) && grounded && sprintTime > 0f)
        {
            sprintRechargeTimer = sprintRechargeDelay;

            sprintTime -= Time.deltaTime;
            desiredMoveSpeed = sprintSpeed;
            sprinting = true;
        }

        // Jumping 
        else if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Walking
        else if (grounded)
        {
            desiredMoveSpeed = walkSpeed;

            sprinting = false;
        }
    }


    // Here is the method used to move the player
    void PlayerMovement()
    {
        rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f, ForceMode.Force);
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //Walking Boolean
        if (walking)
        {
            walking = true;
        }
        else
        {
            walking = false;
        }
    }
    private void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit velocity
        if (flatVel.magnitude > desiredMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * desiredMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    // Current speed lerps to the speed of the current state(walking, sprinting, etc)
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // lerp movementSpeed to desired speed
        float difference = Mathf.Abs(desiredMoveSpeed - currentMoveSpeed);
        //float startValue = currentMoveSpeed;

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, desiredMoveSpeed, Time.deltaTime);

        yield return null;
    }

    // Prevents jumping multiple times
    void ResetJump()
    {
        readyToJump = true;
    }

    // Recharges sprint when not running
    void RechargeSprint()
    {
        if (!Input.GetKey(sprintKey) || sprintTime < maxSprintTime)
        {
            sprintRechargeTimer -= Time.deltaTime;

            if (sprintRechargeTimer <= 0f)
            {
                sprintTime += Time.deltaTime;
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;
using static UnityEngine.EventSystems.EventTrigger;
using TMPro;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    [SerializeField] float sprintRechargeDelay = 2;
    float sprintRechargeTimer;
    float currentMoveSpeed;
    float desiredMoveSpeed;

    [Header("Crouching")]
    public float crouchYScale;
    float startYScale;
    bool canStand;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Ground;
    bool grounded;
    public float groundDrag;

    [Header("Keybinds")]
    public KeyCode crouchKey = KeyCode.C;
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

    public TextMeshProUGUI orangeCount_UI;
    public GameObject interact_Text;
    public GameObject consume_Text;
    public static int numberOfOranges = 0;
    [SerializeField] float pickUpDistance = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crouching = false;
        canSprint = true;
        desiredMoveSpeed = walkSpeed;
        sprinting = false;
        sprintRechargeTimer = sprintRechargeDelay;

        sprintTime = maxSprintTime;

        spawnPoint = transform.position;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
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

        orangeCount_UI.text = numberOfOranges.ToString("") + " / 24";

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpDistance))
        {
            if (hit.collider.name == "Orange")
            {
                // Show interact text if the player is looking at an object they can interact with
                interact_Text.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // Here you can add code to pick up the object
                    Debug.Log("Picked up " + hit.collider.name);
                    // For example, destroy the object or add it to inventory
                    Destroy(hit.collider.gameObject);
                    numberOfOranges += 1;
                }
            }
            else
            {
                interact_Text.SetActive(false);
            }

            if (hit.collider.name == "Final Orange")
            {
                consume_Text.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    SceneManager.LoadScene("Win");
                }
            }
            else
            {
                consume_Text.SetActive(false);
            }
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

        // Start Crouching
        if (Input.GetKeyDown(crouchKey) && CanCrouch())
        {
            desiredMoveSpeed = crouchSpeed;

            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
            sprinting = false;

            Debug.Log("Crouching");
        }

        // Stop Crouching
        else if (Input.GetKeyUp(crouchKey) && grounded)
        {
            desiredMoveSpeed = walkSpeed;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            crouching = false;
        }

        // Sprinting
        else if (Input.GetKey(sprintKey) && grounded && crouching == false && sprintTime > 0f)
        {
            sprintRechargeTimer = sprintRechargeDelay;

            sprintTime -= Time.deltaTime;
            desiredMoveSpeed = sprintSpeed;
            sprinting = true;

            Debug.Log("Running");
        }

        // Walking
        else if (grounded && crouching == false)
        {
            desiredMoveSpeed = walkSpeed;

            sprinting = false;

            Debug.Log("Walking");
        }
    }

    public bool CanCrouch()
    {
        return grounded;
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

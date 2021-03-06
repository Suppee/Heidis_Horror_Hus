using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] Flashlight flashlight;

    //UI and Fade To black variables
    [SerializeField] GameObject noteUI;
    [SerializeField] Image blackSquare;
    [SerializeField] float fadeTime;

    //Camera control variables
    public float xRotation;
    [SerializeField] [Range(2.5f, 25f)] float sensitivityX = 8f;
    [SerializeField] [Range(2.5f, 25f)] float sensitivityY = 0.5f;

    Vector2 lookValue;
    public Transform playerCamera;
    public bool canControl = true;

    //Sound variables
    private int pitchOg = 1;
    [SerializeField] AudioSource audioSource;
    [SerializeField] private Vector2[] pitchMods;
    [SerializeField] private float[] volMod;
    [SerializeField] private float[] soundTimer;
    private float lastFootstep;

    //Movement variables
    bool isMoving;
    bool isGrounded;
    float gravity = -9.85f;
    public float moveSpeed;
    public Vector2 moveValue;
    [SerializeField] LayerMask groundMask;
    Vector3 verticalVelocity = Vector3.zero;
    [SerializeField] private AudioClip footstepsConcrete;
    [SerializeField] private AudioClip running;
    [SerializeField] private AudioClip sneaking;
    Vector3 move;
    Vector3 moveCrouch;
    Vector3 moveSprint;

    //Crouch variables
    [SerializeField] GameObject Body;
    private Vector3 crouchScale = new Vector3(0.75f, 0.5f, 0.75f);
    private Vector3 playerScale;
    bool CrouchActive = false;

    //Sprint variables
    bool SprintActive = false;

    //Keyring variables
    public List<Key> keyring;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        canControl = false;
        StartCoroutine(WakeUp());

        if (noteUI != null)
            noteUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        //Checks if player is moving
        if (moveValue.x != 0 || moveValue.y != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // Character movement
        move = (transform.right * moveValue.x + transform.forward * moveValue.y) * moveSpeed;
        moveCrouch = move / 2;
        moveSprint = move * 2;

        
        verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        //Moves player based on stance
        if (CrouchActive && isMoving)
        {
            ControlMovement(moveCrouch, soundTimer[0], sneaking, pitchMods[0].x, pitchMods[0].y, volMod[0]);
        }
        else if (SprintActive && isMoving)
        {
            ControlMovement(moveSprint, soundTimer[1], running, pitchMods[1].x, pitchMods[1].y, volMod[1]);
        }
        else if (isMoving)
        {
            ControlMovement(move, soundTimer[2], footstepsConcrete, pitchMods[2].x, pitchMods[2].y, volMod[2]);
        }
    }

    private void ControlMovement(Vector3 moveMod, float interval, AudioClip audioClip, float pitchLo, float pitchHi, float volume)
    {
        if (controller.enabled)
        {
            //moves player
            controller.Move((moveMod + verticalVelocity) * Time.deltaTime);

            //plays footstep sounds i last footstep is longer ago than allowed limit
            if (Time.fixedTime - interval >= lastFootstep)
            {
                audioSource.Stop();

                //replaces audio clip with correct clip
                if (audioSource.clip != audioClip)
                {
                    audioSource.clip = audioClip;
                }

                //Sets pitch and volume
                audioSource.pitch = Random.Range(pitchOg * pitchLo, pitchOg * pitchHi);
                audioSource.volume = volume;

                //plays footstep and records current time
                audioSource.Play();
                lastFootstep = Time.fixedTime;
            }
        }
    }

    // Input value from mouse 
    public void Look(InputAction.CallbackContext context)
    {
        if (canControl)
        {
            lookValue = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);

            //local variables
            float mouseX = lookValue.x * sensitivityX * Time.fixedDeltaTime;
            float mouseY = lookValue.y * sensitivityY * Time.fixedDeltaTime;

            //Find current look rotation
            Vector3 rot = transform.localRotation.eulerAngles;
            float desiredX = rot.y + mouseX;

            //Rotate, and also make sure we dont over- or under-rotate.
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //Perform the rotations
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.localRotation = Quaternion.Euler(0, desiredX, 0);
        }
    }

    // Input values from WASD
    public void Move(InputAction.CallbackContext context)
    {
        if (canControl)
        {
            moveValue = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
        }
    }

    // Input value from Interact
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit hit;
            Debug.DrawRay(playerCamera.position, playerCamera.forward * 3, Color.red, 10);
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 3))
            {
                if (hit.transform.CompareTag("Interactable"))
                {
                    print("Interacting with " + hit.collider.gameObject);
                    Interactable interactable = hit.collider.gameObject.GetComponentInParent<Interactable>();

                    if (interactable.playerController != this)
                    {
                        interactable.playerController = this;
                    }

                    interactable.Interact();
                }
            }
        }
    }

    // Input value from flashlight
    public void Flashlight(InputAction.CallbackContext context)
    {
        if (context.performed && canControl && flashlight.gameObject.activeSelf == true)
        {
            flashlight.TurnOnOff();
        }
    }

    // Input value from crouch
    public void Crouch(InputAction.CallbackContext context)
    {

        if (context.performed && CrouchActive)
        {
            CrouchActive = false;
        }
        else if (context.performed)
        {
            CrouchActive = true;
        }

        if (CrouchActive)
        {
            transform.localScale = crouchScale;
            flashlight.gameObject.transform.localScale = new Vector3(3, 2, 6);
        }
        else
        {
            transform.localScale = playerScale;
            flashlight.gameObject.transform.localScale = new Vector3(3, 2, 3);
        }
    }

    // Input value from sprint
    public void Sprint(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            SprintActive = true;
        }
        else
        {
            SprintActive = false;
        }
    }

    IEnumerator WakeUp()
    {
        blackSquare.gameObject.SetActive(true);
        float fadeVal;

        while (blackSquare.color.a > 0)
        {
            fadeVal = blackSquare.color.a - (fadeTime * Time.deltaTime);
            blackSquare.color = new Color(blackSquare.color.r, blackSquare.color.g, blackSquare.color.b, fadeVal);
            yield return null;
        }

        canControl = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == groundMask)
        {
            verticalVelocity.y = 0;
        }
    }
}

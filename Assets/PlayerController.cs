using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Camera control variables
    public float cameraSensitivity = 25f;
    private float xRotCamera;
    Vector2 lookValue;
    public Transform playerCamera;

    //Movement variables
    public float moveSpeed;
    Vector2 moveValue;

    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Lookaround();


        // Forward/Backwards movement
        Vector3 move = transform.right * moveValue.x + transform.forward * moveValue.y;

  
        this.gameObject.GetComponent<CharacterController>().SimpleMove(move * moveSpeed);
      
    }

    private void Lookaround()
    {
        float yRotCamera;
        float mouseX = lookValue.x * cameraSensitivity * Time.fixedDeltaTime;
        float mouseY = lookValue.y * cameraSensitivity * Time.fixedDeltaTime;

        //Find current look rotation
        Vector3 rot = playerCamera.transform.localRotation.eulerAngles;

        // Adds the mouse x value as rotation on the camera (left/right rotation around the cameras Y axis)
        yRotCamera = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotCamera -= mouseY;
        //Clamps the up and down rotation between -90 and 90 degrees to avoid 
        xRotCamera = Mathf.Clamp(xRotCamera, -90f, 90f);

        //Rotate camera up and down  
        playerCamera.transform.localRotation = Quaternion.Euler(xRotCamera, yRotCamera, 0);

        // Rotate whole character when moving mouse left/right
        transform.localRotation = Quaternion.Euler(0, yRotCamera, 0);
    }

    // Input value from mouse 
    public void Look(InputAction.CallbackContext context)
    {
        lookValue = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
    }

    // Input values from WASD
    public void Move(InputAction.CallbackContext context)
    {
        moveValue = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
        Debug.Log(moveValue);
    }

    // Input value from Interact
    public void Interact(InputAction.CallbackContext context)
    {
        print("yeet");
    }
}
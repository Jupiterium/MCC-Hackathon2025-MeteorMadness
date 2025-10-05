using UnityEngine;
using UnityEngine.InputSystem;   // New Input System

public class SpectatorCamera : MonoBehaviour
{
    [Header("Speeds")]
    public float slowSpeed = 2f;
    public float normalSpeed = 6f;
    public float sprintSpeed = 12f;

    [Header("Look")]
    public float sensitivity = 0.15f; // mouse delta multiplier

    private float currentSpeed;

    private void Update()
    {
        // Check if Right Mouse Button is held using the new Input System
        bool holdingRightClick = Mouse.current != null && Mouse.current.rightButton.isPressed;

        if (holdingRightClick)
        {
            // Lock and hide cursor while looking around
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Movement();
            Rotation();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Rotation()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // Mouse delta from new Input System (pixels since last frame)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Convert to our Vector3: X rotation is inverted for typical FPS feel
        Vector3 mouseInput = new Vector3(-mouseDelta.y, mouseDelta.x, 0f);

        // Apply rotation (scaled by sensitivity)
        transform.Rotate(mouseInput * sensitivity, Space.Self);

        // Zero out any roll so the camera never tilts sideways
        Vector3 euler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, euler.y, 0f);
    }

    private void Movement()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Build movement input from WASD / Arrow keys
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            x -= 1f;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            x += 1f;
        }
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            z += 1f;
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            z -= 1f;
        }

        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        // Speed modifiers (Shift = sprint, LeftAlt = slow)
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
        {
            currentSpeed = sprintSpeed;
        }
        else if (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed)
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }

        // Move in local space
        transform.Translate(input * currentSpeed * Time.deltaTime, Space.Self);
    }
}

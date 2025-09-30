using UnityEngine;
using UnityEngine.InputSystem; // New Input System

/// <summary>
/// Split the game view into two resizable panes, each rendered by its own Camera.
/// - Drag the vertical divider to resize the panes.
/// - Scroll the mouse wheel over a pane to zoom that pane's camera (FOV or Ortho size).
/// </summary>
public class SplitScreen : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("Camera that renders the LEFT pane.")]
    public Camera leftCam;

    [Tooltip("Camera that renders the RIGHT pane.")]
    public Camera rightCam;

    [Header("Split / Divider")]
    [Tooltip("Divider position as a fraction of screen width (0..1).")]
    [Range(0.1f, 0.9f)]
    public float splitX = 0.5f;

    [Tooltip("Divider thickness in pixels.")]
    public float dividerWidthPx = 4f;

    [Header("Zoom Settings")]
    [Tooltip("How fast zoom changes per scroll unit.")]
    public float zoomSpeed = 5f;

    [Tooltip("Min/Max Field of View for perspective cameras.")]
    public float minFOV = 20f;
    public float maxFOV = 80f;

    [Tooltip("Min/Max Orthographic Size for ortho cameras.")]
    public float minOrtho = 1f;
    public float maxOrtho = 500f;

    // Internal state for divider dragging
    private bool dragging = false;

    private void Start()
    {
        // Apply initial camera rectangles immediately so Play Mode starts split.
        ApplyRects();
    }

    private void Update()
    {
        // Handle scroll zoom using the New Input System
        Mouse mouse = Mouse.current;

        // If there is no mouse, do nothing.
        if (mouse == null)
        {
            return;
        }

        // Read scroll wheel (Y axis)
        // Scale it down so zoom feels comfortable.
        float scroll = mouse.scroll.ReadValue().y * 0.1f;

        // If there is meaningful scroll input, decide which pane we're over and zoom that camera.
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            // Mouse position in pixels -> normalize to [0..1] across screen width.
            float mouseX = mouse.position.ReadValue().x;
            float normX = mouseX / Screen.width;

            // If mouse is left of the divider, zoom left camera; otherwise zoom right camera.
            if (normX < splitX)
            {
                Zoom(leftCam, scroll);
            }
            else
            {
                Zoom(rightCam, scroll);
            }
        }
    }

    private void Zoom(Camera cam, float scroll)
    {
        if (cam == null)
        {
            return;
        }

        // Positive scroll should zoom IN (reduce FOV / Ortho size).
        if (cam.orthographic == true)
        {
            float size = cam.orthographicSize;
            size = size - (scroll * zoomSpeed);

            if (size < minOrtho)
            {
                size = minOrtho;
            }
            if (size > maxOrtho)
            {
                size = maxOrtho;
            }

            cam.orthographicSize = size;
        }
        else
        {
            float fov = cam.fieldOfView;
            fov = fov - (scroll * zoomSpeed);

            if (fov < minFOV)
            {
                fov = minFOV;
            }
            if (fov > maxFOV)
            {
                fov = maxFOV;
            }

            cam.fieldOfView = fov;
        }
    }

    private void ApplyRects()
    {
        if (leftCam != null)
        {
            leftCam.rect = new Rect(0f, 0f, splitX, 1f);
        }

        if (rightCam != null)
        {
            rightCam.rect = new Rect(splitX, 0f, 1f - splitX, 1f);
        }
    }

    private void OnGUI()
    {
        // Divider center in pixels
        float xPxCenter = (splitX * Screen.width);

        // Left edge of the divider rect in pixels
        float xPx = xPxCenter - (dividerWidthPx * 0.5f);

        // Draw a divider for the user to see
        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.DrawTexture(new Rect(xPx, 0f, dividerWidthPx, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Mouse events for dragging
        Event e = Event.current;
        Rect dividerRect = new Rect(xPx, 0f, dividerWidthPx, Screen.height);

        // Start dragging when mouse is pressed over the divider
        if (e.type == EventType.MouseDown && dividerRect.Contains(e.mousePosition))
        {
            dragging = true;
        }

        // While dragging, update splitX based on mouse X; clamp to keep both panes visible
        if (dragging == true && e.type == EventType.MouseDrag)
        {
            float newSplit = e.mousePosition.x / Screen.width;

            if (newSplit < 0.1f)
            {
                newSplit = 0.1f;
            }
            if (newSplit > 0.9f)
            {
                newSplit = 0.9f;
            }

            splitX = newSplit;
            ApplyRects();
        }

        // Stop dragging on mouse up
        if (e.type == EventType.MouseUp)
        {
            dragging = false;
        }
    }
}

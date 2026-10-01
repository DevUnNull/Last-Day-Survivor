using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool flipSpriteTowardsMovement = true;
    [SerializeField] private bool invertFlipX = true; // Set true if default sprite faces Left

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configure 2D Rigidbody settings
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        // New Input System: Keyboard (WASD & Arrow Keys)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveY += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveY -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;
        }

        // New Input System: Gamepad (Left Stick & D-Pad)
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.05f)
            {
                moveX = stick.x;
                moveY = stick.y;
            }

            if (gamepad.dpad.up.isPressed) moveY = 1f;
            if (gamepad.dpad.down.isPressed) moveY = -1f;
            if (gamepad.dpad.right.isPressed) moveX = 1f;
            if (gamepad.dpad.left.isPressed) moveX = -1f;
        }

        // Normalize movement vector
        moveInput = new Vector2(moveX, moveY).normalized;

        // Flip sprite correctly according to movement direction
        if (flipSpriteTowardsMovement && spriteRenderer != null)
        {
            if (moveX > 0.01f)
            {
                spriteRenderer.flipX = invertFlipX; // Right movement
            }
            else if (moveX < -0.01f)
            {
                spriteRenderer.flipX = !invertFlipX; // Left movement
            }
        }
    }

    private void FixedUpdate()
    {
        // Move player using 2D physics
        Vector2 targetPosition = rb.position + moveInput * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }
}

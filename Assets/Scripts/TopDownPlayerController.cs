using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool flipSpriteTowardsMovement = true;
    [SerializeField] private bool invertFlipX = true; // Set true if default sprite faces Left
    [SerializeField] private float stepInterval = 0.32f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private float stepTimer = 0f;

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

        // Footstep audio logic
        if (moveInput.sqrMagnitude > 0.01f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPlayerFootstep();
                }
            }
        }
        else
        {
            stepTimer = 0.05f; // Fast trigger on first step
        }
    }

    private void FixedUpdate()
    {
        // Move player using 2D physics with stats multiplier
        float speed = moveSpeed;
        if (PlayerStats.Instance != null)
        {
            speed *= PlayerStats.Instance.MoveSpeedMultiplier;
        }

        Vector2 targetPosition = rb.position + moveInput * (speed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }
}

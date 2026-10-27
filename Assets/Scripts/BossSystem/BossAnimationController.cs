using System.Collections;
using UnityEngine;

public enum BossAnimationState
{
    Idle,
    Intro,
    Attack,
    Hit,
    Stunned,
    PhaseTransition,
    Death
}

[RequireComponent(typeof(SpriteRenderer))]
public class BossAnimationController : MonoBehaviour
{
    [Header("Primary Visual Component (Frame-by-Frame Sprite)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator; // Optional Animator for frame-by-frame clips

    [Header("Hit Flash Feedback")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Sprite Facing")]
    [SerializeField] private bool flipXToFace = true;
    [SerializeField] private bool defaultFacingRight = true;

    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;
    private BossAnimationState currentState = BossAnimationState.Idle;

    public BossAnimationState CurrentState => currentState;
    public SpriteRenderer MainSpriteRenderer => spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public void SetDefaultSprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public void SetTint(Color tint)
    {
        originalColor = tint;
        if (spriteRenderer != null && flashCoroutine == null)
        {
            spriteRenderer.color = tint;
        }
    }

    public void FaceTarget(Vector3 targetPos)
    {
        if (!flipXToFace || spriteRenderer == null) return;

        float diffX = targetPos.x - transform.position.x;
        if (Mathf.Abs(diffX) > 0.1f)
        {
            bool shouldFaceRight = diffX > 0f;
            spriteRenderer.flipX = defaultFacingRight ? !shouldFaceRight : shouldFaceRight;
        }
    }

    public void PlayState(BossAnimationState newState, string customTrigger = null)
    {
        currentState = newState;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            string triggerToFire = !string.IsNullOrEmpty(customTrigger) ? customTrigger : newState.ToString();
            SafeSetTrigger(triggerToFire);
        }
    }

    public void PlayIdle() => PlayState(BossAnimationState.Idle);
    public void PlayIntro() => PlayState(BossAnimationState.Intro);
    public void PlayAttack(string attackTrigger = "Attack") => PlayState(BossAnimationState.Attack, attackTrigger);
    public void PlayStunned() => PlayState(BossAnimationState.Stunned);
    public void PlayPhaseTransition() => PlayState(BossAnimationState.PhaseTransition);
    public void PlayDeath() => PlayState(BossAnimationState.Death);

    public void PlayHitFlash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            SafeSetTrigger("Hit");
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }

    private void SafeSetTrigger(string triggerName)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;

        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }
    }
}

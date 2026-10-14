
using UnityEngine;

public class UIImageAnimation : MonoBehaviour
{
    public enum AnimationType
    {
        Float,
        Pulse,
        Rotate,
        Fade
    }

    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float amount = 10f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
        startRotation = rectTransform.localRotation;
    }

    private void Update()
    {
        float time = Time.unscaledTime * speed;

        switch (animationType)
        {
            case AnimationType.Float:
                rectTransform.anchoredPosition =
                    startPosition + Vector2.up * Mathf.Sin(time) * amount;
                break;

            case AnimationType.Pulse:
                float scale = 1f + Mathf.Sin(time) * amount * 0.01f;
                rectTransform.localScale = startScale * scale;
                break;

            case AnimationType.Rotate:
                rectTransform.localRotation =
                    startRotation * Quaternion.Euler(0f, 0f, time * amount);
                break;

            case AnimationType.Fade:
                if (canvasGroup != null)
                {
                    canvasGroup.alpha =
                        0.5f + 0.5f * Mathf.Sin(time);
                }
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimation : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationSpeed = 15f;

    [Header("Audio Settings")]
    [SerializeField] private bool playAudio = true;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isHovering;
    private bool isPressing;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime)
        );
    }

    // Khi chuột đi vào Button
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateTargetScale();

        if (playAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIHover();
        }
    }

    // Khi chuột rời khỏi Button
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        UpdateTargetScale();
    }

    // Khi nhấn chuột xuống
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressing = true;
        UpdateTargetScale();

        if (playAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
    }

    // Khi thả chuột
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressing = false;
        UpdateTargetScale();
    }

    private void UpdateTargetScale()
    {
        if (isPressing)
        {
            targetScale = originalScale * clickScale;
        }
        else if (isHovering)
        {
            targetScale = originalScale * hoverScale;
        }
        else
        {
            targetScale = originalScale;
        }
    }

    private void OnDisable()
    {
        if (rectTransform != null)
            rectTransform.localScale = originalScale;

        isHovering = false;
        isPressing = false;
        targetScale = originalScale;
    }
}

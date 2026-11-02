using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static MobileJoystick Instance { get; private set; }
    public static Vector2 InputVector { get; private set; }

    [Header("Joystick Settings")]
    [SerializeField] private float handleRange = 65f;
    [SerializeField] private bool autoCreateVisuals = true;

    [Header("UI References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    private Canvas canvas;
    private Camera cam;
    private Vector2 startPos;
    private bool isDragging = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        if (autoCreateVisuals && (background == null || handle == null))
        {
            CreateJoystickVisuals();
        }

        if (background != null)
        {
            startPos = background.anchoredPosition;
        }
    }

    private void OnDestroy()
    {
        InputVector = Vector2.zero;
        if (Instance == this) Instance = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta * 0.5f;

        Vector2 input = (eventData.position - position) / (radius.magnitude > 0 ? radius.magnitude : 1f);
        if (input.magnitude > 1f)
        {
            input = input.normalized;
        }

        InputVector = input;
        handle.anchoredPosition = input * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        InputVector = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void CreateJoystickVisuals()
    {
        // 1. Root container (Anchor to bottom-left quadrant)
        RectTransform rootRt = GetComponent<RectTransform>();
        if (rootRt == null) rootRt = gameObject.AddComponent<RectTransform>();

        rootRt.anchorMin = new Vector2(0f, 0f);
        rootRt.anchorMax = new Vector2(0.5f, 0.6f); // Left touch zone
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // Transparent image to capture touches across the entire left side of the screen
        Image zoneImg = GetComponent<Image>();
        if (zoneImg == null) zoneImg = gameObject.AddComponent<Image>();
        zoneImg.color = Color.clear;
        zoneImg.raycastTarget = true;

        // 2. Background Ring
        if (background == null)
        {
            GameObject bgObj = new GameObject("JoystickBackground");
            bgObj.transform.SetParent(transform, false);
            background = bgObj.AddComponent<RectTransform>();
            background.anchorMin = new Vector2(0f, 0f);
            background.anchorMax = new Vector2(0f, 0f);
            background.pivot = new Vector2(0.5f, 0.5f);
            background.anchoredPosition = new Vector2(220f, 220f);
            background.sizeDelta = new Vector2(180f, 180f);

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.22f); // Translucent ring
            bgImg.raycastTarget = false;
        }

        // 3. Thumb Handle
        if (handle == null)
        {
            GameObject handleObj = new GameObject("JoystickHandle");
            handleObj.transform.SetParent(background.transform, false);
            handle = handleObj.AddComponent<RectTransform>();
            handle.anchorMin = new Vector2(0.5f, 0.5f);
            handle.anchorMax = new Vector2(0.5f, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(80f, 80f);

            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.color = new Color(1f, 0.9f, 0.3f, 0.6f); // Soft golden glow
            handleImg.raycastTarget = false;
        }
    }
}

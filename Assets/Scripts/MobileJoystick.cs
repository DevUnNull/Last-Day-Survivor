using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static MobileJoystick Instance { get; private set; }
    public static Vector2 InputVector { get; private set; }

    [Header("Joystick Settings")]
    [SerializeField] private float handleRange = 55f;
    [SerializeField] private bool autoCreateVisuals = true;

    [Header("Sprite Assets (Pixel Art)")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite handleSprite;

    [Header("UI References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image touchZoneImage;

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

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        touchZoneImage = GetComponent<Image>();

        if (autoCreateVisuals && (background == null || handle == null))
        {
            CreateJoystickVisuals();
        }

        if (background != null)
        {
            startPos = background.anchoredPosition;
        }
    }

    private void Update()
    {
        // When game is paused (e.g. LevelUp upgrade selection, Pause, Game Over),
        // completely hide and disable joystick raycasting so buttons and cards receive all clicks cleanly.
        bool isPaused = Time.timeScale <= 0f;
        if (isPaused)
        {
            if (isDragging)
            {
                isDragging = false;
                InputVector = Vector2.zero;
                if (handle != null) handle.anchoredPosition = Vector2.zero;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                canvasGroup.alpha = 0f;
            }
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
            }
        }
    }

    private void OnDestroy()
    {
        InputVector = Vector2.zero;
        if (Instance == this) Instance = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Time.timeScale <= 0f) return;
        isDragging = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Time.timeScale <= 0f)
        {
            OnPointerUp(eventData);
            return;
        }

        if (background == null || handle == null) return;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        float radius = background.sizeDelta.x * 0.5f;
        if (radius <= 0f) radius = 80f;

        Vector2 offset = eventData.position - position;
        Vector2 input = offset / radius;
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
        // Load Pixel Art Sprites if not assigned
        if (backgroundSprite == null)
        {
            backgroundSprite = Resources.Load<Sprite>("UI/joystick_base");
#if UNITY_EDITOR
            if (backgroundSprite == null)
            {
                backgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/UI/joystick_base.png");
            }
#endif
        }

        if (handleSprite == null)
        {
            handleSprite = Resources.Load<Sprite>("UI/joystick_knob");
#if UNITY_EDITOR
            if (handleSprite == null)
            {
                handleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/UI/joystick_knob.png");
            }
#endif
        }

        // 1. Root container (Confined strictly to bottom-left corner 300x300, avoiding card areas)
        RectTransform rootRt = GetComponent<RectTransform>();
        if (rootRt == null) rootRt = gameObject.AddComponent<RectTransform>();

        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.zero;
        rootRt.pivot = Vector2.zero;
        rootRt.anchoredPosition = Vector2.zero;
        rootRt.sizeDelta = new Vector2(300f, 300f);

        touchZoneImage = GetComponent<Image>();
        if (touchZoneImage == null) touchZoneImage = gameObject.AddComponent<Image>();
        touchZoneImage.color = Color.clear;
        touchZoneImage.raycastTarget = true;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 2. Background Ring
        if (background == null)
        {
            GameObject bgObj = new GameObject("JoystickBackground");
            bgObj.transform.SetParent(transform, false);
            background = bgObj.AddComponent<RectTransform>();
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.zero;
            background.pivot = new Vector2(0.5f, 0.5f);
            background.anchoredPosition = new Vector2(150f, 150f);
            background.sizeDelta = new Vector2(160f, 160f);

            Image bgImg = bgObj.AddComponent<Image>();
            if (backgroundSprite != null)
            {
                bgImg.sprite = backgroundSprite;
                bgImg.color = new Color(1f, 1f, 1f, 0.8f);
            }
            else
            {
                bgImg.color = new Color(1f, 1f, 1f, 0.25f);
            }
            bgImg.preserveAspect = true;
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
            handle.sizeDelta = new Vector2(75f, 75f);

            Image handleImg = handleObj.AddComponent<Image>();
            if (handleSprite != null)
            {
                handleImg.sprite = handleSprite;
                handleImg.color = new Color(1f, 1f, 1f, 0.95f);
            }
            else
            {
                handleImg.color = new Color(1f, 0.9f, 0.3f, 0.6f);
            }
            handleImg.preserveAspect = true;
            handleImg.raycastTarget = false;
        }
    }
}

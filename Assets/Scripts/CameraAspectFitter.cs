using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectFitter : MonoBehaviour
{
    [Header("Target Reference Aspect (16:9 Standard)")]
    [SerializeField] private float targetAspect = 16f / 9f;
    [SerializeField] private float baseOrthographicSize = 5f;

    private Camera cam;
    private float lastAspect = 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttachToCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CameraAspectFitter>() == null)
        {
            mainCam.gameObject.AddComponent<CameraAspectFitter>();
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void Update()
    {
        if (cam != null && !Mathf.Approximately(cam.aspect, lastAspect))
        {
            AdjustCamera();
        }
    }

    private void AdjustCamera()
    {
        if (cam == null || !cam.orthographic) return;

        lastAspect = cam.aspect;

        if (cam.aspect < targetAspect)
        {
            // Screen is taller/narrower than 16:9 (e.g. 4:3, 16:10 tablets, foldables)
            // Expand orthographic size so horizontal arena width is preserved without clipping
            float scaleRatio = targetAspect / cam.aspect;
            cam.orthographicSize = baseOrthographicSize * scaleRatio;
        }
        else
        {
            // Screen is wider than 16:9 (e.g. 19.5:9, 20:9 modern smartphones)
            // Maintain base orthographic size so vertical playfield is preserved
            cam.orthographicSize = baseOrthographicSize;
        }
    }
}

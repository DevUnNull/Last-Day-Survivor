using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(Quit);
            btn.onClick.AddListener(Quit);
        }
    }

    public void Quit()
    {
        Debug.Log("[QuitButton] Thoát trò chơi...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

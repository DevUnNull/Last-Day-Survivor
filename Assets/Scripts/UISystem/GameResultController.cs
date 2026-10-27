using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultController : MonoBehaviour
{
    public static GameResultController Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Victory UI Elements")]
    [SerializeField] private Image victoryBannerImage;
    [SerializeField] private TextMeshProUGUI victoryTitleText;
    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI victoryKillsText;
    [SerializeField] private TextMeshProUGUI victoryRewardsText;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMenuButton;

    [Header("Defeat UI Elements")]
    [SerializeField] private Image defeatBannerImage;
    [SerializeField] private TextMeshProUGUI defeatTitleText;
    [SerializeField] private TextMeshProUGUI defeatTimeText;
    [SerializeField] private TextMeshProUGUI defeatKillsText;
    [SerializeField] private Button defeatRestartButton;
    [SerializeField] private Button defeatMenuButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (victoryRestartButton != null) victoryRestartButton.onClick.AddListener(RestartGame);
        if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (defeatRestartButton != null) defeatRestartButton.onClick.AddListener(RestartGame);
        if (defeatMenuButton != null) defeatMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void ShowVictory(MapData map, float timeSurvived, int kills, int gold)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryTitleText != null)
            {
                victoryTitleText.text = map != null ? $"{map.mapName.ToUpper()} CONQUERED!" : "VICTORY!";
            }

            int m = Mathf.FloorToInt(timeSurvived / 60f);
            int s = Mathf.FloorToInt(timeSurvived % 60f);
            if (victoryTimeText != null) victoryTimeText.text = $"Time: {m:00}:{s:00}";
            if (victoryKillsText != null) victoryKillsText.text = $"Enemies Slain: {kills}";
            if (victoryRewardsText != null) victoryRewardsText.text = $"Gold Earned: +{gold}";
        }
    }

    public void ShowDefeat(MapData map, float timeSurvived, int kills)
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);

            if (defeatTitleText != null)
            {
                defeatTitleText.text = "DEFEATED";
            }

            int m = Mathf.FloorToInt(timeSurvived / 60f);
            int s = Mathf.FloorToInt(timeSurvived % 60f);
            if (defeatTimeText != null) defeatTimeText.text = $"Time: {m:00}:{s:00}";
            if (defeatKillsText != null) defeatKillsText.text = $"Enemies Slain: {kills}";
        }
        else if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("1_1_MainMenu");
    }
}

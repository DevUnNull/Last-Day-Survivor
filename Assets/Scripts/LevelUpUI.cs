using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private Text headerTitleText;
    [SerializeField] private Text subtitleText;

    [Header("Upgrade Cards")]
    [SerializeField] private UpgradeCardUI[] cards;

    public bool IsShowing => modalPanel != null && modalPanel.activeSelf;

    public void Show(List<UpgradeOption> options)
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(true);
        }

        // Pause gameplay
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardAppear();
        }

        if (headerTitleText != null)
        {
            headerTitleText.text = "LEVEL UP!";
        }
        if (subtitleText != null)
        {
            subtitleText.text = "Choose an upgrade to empower your Wizard";
        }

        if (cards == null || cards.Length == 0)
        {
            cards = GetComponentsInChildren<UpgradeCardUI>(true);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < options.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(options[i], this);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnCardClicked(UpgradeOption option)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardSelect();
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChosen(option);
        }
    }

    public void Hide()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }

        // Resume gameplay
        Time.timeScale = 1f;
    }
}

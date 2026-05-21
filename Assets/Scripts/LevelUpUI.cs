using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;

    [Header("Три карты выбора")]
    public Button cardButton1;
    public Button cardButton2;
    public Button cardButton3;

    [Header("Тексты карт")]
    public TextMeshProUGUI cardText1;
    public TextMeshProUGUI cardText2;
    public TextMeshProUGUI cardText3;

    private List<int> currentOptions;

    private string[] upgradeNames = new string[]
    {
        "ТОЧНОСТЬ",
        "СИЛА",
        "БРОНЯ",
        "ЗДОРОВЬЕ",
        "НЕУЛОВИМОСТЬ",
        "БРОНЕБОЙНОСТЬ АТАК"
    };

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        cardButton1.onClick.AddListener(() => OnCardSelected(0));
        cardButton2.onClick.AddListener(() => OnCardSelected(1));
        cardButton3.onClick.AddListener(() => OnCardSelected(2));
    }

    public void ShowLevelUpPanel(List<int> options)
    {
        currentOptions = options;

        if (cardText1 != null) cardText1.text = upgradeNames[options[0]];
        if (cardText2 != null) cardText2.text = upgradeNames[options[1]];
        if (cardText3 != null) cardText3.text = upgradeNames[options[2]];

        if (panel != null)
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void OnCardSelected(int buttonIndex)
    {
        if (currentOptions == null || buttonIndex >= currentOptions.Count) return;

        int exactUpgradeType = currentOptions[buttonIndex];
        PlayerXP.Instance.SelectUpgrade(exactUpgradeType);

        if (panel != null)
        {
            panel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
}
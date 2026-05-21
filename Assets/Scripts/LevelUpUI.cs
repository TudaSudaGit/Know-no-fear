using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;

    public Button cardButton1;
    public Button cardButton2;
    public Button cardButton3;

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

        SetButtonHoverColor(cardButton1, options[0]);
        SetButtonHoverColor(cardButton2, options[1]);
        SetButtonHoverColor(cardButton3, options[2]);

        if (panel != null)
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void SetButtonHoverColor(Button button, int upgradeIndex)
    {
        if (button == null) return;

        Color targetColor = Color.white;

        switch (upgradeIndex)
        {
            case 0:
                targetColor = new Color(1f, 0.92f, 0.016f);
                break;
            case 1:
                targetColor = new Color(0.85f, 0.1f, 0.1f);
                break;
            case 2:
                targetColor = new Color(0.35f, 0.75f, 1f);
                break;
            case 3:
                targetColor = new Color(0.1f, 0.75f, 0.1f);
                break;
            case 4:
                targetColor = new Color(0.6f, 0.2f, 0.8f);
                break;
            case 5:
                targetColor = new Color(0.05f, 0.15f, 0.6f);
                break;
        }

        ColorBlock cb = button.colors;
        cb.highlightedColor = targetColor;
        cb.selectedColor = targetColor;
        button.colors = cb;
    }

    void OnCardSelected(int index)
    {
        if (currentOptions != null && index < currentOptions.Count)
        {
            int upgradeIndex = currentOptions[index];
            if (PlayerXP.Instance != null)
            {
                PlayerXP.Instance.SelectUpgrade(upgradeIndex);
            }
        }
    }
}
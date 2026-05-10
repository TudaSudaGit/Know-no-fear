using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrosshairSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsMenu;
    public Image aimVisualImage;

    [Header("Crosshair Sprites")]
    public List<Sprite> crosshairSprites = new List<Sprite>();

    [Header("Button Container")]
    public Transform buttonContainer; // Content из ScrollView

    private int selectedIndex = 0;
    private const string SAVE_KEY = "SelectedCrosshair";

    // Цвета кнопок
    private Color colorSelected = new Color(1f, 0.8f, 0f, 1f);
    private Color colorNormal = new Color(0.15f, 0.15f, 0.15f, 1f);

    void Start()
    {
        selectedIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);
        BuildButtons();
        ApplyCrosshair(selectedIndex);
        settingsMenu.SetActive(false);
    }

    private bool menuJustClosed = false;

    void Update()
    {
        if (menuJustClosed)
        {
            menuJustClosed = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isOpen = settingsMenu.activeSelf;
            settingsMenu.SetActive(!isOpen);
            Time.timeScale = isOpen ? 1f : 0f;
        }
    }

    public void CloseMenu()
    {
        settingsMenu.SetActive(false);
        menuJustClosed = true;
        Time.timeScale = 1f;
    }

    void BuildButtons()
    {
        // Удалить все старые дочерние объекты в Content
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
            Destroy(buttonContainer.GetChild(i).gameObject);

        for (int i = 0; i < crosshairSprites.Count; i++)
        {
            int idx = i;

            // --- Создаём кнопку вручную без Prefab ---

            // Корневой объект кнопки
            GameObject btnObj = new GameObject("Btn_" + i);
            btnObj.transform.SetParent(buttonContainer, false);

            // Размер ячейки
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 120);

            // Фон кнопки
            Image bgImage = btnObj.AddComponent<Image>();
            bgImage.color = (i == selectedIndex) ? colorSelected : colorNormal;

            // Компонент Button
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            // Дочерний Image для спрайта прицела
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            // Растягиваем на весь квадрат с отступом 10px
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(10, 10);
            iconRt.offsetMax = new Vector2(-10, -10);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = crosshairSprites[i];
            iconImage.preserveAspect = true;

            // Клик
            btn.onClick.AddListener(() => OnSelectCrosshair(idx));
        }
    }

    void OnSelectCrosshair(int idx)
    {
        selectedIndex = idx;
        ApplyCrosshair(idx);
        PlayerPrefs.SetInt(SAVE_KEY, idx);
        PlayerPrefs.Save();
        RefreshHighlights();
    }

    void ApplyCrosshair(int idx)
    {
        if (idx >= 0 && idx < crosshairSprites.Count)
            aimVisualImage.sprite = crosshairSprites[idx];
    }

    void RefreshHighlights()
    {
        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            Image bg = buttonContainer.GetChild(i).GetComponent<Image>();
            if (bg != null)
                bg.color = (i == selectedIndex) ? colorSelected : colorNormal;
        }
    }

}
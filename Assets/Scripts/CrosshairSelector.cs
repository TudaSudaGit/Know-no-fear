using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrosshairSelector : MonoBehaviour
{
    public GameObject settingsMenu;
    public Image aimVisualImage;
    public List<Sprite> crosshairSprites = new List<Sprite>();
    public Transform buttonContainer;
    public Slider sizeSlider;

    private int selectedIndex = 0;
    private const string SAVE_KEY = "SelectedCrosshair";

    private Color colorSelected = new Color(1f, 0.8f, 0f, 1f);
    private Color colorNormal = new Color(0.15f, 0.15f, 0.15f, 1f);

    void Start()
    {
        selectedIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);
        BuildButtons();
        ApplyCrosshair(selectedIndex);

        if (sizeSlider != null && aimVisualImage != null)
        {
            float defaultSize = sizeSlider.value;
            aimVisualImage.rectTransform.sizeDelta = new Vector2(defaultSize, defaultSize);
        }

        settingsMenu.SetActive(false);
    }

    public void CloseMenu()
    {
        settingsMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    void BuildButtons()
    {
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
            Destroy(buttonContainer.GetChild(i).gameObject);

        for (int i = 0; i < crosshairSprites.Count; i++)
        {
            int idx = i;

            GameObject btnObj = new GameObject("Btn_" + i);
            btnObj.transform.SetParent(buttonContainer, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 120);

            Image bgImage = btnObj.AddComponent<Image>();
            bgImage.color = (i == selectedIndex) ? colorSelected : colorNormal;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(10, 10);
            iconRt.offsetMax = new Vector2(-10, -10);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = crosshairSprites[i];
            iconImage.preserveAspect = true;

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

    public void AdjustCrosshair(float size)
    {
        if (aimVisualImage != null)
            aimVisualImage.rectTransform.sizeDelta = new Vector2(size, size);
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
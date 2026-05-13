using UnityEngine;
using UnityEngine.UI;

public class CrosshairColor : MonoBehaviour
{
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color targetColor = Color.red;
    public float changeSpeed = 5f;

    private bool isOverEnemy = false;
    private Color currentColor;

    void Start()
    {
        if (crosshairImage != null)
        {
            currentColor = normalColor;
        }
    }

    void Update()
    {
        Color desiredColor = isOverEnemy ? targetColor : normalColor;

        currentColor = Color.Lerp(currentColor, desiredColor, Time.deltaTime * changeSpeed);

        if (crosshairImage != null && crosshairImage.material != null)
        {
            crosshairImage.material.SetColor("_Color", currentColor);
        }

        isOverEnemy = false;
    }

    public void SetOverEnemy()
    {
        isOverEnemy = true;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject settingsMenu; // Сюда перетащи панель SettingsMenu
    public Image crosshairImage;   // Сюда перетащи объект прицела из Canvas
    private bool isMenuOpen = false;

    void Update()
    {
        // Отслеживаем нажатие Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        settingsMenu.SetActive(isMenuOpen);

        // Пауза времени: 0 - стоп, 1 - обычная скорость
        Time.timeScale = isMenuOpen ? 0f : 1f;

        // Показываем/скрываем стандартный курсор мыши для выбора в меню
        Cursor.visible = isMenuOpen;
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Confined;
    }

    // Метод для кнопок: меняет спрайт прицела на новый
    public void SetCrosshair(Sprite newSprite)
    {
        if (crosshairImage != null)
        {
            crosshairImage.sprite = newSprite;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Панели и Корневые Объекты")]
    public GameObject gameplayRoot;
    public GameObject canvasPause;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject complexityPanel;
    public GameObject noSaveWarningPanel; // Окно-предупреждение "Нет сохранения"

    [Header("Ссылки на Компоненты")]
    public Button loadGameButton;
    public PauseMenuController pauseMenuController;

    [Header("Менеджеры Спавна")]
    public ArmorSpawnManager[] armorSpawners;
    public AmmoSpawnManager[] ammoSpawners;

    void Start()
    {
        // Гарантируем, что при запуске главного меню время ИДЕТ
        Time.timeScale = 1f;

        GameSettings.LoadOptions();

        // Кнопка загрузки всегда кликабельна, чтобы при отсутствии сейва показать попап
        if (loadGameButton != null)
        {
            loadGameButton.interactable = true;
        }

        ShowMainMenu();
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (canvasPause != null) canvasPause.SetActive(false);
    }

    public void PlayNewGame()
    {
        // 1. Сразу размораживаем время, чтобы исключить зависание
        Time.timeScale = 1f;
        GameSettings.IsGameSaved = false;

        // 2. Управляем видимостью интерфейса
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (complexityPanel != null) complexityPanel.SetActive(false);
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);

        if (canvasPause != null) canvasPause.SetActive(true);
        if (gameplayRoot != null) gameplayRoot.SetActive(true);

        // 3. Снимаем флаги паузы в контроллере
        if (pauseMenuController != null)
        {
            pauseMenuController.Resume();
        }

        // 4. Запускаем спавн объектов (в безопасном режиме)
        ProcessSpawners();
    }

    public void LoadGame()
    {
        // Проверка на наличие сохранения
        if (!PlayerPrefs.HasKey("SavedLevel"))
        {
            if (noSaveWarningPanel != null)
            {
                noSaveWarningPanel.SetActive(true);
            }
            Debug.LogWarning("Невозможно загрузить: файл сохранения 'SavedLevel' не найден!");
            return;
        }

        // 1. Размораживаем время СРАЗУ после успешной проверки сохранения
        Time.timeScale = 1f;
        GameSettings.IsGameSaved = true;

        // 2. Управляем видимостью интерфейса
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (complexityPanel != null) complexityPanel.SetActive(false);
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);

        if (canvasPause != null) canvasPause.SetActive(true);
        if (gameplayRoot != null) gameplayRoot.SetActive(true);

        // 3. Принудительно снимаем игру с паузы
        if (pauseMenuController != null)
        {
            pauseMenuController.Resume();
        }

        // 4. Запускаем спавн объектов
        ProcessSpawners();
    }

    // Метод для закрытия окна предупреждения (привязать к кнопке "ОК"/"Закрыть" на панели)
    public void CloseWarningPanel()
    {
        if (noSaveWarningPanel != null)
        {
            noSaveWarningPanel.SetActive(false);
        }
    }

    // Безопасный вызов спавнеров, защищающий основной поток от NullReferenceException
    private void ProcessSpawners()
    {
        try
        {
            if (armorSpawners != null)
            {
                foreach (ArmorSpawnManager spawner in armorSpawners)
                {
                    if (spawner != null) spawner.SpawnArmorObjects();
                }
            }

            if (ammoSpawners != null)
            {
                foreach (AmmoSpawnManager spawner in ammoSpawners)
                {
                    if (spawner != null) spawner.SpawnAmmoObjects();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Произошла ошибка при спавне объектов, но работа игры и время не нарушены: {e.Message}");
        }
    }

    public void SetDifficulty(int difficultyIndex)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.ChangeDifficulty(difficultyIndex);
        }
    }

    public void ShowSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (complexityPanel != null) complexityPanel.SetActive(false);
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    public void ShowComplexity()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (complexityPanel != null) complexityPanel.SetActive(true);
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (complexityPanel != null) complexityPanel.SetActive(false);
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }
}
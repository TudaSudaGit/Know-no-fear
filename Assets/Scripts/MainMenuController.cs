using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject gameplayRoot;
    public GameObject canvasPause;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public Button loadGameButton;
    public PauseMenuController pauseMenuController;
    public ArmorSpawnManager[] armorSpawners;
    public AmmoSpawnManager[] ammoSpawners;

    void Start()
    {
        GameSettings.LoadOptions();
        if (loadGameButton != null)
        {
            loadGameButton.interactable = PlayerPrefs.HasKey("SavedLevel");
        }
        ShowMainMenu();
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (canvasPause != null) canvasPause.SetActive(false);
    }

    public void PlayNewGame()
    {
        GameSettings.IsGameSaved = false;

        Debug.Log($"[MainMenu] Нажата Новая Игра. В списке брони: {armorSpawners?.Length ?? 0} эл., в списке патрон: {ammoSpawners?.Length ?? 0} эл.");

        if (armorSpawners != null)
        {
            foreach (ArmorSpawnManager spawner in armorSpawners)
            {
                if (spawner != null)
                {
                    Debug.Log($"[MainMenu] Запускаю спавн БРОНИ на объекте: {spawner.gameObject.name}");
                    spawner.SpawnArmorObjects();
                }
                else
                {
                    Debug.LogWarning("[MainMenu] Предупреждение: В списке armorSpawners есть пустой слот (None)!");
                }
            }
        }

        if (ammoSpawners != null)
        {
            foreach (AmmoSpawnManager spawner in ammoSpawners)
            {
                if (spawner != null)
                {
                    Debug.Log($"[MainMenu] Запускаю спавн ПАТРОН на объекте: {spawner.gameObject.name}");
                    spawner.SpawnAmmoObjects();
                }
                else
                {
                    Debug.LogWarning("[MainMenu] Предупреждение: В списке ammoSpawners есть пустой слот (None)!");
                }
            }
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (canvasPause != null) canvasPause.SetActive(true);
        if (pauseMenuController != null)
        {
            pauseMenuController.Resume();
        }
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
        Time.timeScale = 1f;
    }

    public void LoadGame()
    {
        GameSettings.IsGameSaved = true;

        Debug.Log($"[MainMenu] Загрузка игры. В списке брони: {armorSpawners?.Length ?? 0} эл., в списке патрон: {ammoSpawners?.Length ?? 0} эл.");

        if (armorSpawners != null)
        {
            foreach (ArmorSpawnManager spawner in armorSpawners)
            {
                if (spawner != null)
                {
                    spawner.SpawnArmorObjects();
                }
            }
        }

        if (ammoSpawners != null)
        {
            foreach (AmmoSpawnManager spawner in ammoSpawners)
            {
                if (spawner != null)
                {
                    spawner.SpawnAmmoObjects();
                }
            }
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (canvasPause != null) canvasPause.SetActive(true);
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
        if (pauseMenuController != null)
        {
            pauseMenuController.Pause();
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
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject gameplayRoot, canvasMenu, pausePanel, confirmationPanel, crosshairSettingsPanel;
    public MainMenuController mainMenuController;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (crosshairSettingsPanel != null && crosshairSettingsPanel.activeSelf)
                {
                    crosshairSettingsPanel.SetActive(false);
                    pausePanel.SetActive(true);
                }
                else if (confirmationPanel != null && confirmationPanel.activeSelf)
                {
                    confirmationPanel.SetActive(false);
                    pausePanel.SetActive(true);
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        confirmationPanel.SetActive(false);
        if (crosshairSettingsPanel != null) crosshairSettingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenCrosshairMenu()
    {
        pausePanel.SetActive(false);
        if (crosshairSettingsPanel != null) crosshairSettingsPanel.SetActive(true);
    }

    public void CloseCrosshairMenu()
    {
        if (crosshairSettingsPanel != null) crosshairSettingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void SaveGame()
    {
        if (PlayerXP.Instance != null)
        {
            PlayerPrefs.SetInt("SavedLevel", PlayerXP.Instance.currentLevel);
            PlayerPrefs.SetInt("SavedXP", PlayerXP.Instance.currentXP);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        }
        if (WeaponController.Instance != null) WeaponController.Instance.SaveAmmoData();
        if (player != null)
        {
            UnitStats playerStats = player.GetComponent<UnitStats>();
            if (playerStats != null) PlayerPrefs.SetInt("SavedPlayerArmor", playerStats.armorPoints);
        }
        AmmoSpawnManager ammoASM = Object.FindAnyObjectByType<AmmoSpawnManager>();
        if (ammoASM != null) ammoASM.SaveAmmoState();
        ArmorSpawnManager armorASM = Object.FindAnyObjectByType<ArmorSpawnManager>();
        if (armorASM != null) armorASM.SaveArmorState();
        PlayerPrefs.Save();
        GameSettings.IsGameSaved = true;
    }

    public void AttemptExit()
    {
        if (GameSettings.IsGameSaved) ConfirmExit();
        else
        {
            pausePanel.SetActive(false);
            confirmationPanel.SetActive(true);
        }
    }

    public void ConfirmExit()
    {
        if (PlayerCurseHandler.Instance != null)
        {
            PlayerCurseHandler.Instance.ClearCurses();
        }

        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(0);
    }
}
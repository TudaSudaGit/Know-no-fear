using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject gameplayRoot;
    public GameObject canvasMenu;
    public MainMenuController mainMenuController;
    public GameObject pausePanel;
    public GameObject confirmationPanel;
    public GameObject crosshairSettingsPanel;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
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
        PlayerPrefs.Save();
        GameSettings.IsGameSaved = true;
    }

    public void AttemptExit()
    {
        if (GameSettings.IsGameSaved)
        {
            ConfirmExit();
        }
        else
        {
            pausePanel.SetActive(false);
            confirmationPanel.SetActive(true);
        }
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CancelExit()
    {
        confirmationPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
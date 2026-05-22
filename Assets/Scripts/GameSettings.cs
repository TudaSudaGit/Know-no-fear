using UnityEngine;
using TMPro;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public static int Difficulty = 1;
    public static float CrosshairSize = 1f;
    public static string BindLeft = "A";
    public static string BindRight = "D";
    public static string BindInteract = "E";
    public static bool IsGameSaved = false;

    public TextMeshProUGUI difficultyText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadOptions();
        UpdateDifficultyText();
    }

    public static void SaveOptions()
    {
        PlayerPrefs.SetInt("Difficulty", Difficulty);
        PlayerPrefs.SetFloat("CrosshairSize", CrosshairSize);
        PlayerPrefs.SetString("BindLeft", BindLeft);
        PlayerPrefs.SetString("BindRight", BindRight);
        PlayerPrefs.SetString("BindInteract", BindInteract);
        PlayerPrefs.Save();
    }

    public static void LoadOptions()
    {
        Difficulty = PlayerPrefs.GetInt("Difficulty", 1);
        CrosshairSize = PlayerPrefs.GetFloat("CrosshairSize", 1f);
        BindLeft = PlayerPrefs.GetString("BindLeft", "A");
        BindRight = PlayerPrefs.GetString("BindRight", "D");
        BindInteract = PlayerPrefs.GetString("BindInteract", "E");
    }

    public void ChangeDifficulty(int level)
    {
        Difficulty = level;
        SaveOptions();
        UpdateDifficultyText();
    }

    public void UpdateDifficultyText()
    {
        if (difficultyText != null)
        {
            if (Difficulty == 0) difficultyText.text = "Сложность: Легко";
            else if (Difficulty == 1) difficultyText.text = "Сложность: Средне";
            else if (Difficulty == 2) difficultyText.text = "Сложность: Хард";
        }
    }
}
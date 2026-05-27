using UnityEngine;

public class DifficultyModifier : MonoBehaviour
{
    public bool isPlayer = false;
    private UnitStats stats;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
    }

    void OnEnable() { ApplyDifficultySettings(); }

    public void ApplyDifficultySettings()
    {
        GameSettings.LoadOptions();
        if (stats == null) return;

        if (isPlayer)
        {
            if (GameSettings.Difficulty == 0) stats.armorPoints = 5;
            else if (GameSettings.Difficulty == 1) stats.armorPoints = 3;
            else if (GameSettings.Difficulty == 2) stats.armorPoints = 1;

            if (WeaponController.Instance != null)
            {
                WeaponController.Instance.UpdateAmmoDifficulty();
            }
        }
    }

    void Start()
    {
        if (isPlayer)
        {
            if (GameSettings.IsGameSaved && PlayerPrefs.HasKey("SavedPlayerArmor") && stats != null)
                stats.armorPoints = PlayerPrefs.GetInt("SavedPlayerArmor");
            if (GameSettings.IsGameSaved && PlayerPrefs.HasKey("PlayerX"))
            {
                transform.position = new Vector3(PlayerPrefs.GetFloat("PlayerX"), PlayerPrefs.GetFloat("PlayerY"), transform.position.z);
                if (PlayerXP.Instance != null)
                {
                    PlayerXP.Instance.currentLevel = PlayerPrefs.GetInt("SavedLevel", 1);
                    PlayerXP.Instance.currentXP = PlayerPrefs.GetInt("SavedXP", 0);
                }
            }
        }
    }
}
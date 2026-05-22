using UnityEngine;

public class DifficultyModifier : MonoBehaviour
{
    public bool isPlayer = false;

    void Awake()
    {
        GameSettings.LoadOptions();
        UnitStats stats = GetComponent<UnitStats>();

        if (!isPlayer && stats != null)
        {
            if (GameSettings.Difficulty == 0)
            {
                stats.ballisticSkill = Mathf.Max(2, stats.ballisticSkill - 1);
                stats.weaponSkill = Mathf.Max(2, stats.weaponSkill - 1);
                stats.strength = Mathf.Max(1, stats.strength - 1);
                stats.damage = Mathf.Max(1, stats.damage - 1);
                stats.toughness = Mathf.Max(1, stats.toughness - 1);
                stats.wounds = Mathf.Max(1, stats.wounds - 1);
            }
            if (GameSettings.Difficulty == 2)
            {
                stats.ballisticSkill = Mathf.Min(7, stats.ballisticSkill + 1);
                stats.weaponSkill = Mathf.Min(7, stats.weaponSkill + 1);
                stats.strength += 1;
                stats.damage += 1;
                stats.toughness += 1;
                stats.wounds += 1;
            }
        }

        if (isPlayer && stats != null)
        {
            if (GameSettings.Difficulty == 0)
            {
                stats.wounds += 1;
            }
            if (GameSettings.Difficulty == 2)
            {
                stats.wounds = Mathf.Max(1, stats.wounds - 1);
            }
        }
    }

    void Start()
    {
        if (isPlayer)
        {
            if (GameSettings.Difficulty == 0)
            {
                if (PlayerXP.Instance != null)
                {
                    PlayerXP.Instance.maxHealthBonus += 1;
                }
            }

            if (GameSettings.IsGameSaved && PlayerPrefs.HasKey("PlayerX"))
            {
                transform.position = new Vector3(
                    PlayerPrefs.GetFloat("PlayerX"),
                    PlayerPrefs.GetFloat("PlayerY"),
                    transform.position.z
                );
                if (PlayerXP.Instance != null)
                {
                    PlayerXP.Instance.currentLevel = PlayerPrefs.GetInt("SavedLevel", 1);
                    PlayerXP.Instance.currentXP = PlayerPrefs.GetInt("SavedXP", 0);
                }
            }
        }
    }
}
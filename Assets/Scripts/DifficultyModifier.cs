using UnityEngine;

public class DifficultyModifier : MonoBehaviour
{
    public bool isPlayer = false;

    private int baseBallisticSkill;
    private int baseWeaponSkill;
    private int baseStrength;
    private int baseDamage;
    private int baseToughness;
    private int baseWounds;

    private UnitStats stats;
    private bool isBaseSaved = false;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
        SaveOriginalStats();
    }

    void OnEnable()
    {
        ApplyDifficultySettings();
    }

    void SaveOriginalStats()
    {
        if (stats == null || isBaseSaved) return;

        baseBallisticSkill = stats.ballisticSkill;
        baseWeaponSkill = stats.weaponSkill;
        baseStrength = stats.strength;
        baseDamage = stats.damage;
        baseToughness = stats.toughness;
        baseWounds = stats.wounds;

        isBaseSaved = true;
    }

    public void ApplyDifficultySettings()
    {
        GameSettings.LoadOptions();
        if (stats == null) return;

        stats.ballisticSkill = baseBallisticSkill;
        stats.weaponSkill = baseWeaponSkill;
        stats.strength = baseStrength;
        stats.damage = baseDamage;
        stats.toughness = baseToughness;
        stats.wounds = baseWounds;

        if (!isPlayer)
        {
            if (GameSettings.Difficulty == 0)
            {
                stats.ballisticSkill = Mathf.Max(2, baseBallisticSkill - 1);
                stats.weaponSkill = Mathf.Max(2, baseWeaponSkill - 1);
                stats.strength = Mathf.Max(1, baseStrength - 1);
                stats.damage = Mathf.Max(1, baseDamage - 1);
                stats.toughness = Mathf.Max(1, baseToughness - 1);
                stats.wounds = Mathf.Max(1, baseWounds - 1);
            }
            if (GameSettings.Difficulty == 2) // Сложно
            {
                stats.ballisticSkill = Mathf.Min(7, baseBallisticSkill + 1);
                stats.weaponSkill = Mathf.Min(7, baseWeaponSkill + 1);
                stats.strength += 1;
                stats.damage += 1;
                stats.toughness += 1;
                stats.wounds += 1;
            }
        }
        else
        {
            if (GameSettings.Difficulty == 0)
            {
                stats.wounds += 1;
                if (PlayerXP.Instance != null)
                {
                    PlayerXP.Instance.maxHealthBonus = 1;
                }
            }
            if (GameSettings.Difficulty == 2)
            {
                stats.wounds = Mathf.Max(1, stats.wounds - 1);
            }

            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.maxHealth = stats.wounds;
                health.currentHealth = health.maxHealth;
                health.ApplyDamage(0);
            }
        }
    }

    void Start()
    {
        if (isPlayer)
        {
            if (GameSettings.IsGameSaved && PlayerPrefs.HasKey("SavedPlayerArmor"))
            {
                if (stats != null)
                {
                    stats.armorPoints = PlayerPrefs.GetInt("SavedPlayerArmor");
                }
            }
            if (GameSettings.Difficulty == 0 && PlayerXP.Instance != null)
            {
                PlayerXP.Instance.maxHealthBonus = 1;
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
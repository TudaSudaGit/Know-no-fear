using UnityEngine;
using System.Collections.Generic;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance { get; private set; }

    public int currentLevel = 1;
    public int currentXP = 0;
    public int baseXPToLevel = 100;
    public float xpExponent = 1.5f;

    public LevelUpUI levelUpUI;

    public int accuracy = 0;
    public int strength = 0;
    public int armor = 0;
    public int maxHealthBonus = 0;
    public int elusiveness = 0;
    public int armorPenetration = 0;

    private UnitStats playerStats;
    private Health playerHealth;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerStats = GetComponent<UnitStats>();
        playerHealth = GetComponent<Health>();
    }

    public int GetXPRequiredForNextLevel()
    {
        return Mathf.RoundToInt(baseXPToLevel * Mathf.Pow(currentLevel, xpExponent));
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= GetXPRequiredForNextLevel())
        {
            currentXP -= GetXPRequiredForNextLevel();
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        if (levelUpUI != null)
        {
            List<int> pool = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> selectedUpgrades = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                selectedUpgrades.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex);
            }

            levelUpUI.ShowLevelUpPanel(selectedUpgrades);
        }
    }

    public void SelectUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                accuracy++;
                if (playerStats != null)
                {
                    playerStats.ballisticSkill = Mathf.Max(2, playerStats.ballisticSkill - 1);
                    playerStats.weaponSkill = Mathf.Max(2, playerStats.weaponSkill - 1);
                }
                break;

            case 1:
                strength++;
                if (playerStats != null)
                {
                    playerStats.strength += 1;
                }
                break;

            case 2:
                armor++;
                if (playerStats != null)
                {
                    playerStats.save = Mathf.Max(2, playerStats.save - 1);
                }
                break;

            case 3:
                maxHealthBonus++;
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += 1;
                    playerHealth.currentHealth += 1;
                    playerHealth.ApplyDamage(0);
                }
                break;

            case 4:
                elusiveness++;
                break;

            case 5:
                armorPenetration++;
                if (playerStats != null)
                {
                    playerStats.armorPenetration += 1;
                }
                break;
        }

        if (TutorialManager.Instance != null) TutorialManager.Instance.OnUpgradeSelected();

        if (levelUpUI != null && levelUpUI.panel != null)
        {
            levelUpUI.panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
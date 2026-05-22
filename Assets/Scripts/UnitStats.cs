using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Навыки")]
    [Range(2, 7)] public int ballisticSkill = 4;
    [Range(2, 7)] public int weaponSkill = 4;

    [Header("Оружие")]
    public int strength = 4;
    public int damage = 1;
    public int armorPenetration = 0;

    [Header("Защита")]
    public int toughness = 4;
    [Range(2, 7)] public int save = 5;
    public int wounds = 5;

    public int armorPoints = 0;

    public void TakeDamage(int amount)
    {
        if (armorPoints > 0)
        {
            if (amount <= armorPoints)
            {
                armorPoints -= amount;
                return;
            }
            else
            {
                amount -= armorPoints;
                armorPoints = 0;
            }
        }
        wounds -= amount;
    }
}
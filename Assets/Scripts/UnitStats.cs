using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Навыки (нужно X+ на D6)")]
    [Range(2, 7)] public int ballisticSkill = 4;
    [Range(2, 7)] public int weaponSkill    = 4;

    [Header("Оружие")]
    public int strength         = 4;
    public int damage           = 1;
    public int armorPenetration = 0;   // положительное число: AP-2 → пишем 2

    [Header("Защита")]
    public int toughness             = 4;
    [Range(2, 7)] public int save    = 5;
    public int wounds                = 5;
}

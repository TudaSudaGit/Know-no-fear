using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    public static bool IsTutorialCombatActive = false;
    private bool hasBeenHit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenHit) return;
        if (!other.CompareTag("Bullet") && !other.name.Contains("Bullet")) return;

        IsTutorialCombatActive = true;
        hasBeenHit = true;

        UnitStats myStats = GetComponent<UnitStats>();
        Health myHealth = GetComponent<Health>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        DiceRollPanel.Request(new DiceRollPanel.CombatRequest
        {
            attacker = player.GetComponent<UnitStats>(),
            defender = myStats,
            defenderHealth = myHealth,
            attackerIsPlayer = true,
            isMelee = false,
            isForced = true,
            forcedHit = 6,
            forcedWound = 4,
            forcedSave = 1
        });

        GetComponent<EnemyXP>()?.DropXP();

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TargetHit();
        }

        Destroy(other.gameObject);
        Invoke("ResetLock", 0.5f);
    }

    void ResetLock()
    {
        IsTutorialCombatActive = false;
    }
}
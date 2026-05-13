using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public UnitStats attackerStats;

    void Start()
    {
        Destroy(gameObject, 10f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player")) { Destroy(gameObject); return; }

        Health targetHealth = hitInfo.GetComponent<Health>()
                           ?? hitInfo.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            UnitStats targetStats = hitInfo.GetComponent<UnitStats>()
                                 ?? hitInfo.GetComponentInParent<UnitStats>();

            DiceRollPanel.Request(new DiceRollPanel.CombatRequest
            {
                attacker         = attackerStats,
                defender         = targetStats,
                defenderHealth   = targetHealth,
                attackerIsPlayer = true,
                isMelee          = false
            });
        }

        Destroy(gameObject);
    }
}

using UnityEngine;

public class LaserShot : MonoBehaviour
{
    public float speed    = 15f;
    public int   damage   = 1;
    public float lifetime = 8f;

    [HideInInspector] public UnitStats attackerStats;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        Health targetHealth = hitInfo.GetComponentInParent<Health>()
                           ?? hitInfo.GetComponent<Health>();
        UnitStats targetStats = hitInfo.GetComponentInParent<UnitStats>()
                             ?? hitInfo.GetComponent<UnitStats>();

        if (targetHealth != null && targetHealth.gameObject.CompareTag("Player"))
        {
            if (DiceRollPanel.Instance != null && attackerStats != null && targetStats != null)
            {
                DiceRollPanel.Instance.RequestCombat(new DiceRollPanel.CombatRequest
                {
                    attacker         = attackerStats,
                    defender         = targetStats,
                    defenderHealth   = targetHealth,
                    attackerIsPlayer = false,
                    isMelee          = false
                });
            }
            else
            {
                targetHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}

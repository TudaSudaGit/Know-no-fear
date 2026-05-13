using UnityEngine;

public class TankEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public Transform uiContainer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f && !isAttacking)
            {
                StartAttack();
                attackTimer = attackCooldown;
            }
        }
        else if (!isAttacking)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
            animator.SetFloat("Speed", Mathf.Abs(dir.x));
        }

        float sx = player.position.x < transform.position.x ? -1.8f : 1.8f;
        transform.localScale = new Vector3(sx, 1.8f, 1f);

        FixUI();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isAttacking) return;

        Health targetHealth = other.GetComponentInParent<Health>()
                           ?? other.GetComponent<Health>();

        if (targetHealth == null || !targetHealth.gameObject.CompareTag("Player")) return;

        UnitStats myStats = GetComponent<UnitStats>();
        UnitStats targetStats = other.GetComponentInParent<UnitStats>()
                             ?? other.GetComponent<UnitStats>();

        int atkCount = myStats != null ? myStats.attacks : 1;
        ExecuteCombat(atkCount, myStats, targetStats, targetHealth);

        isAttacking = false;
    }

    void ExecuteCombat(int count, UnitStats attacker, UnitStats defender, Health hp)
    {
        if (count <= 0) return;

        DiceRollPanel.Request(new DiceRollPanel.CombatRequest
        {
            attacker = attacker,
            defender = defender,
            defenderHealth = hp,
            attackerIsPlayer = false,
            isMelee = true
        });

        ExecuteCombat(count - 1, attacker, defender, hp);
    }

    void StartAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsAttacking", true);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    void FixUI()
    {
        if (uiContainer != null)
        {
            Vector3 ls = uiContainer.localScale;
            ls.x = Mathf.Abs(ls.x) * Mathf.Sign(transform.localScale.x);
            uiContainer.localScale = ls;
        }
    }
}
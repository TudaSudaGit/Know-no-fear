using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float shootRange = 6f;
    public float retreatRange = 3f;
    public Transform uiContainer;
    public Transform firePoint;
    public float shootCooldown = 2f;

    private float shootTimer = 0f;
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        UpdateCooldown();
        HandleState(dist);
        FlipToPlayer();
        FixUI();
    }

    void UpdateCooldown()
    {
        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
    }

    void HandleState(float dist)
    {
        if (dist <= retreatRange) return;
        else if (dist <= shootRange) StateAttack();
    }

    void StateAttack()
    {
        if (shootTimer <= 0f)
        {
            shootTimer = shootCooldown;
            ApplyCombatDamage();
        }
    }

    void ApplyCombatDamage()
    {
        UnitStats myStats = GetComponent<UnitStats>();
        UnitStats targetStats = player.GetComponentInParent<UnitStats>() ?? player.GetComponent<UnitStats>();

        DiceRollPanel.Request(new DiceRollPanel.CombatRequest
        {
            attacker = myStats,
            defender = targetStats,
            attackerIsPlayer = false,
            isMelee = false
        });
    }

    void FlipToPlayer()
    {
        float sx = player.position.x < transform.position.x ? -0.9f : 0.9f;
        transform.localScale = new Vector3(sx, 0.9f, 1f);
    }

    void FixUI()
    {
        if (uiContainer == null) return;
        Vector3 ls = uiContainer.localScale;
        ls.x = Mathf.Abs(ls.x) * Mathf.Sign(transform.localScale.x);
        uiContainer.localScale = ls;
    }
}
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
        FlipSprite();
        FixUI();
    }

    void UpdateCooldown()
    {
        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
    }

    void HandleState(float dist)
    {
        if (dist <= retreatRange) StateRetreat();
        else if (dist <= shootRange) StateAttack();
        else if (dist <= detectionRange) StateApproach();
        else StateIdle();
    }

    void StateRetreat()
    {
        Vector2 dir = (transform.position - player.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", Mathf.Abs(dir.x));
    }

    void StateApproach()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", Mathf.Abs(dir.x));
    }

    void StateIdle()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);
    }

    void StateAttack()
    {
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetFloat("Speed", 0f);

        if (shootTimer <= 0f)
        {
            shootTimer = shootCooldown;
            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    public void SpawnBullet()
    {
        if (player == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        GameObject laserObj = new GameObject("Laser");
        LaserBeam beam = laserObj.AddComponent<LaserBeam>();
        beam.Fire(origin, player.position);

        ApplyCombatDamage();
    }

    void ApplyCombatDamage()
    {
        UnitStats myStats = GetComponent<UnitStats>();
        UnitStats targetStats = player.GetComponentInParent<UnitStats>() ?? player.GetComponent<UnitStats>();
        Health targetHealth = player.GetComponentInParent<Health>() ?? player.GetComponent<Health>();

        DiceRollPanel.Request(new DiceRollPanel.CombatRequest
        {
            attacker = myStats,
            defender = targetStats,
            defenderHealth = targetHealth,
            attackerIsPlayer = false,
            isMelee = false
        });
    }

    void FlipSprite()
    {
        float vx = rb.linearVelocity.x;
        float faceDir = (Mathf.Abs(vx) > 0.05f)
            ? (vx > 0 ? 0.9f : -0.9f)
            : (player.position.x > transform.position.x ? 0.9f : -0.9f);

        transform.localScale = new Vector3(faceDir, 0.9f, 1f);
    }

    void FixUI()
    {
        if (uiContainer == null) return;
        Vector3 ls = uiContainer.localScale;
        ls.x = Mathf.Abs(ls.x) * Mathf.Sign(transform.localScale.x);
        uiContainer.localScale = ls;
    }
}
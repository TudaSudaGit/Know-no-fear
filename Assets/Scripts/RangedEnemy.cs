using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed      = 2f;
    public float detectionRange = 10f;
    public float shootRange     = 6f;
    public float retreatRange   = 3f;

    [Header("Стрельба")]
    public Transform firePoint;
    public float shootCooldown  = 2f;
    private float shootTimer    = 0f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        shootTimer -= Time.deltaTime;

        if (dist <= retreatRange)
        {
            Retreat();
        }
        else if (dist <= shootRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("Speed", 0f);
            if (shootTimer <= 0f) { Shoot(); shootTimer = shootCooldown; }
        }
        else if (dist <= detectionRange)
        {
            MoveToPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("Speed", 0f);
        }

        FlipSprite();
    }

    void MoveToPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("Speed", Mathf.Abs(dir.x));
    }

    void Retreat()
    {
        Vector2 dir = (transform.position - player.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("Speed", Mathf.Abs(dir.x));
    }

    void Shoot()
    {
        if (firePoint == null || player == null) return;
        animator.SetTrigger("Attack");
        shootTimer = shootCooldown;
    }

    public void SpawnBullet()
    {
        if (player == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        GameObject laserObj = new GameObject("Laser");
        LaserBeam beam = laserObj.AddComponent<LaserBeam>();
        beam.Fire(origin, player.position);

        UnitStats myStats     = GetComponent<UnitStats>();
        Health playerHealth   = player.GetComponentInParent<Health>() ?? player.GetComponent<Health>();
        UnitStats playerStats = player.GetComponentInParent<UnitStats>() ?? player.GetComponent<UnitStats>();

        int atkCount = myStats != null ? myStats.attacks : 1;

        for (int i = 0; i < atkCount; i++)
        {
            DiceRollPanel.Request(new DiceRollPanel.CombatRequest
            {
                attacker         = myStats,
                defender         = playerStats,
                defenderHealth   = playerHealth,
                attackerIsPlayer = false,
                isMelee          = false
            });
        }
    }

    void FlipSprite()
    {
        float sx = player.position.x < transform.position.x ? -0.9f : 0.9f;
        transform.localScale = new Vector3(sx, 0.9f, 1f);
    }
}

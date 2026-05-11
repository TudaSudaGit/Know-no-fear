using UnityEngine;

public class TankEnemy : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;

    [Header("Атака")]
    public float attackCooldown = 1.5f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        else
        {
         
            if (!isAttacking)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
                animator.SetFloat("Speed", Mathf.Abs(dir.x));
            }
        }

        spriteRenderer.flipX = player.position.x < transform.position.x;
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
}
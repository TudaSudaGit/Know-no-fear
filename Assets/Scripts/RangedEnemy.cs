using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float shootRange = 6f;
    public float retreatRange = 3f;

    [Header("Стрельба")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public float shootCooldown = 2f;
    private float shootTimer = 0f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
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

            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
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
        Debug.Log("Shoot вызван, bulletPrefab = " + bulletPrefab);
        if (bulletPrefab == null || firePoint == null) return;

        animator.SetTrigger("Attack");

       
        GameObject laser = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 dir = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        laser.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FlipSprite()
    {
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }
}
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifetime = 2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Health playerHealth = hitInfo.GetComponentInParent<Health>();

        if (playerHealth != null && hitInfo.CompareTag("Player"))
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
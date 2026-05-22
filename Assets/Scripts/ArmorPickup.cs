using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    public int armorAmount = 5;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isLanded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Setup(int amount)
    {
        armorAmount = amount;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLanded && !collision.gameObject.CompareTag("Player"))
        {
            isLanded = true;
            rb.bodyType = RigidbodyType2D.Static;
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnitStats stats = other.GetComponentInParent<UnitStats>() ?? other.GetComponent<UnitStats>();

            if (stats != null)
            {
                stats.armorPoints += armorAmount;
                Destroy(gameObject);
            }
        }
    }
}
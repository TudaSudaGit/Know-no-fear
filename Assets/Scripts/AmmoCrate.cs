using UnityEngine;

public class AmmoCrate : MonoBehaviour
{
    private int ammoAmount;
    private bool isPlayerInside = false;
    private WeaponController playerWeapon;
    private GameObject interactionHint;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isLanded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Setup(int amount, GameObject hint)
    {
        ammoAmount = amount;
        interactionHint = hint;
    }

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && playerWeapon != null)
        {
            playerWeapon.AddReserveAmmo(ammoAmount);
            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
            Destroy(gameObject);
        }
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
            isPlayerInside = true;
            playerWeapon = other.GetComponentInChildren<WeaponController>() ?? other.GetComponent<WeaponController>();
            if (interactionHint != null)
            {
                interactionHint.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerWeapon = null;
            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }
    }
}
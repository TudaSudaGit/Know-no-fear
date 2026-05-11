using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Health targetHealth = hitInfo.GetComponent<Health>();

        if (targetHealth != null && !hitInfo.CompareTag("Player"))
        {
            targetHealth.TakeDamage(1);
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        Destroy(gameObject, 10f);
    }
}
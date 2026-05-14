using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Настройки пули")]
    public GameObject bulletPrefab;
    public float bulletForce = 20f;

    [Header("Точки направления оружия")]
    public Transform firePointStart;
    public Transform firePointEnd;

    [Header("Настройки гильз")]
    public GameObject shellPrefab;
    public Transform shellPoint;
    public Transform shellTarget;
    public float shellForce = 5f;

    void Update()
    {
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
        {
            Shoot();
            EjectShell();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePointEnd.position, firePointEnd.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (firePointEnd.position - firePointStart.position).normalized;
            rb.AddForce(dir * bulletForce, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void EjectShell()
    {
        if (shellPrefab == null || shellPoint == null || shellTarget == null) return;

        GameObject shell = Instantiate(shellPrefab, shellPoint.position, shellPoint.rotation);
        Rigidbody2D rb = shell.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 ejectionDir = (shellTarget.position - shellPoint.position).normalized;
            float randomSpread = Random.Range(-0.1f, 0.1f);
            Vector2 finalDir = new Vector2(ejectionDir.x, ejectionDir.y + randomSpread);

            rb.AddForce(finalDir * shellForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-50f, 50f), ForceMode2D.Impulse);
        }

        Destroy(shell, 2f);
    }
}
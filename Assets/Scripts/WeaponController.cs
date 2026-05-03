using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Настройки пули")]
    public GameObject bulletPrefab;
    public float bulletForce = 20f;

    [Header("Точки направления оружия")]
    public Transform firePointStart;
    public Transform firePointEnd;

    void Update()
    {
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePointEnd.position, firePointEnd.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 barrelDirection = (firePointEnd.position - firePointStart.position).normalized;
            rb.AddForce(barrelDirection * bulletForce, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(barrelDirection.y, barrelDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}

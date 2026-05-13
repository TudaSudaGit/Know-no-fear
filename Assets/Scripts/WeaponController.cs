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
            Shoot();
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePointEnd.position, firePointEnd.rotation);

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
            bulletComp.attackerStats = GetComponentInParent<UnitStats>();

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (firePointEnd.position - firePointStart.position).normalized;
            rb.AddForce(dir * bulletForce, ForceMode2D.Impulse);
            bullet.transform.rotation = Quaternion.AngleAxis(
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, Vector3.forward);
        }
    }
}

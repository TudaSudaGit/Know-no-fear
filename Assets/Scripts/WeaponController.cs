using UnityEngine;
using TMPro;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    public static WeaponController Instance { get; private set; }
    public GameObject bulletPrefab;
    public float bulletForce = 20f;
    public Transform firePointStart, firePointEnd;
    public GameObject shellPrefab;
    public Transform shellPoint, shellTarget;
    public float shellForce = 5f;
    public int magazineSize = 30, maxReserveAmmo = 90;
    public TextMeshProUGUI ammoText;
    public float fireRate = 0.2f, reloadTime = 1.5f;
    private int currentAmmoInClip, currentReserveAmmo;
    private float fireTimer = 0f;
    private bool isReloading = false;
    private int baseMaxReserve = -1;

    void Awake()
    {
        Instance = this;
        if (baseMaxReserve == -1) baseMaxReserve = maxReserveAmmo;
    }

    void Start()
    {
        UpdateAmmoDifficulty();
    }

    public void UpdateAmmoDifficulty()
    {
        if (baseMaxReserve == -1) baseMaxReserve = maxReserveAmmo;
        GameSettings.LoadOptions();
        if (GameSettings.Difficulty == 0) maxReserveAmmo = baseMaxReserve + 60;
        else if (GameSettings.Difficulty == 1) maxReserveAmmo = baseMaxReserve + 30;
        else if (GameSettings.Difficulty == 2) maxReserveAmmo = baseMaxReserve;

        if (GameSettings.IsGameSaved && PlayerPrefs.HasKey("SavedAmmoClip"))
        {
            currentAmmoInClip = PlayerPrefs.GetInt("SavedAmmoClip");
            currentReserveAmmo = PlayerPrefs.GetInt("SavedAmmoReserve");
        }
        else
        {
            currentAmmoInClip = magazineSize;
            currentReserveAmmo = maxReserveAmmo;
        }
        UpdateAmmoUI();
    }

    void Update()
    {
        if (PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.IsQCurseActive) return;
        if (isReloading) return;
        if (fireTimer > 0f) fireTimer -= Time.deltaTime;
        bool inverted = PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.IsInverted;
        bool shootBlocked = PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.IsShootBlocked;
        bool fireInput = inverted ? (Input.GetMouseButton(0) && Input.GetMouseButtonDown(1)) : (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0));
        if (!shootBlocked && fireInput && currentAmmoInClip > 0 && fireTimer <= 0f)
        {
            Shoot();
            EjectShell();
            fireTimer = fireRate;
        }
        if (Input.GetKeyDown(KeyCode.R) && currentAmmoInClip < magazineSize && currentReserveAmmo > 0)
            StartCoroutine(ReloadRoutine());
    }

    void Shoot()
    {
        currentAmmoInClip--;
        UpdateAmmoUI();
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

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        if (ammoText != null) ammoText.text = "RELOADING...";
        yield return new WaitForSeconds(reloadTime);
        int ammoNeeded = magazineSize - currentAmmoInClip;
        int ammoToLoad = Mathf.Min(ammoNeeded, currentReserveAmmo);
        currentAmmoInClip += ammoToLoad;
        currentReserveAmmo -= ammoToLoad;
        isReloading = false;
        UpdateAmmoUI();
    }

    public void AddReserveAmmo(int amount)
    {
        currentReserveAmmo = Mathf.Min(currentReserveAmmo + amount, maxReserveAmmo);
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null) ammoText.text = $"{currentAmmoInClip} / {currentReserveAmmo}";
    }

    public void SaveAmmoData()
    {
        PlayerPrefs.SetInt("SavedAmmoClip", currentAmmoInClip);
        PlayerPrefs.SetInt("SavedAmmoReserve", currentReserveAmmo);
        PlayerPrefs.Save();
    }
}
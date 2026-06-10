using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public bool isPlayerHealth = false;
    [FormerlySerializedAs("heartsContainer")]
    public Transform healthContainer;

    private TextMeshProUGUI cubeText;
    private TextMeshProUGUI armorText;
    private Image cubeImage;
    private GameObject armorCube;
    private UnitStats cachedStats;

    private static readonly Color PLAYER_COLOR = new Color(1.00f, 0.84f, 0.05f);
    private static readonly Color ENEMY_COLOR = new Color(0.52f, 0.20f, 0.84f);
    private static readonly Color ARMOR_COLOR = new Color(0.45f, 0.55f, 0.62f);

    void Start()
    {
        cachedStats = GetComponent<UnitStats>()
                   ?? GetComponentInParent<UnitStats>()
                   ?? GetComponentInChildren<UnitStats>();
        if (cachedStats != null) maxHealth = cachedStats.wounds;
        currentHealth = maxHealth;
        if (healthContainer != null) SetupUI();
    }

    void Update()
    {
        if (cachedStats != null && armorCube != null && armorText != null)
        {
            if (cachedStats.armorPoints > 0)
            {
                armorCube.SetActive(true);
                armorText.text = cachedStats.armorPoints.ToString();
            }
            else
            {
                armorCube.SetActive(false);
            }
        }
    }

    void SetupUI()
    {
        foreach (Transform child in healthContainer)
            Destroy(child.gameObject);

        Color c = isPlayerHealth ? PLAYER_COLOR : ENEMY_COLOR;

        GameObject cube = new GameObject("HealthCube");
        cube.transform.SetParent(healthContainer, false);

        cubeImage = cube.AddComponent<Image>();
        cubeImage.color = c;

        RectTransform rt = cube.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 120f);

        Outline ol = cube.AddComponent<Outline>();
        ol.effectColor = new Color(0f, 0f, 0f, 0.80f);
        ol.effectDistance = new Vector2(3f, -3f);

        GameObject textGO = new GameObject("Num");
        textGO.transform.SetParent(cube.transform, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = currentHealth.ToString();
        tmp.fontSize = 64f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = isPlayerHealth ? new Color(0.10f, 0.07f, 0f) : Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        cubeText = tmp;

        armorCube = new GameObject("ArmorCube");
        armorCube.transform.SetParent(healthContainer, false);

        Image armorImg = armorCube.AddComponent<Image>();
        armorImg.color = ARMOR_COLOR;

        RectTransform art = armorCube.GetComponent<RectTransform>();
        art.sizeDelta = new Vector2(120f, 120f);

        Outline aOl = armorCube.AddComponent<Outline>();
        aOl.effectColor = new Color(0f, 0f, 0f, 0.80f);
        aOl.effectDistance = new Vector2(3f, -3f);

        GameObject armorTextGO = new GameObject("ArmorNum");
        armorTextGO.transform.SetParent(armorCube.transform, false);

        TextMeshProUGUI armTmp = armorTextGO.AddComponent<TextMeshProUGUI>();
        armTmp.text = "";
        armTmp.fontSize = 64f;
        armTmp.fontStyle = FontStyles.Bold;
        armTmp.color = Color.white;
        armTmp.alignment = TextAlignmentOptions.Center;

        RectTransform atrt = armorTextGO.GetComponent<RectTransform>();
        atrt.anchorMin = Vector2.zero;
        atrt.anchorMax = Vector2.one;
        atrt.offsetMin = Vector2.zero;
        atrt.offsetMax = Vector2.zero;

        armorText = armTmp;

        if (cachedStats != null)
        {
            armorCube.SetActive(cachedStats.armorPoints > 0);
        }
    }

    public void TakeDamage(int dmg) => ApplyDamage(dmg);

    public void ApplyDamage(int dmg)
    {
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        RefreshUI();
        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// Вызывается после UnitStats.TakeDamage() — берёт актуальное значение wounds
    /// и синхронизирует визуал без повторного вычитания урона.
    /// </summary>
    public void SyncFromStats()
    {
        if (cachedStats == null) return;
        currentHealth = Mathf.Max(0, cachedStats.wounds);
        RefreshUI();
        if (currentHealth <= 0) Die();
    }

    void RefreshUI()
    {
        if (cubeText != null) cubeText.text = currentHealth.ToString();
        if (cubeImage != null)
        {
            float t = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            Color base_c = isPlayerHealth ? PLAYER_COLOR : ENEMY_COLOR;
            cubeImage.color = Color.Lerp(new Color(0.4f, 0.05f, 0.05f), base_c, t);
        }
    }

    void Die()
    {
        if (isPlayerHealth)
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.ShowGameOver();
            }
        }
        else
        {
            EnemyXP enemyXP = GetComponent<EnemyXP>();
            if (enemyXP != null)
            {
                enemyXP.DropXP();
            }
            Destroy(gameObject);
        }
    }
}
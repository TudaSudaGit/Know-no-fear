using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Таймер выживания. Запускается вызовом SurvivalTimer.Instance.StartTimer()
/// (например, из TutorialManager после телепортации).
/// По истечении 3 минут убивает игрока → GameOver.
/// </summary>
public class SurvivalTimer : MonoBehaviour
{
    public static SurvivalTimer Instance { get; private set; }

    [Header("Время (секунды)")]
    public float duration = 180f;           // 3 минуты

    [Header("UI")]
    public TextMeshProUGUI timerText;       // Назначить в инспекторе, или создастся автоматически
    public Image timerBackground;          // Опциональный фон панели

    [Header("Предупреждение")]
    public float warningThreshold = 30f;   // Когда начать мигать красным
    public Color normalColor = new Color(1.00f, 0.84f, 0.05f); // Жёлтый (как PLAYER_COLOR)
    public Color warningColor = new Color(0.92f, 0.22f, 0.22f); // Красный

    // ── Внутреннее состояние ────────────────────────────────────────────────
    private float timeLeft;
    private bool isRunning = false;
    private bool isFinished = false;
    private float blinkTimer = 0f;
    private bool blinkState = true;

    // ── Unity lifecycle ─────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Если UI не назначен в инспекторе — создаём программно
        if (timerText == null)
            BuildTimerUI();

        SetTimerUIVisible(false);
    }

    void Update()
    {
        if (!isRunning || isFinished) return;

        timeLeft -= Time.deltaTime;

        UpdateTimerDisplay();

        if (timeLeft <= 0f)
            TimerExpired();
    }

    // ── Публичные методы ────────────────────────────────────────────────────

    /// <summary>Запустить таймер (вызывать после телепорта из TutorialManager)</summary>
    public void StartTimer()
    {
        timeLeft = duration;
        isRunning = true;
        isFinished = false;
        blinkTimer = 0f;
        blinkState = true;

        SetTimerUIVisible(true);
        UpdateTimerDisplay();
    }

    /// <summary>Остановить таймер досрочно (например, при победе)</summary>
    public void StopTimer()
    {
        isRunning = false;
        SetTimerUIVisible(false);
    }

    // ── Приватные вспомогательные ───────────────────────────────────────────

    void TimerExpired()
    {
        isRunning = false;
        isFinished = true;
        SetTimerUIVisible(false);

        // Убиваем игрока через Health — это вызовет Die() → GameOverManager
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
            {
                health.ApplyDamage(health.currentHealth); // снимаем все очки жизни
                return;
            }
        }

        // Запасной вариант: напрямую через GameOverManager
        if (GameOverManager.Instance != null)
            GameOverManager.Instance.ShowGameOver();
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{minutes:0}:{seconds:00}";

        bool isWarning = timeLeft <= warningThreshold;

        if (isWarning)
        {
            // Мигание
            blinkTimer += Time.deltaTime * 3f;
            if (blinkTimer >= 1f) { blinkTimer = 0f; blinkState = !blinkState; }

            timerText.color = blinkState ? warningColor : new Color(warningColor.r, warningColor.g, warningColor.b, 0.3f);
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    void SetTimerUIVisible(bool visible)
    {
        if (timerText != null)
            timerText.gameObject.SetActive(visible);

        if (timerBackground != null)
            timerBackground.gameObject.SetActive(visible);
    }

    // ── Автосоздание UI ─────────────────────────────────────────────────────

    void BuildTimerUI()
    {
        // Создаём отдельный Canvas поверх всего
        GameObject canvasGO = new GameObject("_SurvivalTimerCanvas");
        Canvas cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 997;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Фон-таблетка
        GameObject bgGO = new GameObject("TimerBG");
        bgGO.transform.SetParent(canvasGO.transform, false);

        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.06f, 0.12f, 0.88f);
        timerBackground = bg;

        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 1f);
        bgRt.anchorMax = new Vector2(0.5f, 1f);
        bgRt.pivot = new Vector2(0.5f, 1f);
        bgRt.sizeDelta = new Vector2(180f, 56f);
        bgRt.anchoredPosition = new Vector2(0f, -12f);

        // Иконка ⏳ + текст
        GameObject textGO = new GameObject("TimerLabel");
        textGO.transform.SetParent(bgGO.transform, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "0:00";
        tmp.fontSize = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = normalColor;
        tmp.alignment = TextAlignmentOptions.Center;
        timerText = tmp;

        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        // Outline для читаемости
        Outline ol = textGO.AddComponent<Outline>();
        ol.effectColor = new Color(0f, 0f, 0f, 0.85f);
        ol.effectDistance = new Vector2(2f, -2f);
    }
}
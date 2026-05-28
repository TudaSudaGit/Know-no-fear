using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerCurseHandler : MonoBehaviour
{
    public static PlayerCurseHandler Instance;

    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public float messageDuration = 5f;
    public int qPressesTarget = 15;

    public bool IsQCurseActive { get; private set; }
    public bool IsInverted { get; private set; }
    public bool IsShootBlocked { get; private set; }

    public bool AnyCurseActive => IsQCurseActive || IsInverted || IsShootBlocked;

    private int currentQPresses;
    private Coroutine hidePanelCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (IsQCurseActive)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentQPresses--;
                if (currentQPresses <= 0)
                {
                    ClearCurses();
                }
                else
                {
                    tutorialText.text = $"Проклятие: все действия заблокированы\nНажмите Q {currentQPresses} раз";
                }
            }
        }
    }

    public void ApplyCurse(int spell)
    {
        ClearCurses();

        if (tutorialPanel == null || tutorialText == null) return;

        tutorialPanel.SetActive(true);

        switch (spell)
        {
            case 0:
                IsInverted = true;
                hidePanelCoroutine = StartCoroutine(CountdownCurseRoutine(0, messageDuration));
                break;
            case 1:
                IsShootBlocked = true;
                hidePanelCoroutine = StartCoroutine(CountdownCurseRoutine(1, messageDuration));
                break;
            case 2:
                IsQCurseActive = true;
                currentQPresses = qPressesTarget;
                tutorialText.text = $"Проклятие: все действия заблокированы\n Для разблокировки нажмите Q {currentQPresses} раз";
                break;
            default:
                tutorialText.text = "Темная магия";
                hidePanelCoroutine = StartCoroutine(DefaultHideRoutine());
                break;
        }
    }

    public void ClearCurses()
    {
        IsQCurseActive = false;
        IsInverted = false;
        IsShootBlocked = false;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (hidePanelCoroutine != null)
        {
            StopCoroutine(hidePanelCoroutine);
            hidePanelCoroutine = null;
        }
    }

    private IEnumerator CountdownCurseRoutine(int spellType, float duration)
    {
        float timeLeft = duration;
        while (timeLeft > 0)
        {
            int seconds = Mathf.CeilToInt(timeLeft);
            if (spellType == 0)
                tutorialText.text = $"Проклятие: инверсия управления\nВаше управление инвертировано на {seconds} сек.";
            else if (spellType == 1)
                tutorialText.text = $"Проклятие: запрет стрельбы\nВы не можете стрелять {seconds} сек.";

            yield return null;
            timeLeft -= Time.deltaTime;
        }
        ClearCurses();
    }

    private IEnumerator DefaultHideRoutine()
    {
        yield return new WaitForSeconds(messageDuration);
        ClearCurses();
    }
}
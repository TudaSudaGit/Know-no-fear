using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum TutorialStep { StartSettings, Move, Reload, Aim, ShootPractice, CombatExplain, LevelUpExplain, Finished }

    public TutorialStep currentStep = TutorialStep.StartSettings;
    public RectTransform tutorialPanel;
    public TextMeshProUGUI instructionText;
    public Transform worldTargetPoint;
    public float yOffset = 100f;
    public GameObject tutorialTarget;
    public GameObject exitDoor;
    private Canvas mainCanvas;

    void Awake() { Instance = this; }

    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
            mainCanvas = tutorialPanel.GetComponentInParent<Canvas>();
        }
        ShowStepInstruction();
    }

    void Update()
    {
        if (currentStep == TutorialStep.Finished) return;
        HandleInput();
    }

    void LateUpdate()
    {
        if (worldTargetPoint == null || tutorialPanel == null || currentStep == TutorialStep.Finished) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldTargetPoint.position);
        float scaleFactor = (mainCanvas != null) ? mainCanvas.scaleFactor : 1f;
        screenPos.y += yOffset * scaleFactor;
        tutorialPanel.position = screenPos;
    }

    void HandleInput()
    {
        switch (currentStep)
        {
            case TutorialStep.StartSettings:
                if (Input.GetKeyDown(KeyCode.Escape)) AdvanceStep();
                break;
            case TutorialStep.Move:
                if (Input.GetKeyDown(InputManager.MoveLeftKey) || Input.GetKeyDown(InputManager.MoveRightKey)) AdvanceStep();
                break;
            case TutorialStep.Reload:
                if (Input.GetKeyDown(InputManager.ReloadKey)) AdvanceStep();
                break;
            case TutorialStep.Aim:
                if (Input.GetKeyDown(InputManager.AimKey)) AdvanceStep();
                break;
        }
    }

    void ShowStepInstruction()
    {
        switch (currentStep)
        {
            case TutorialStep.StartSettings:
                instructionText.text = "НАСТРОЙКИ:\nНажми [ESC], чтобы открыть меню паузы.\nЗдесь можно сохранить игру, выйти или настроить прицел.";
                break;
            case TutorialStep.Move:
                instructionText.text = $"ДВИЖЕНИЕ:\nИспользуй [{FormatKey(InputManager.MoveLeftKey)}] и [{FormatKey(InputManager.MoveRightKey)}] для передвижения.";
                break;
            case TutorialStep.Reload:
                instructionText.text = $"ПЕРЕЗАРЯДКА:\nНажми [{FormatKey(InputManager.ReloadKey)}], чтобы перезарядить оружие.";
                break;
            case TutorialStep.Aim:
                instructionText.text = $"ПРИЦЕЛИВАНИЕ:\nЗажми [{FormatKey(InputManager.AimKey)}], чтобы прицелиться.";
                break;
            case TutorialStep.ShootPractice:
                instructionText.text = $"СТРЕЛЬБА:\nУдерживая [{FormatKey(InputManager.AimKey)}], нажми [{FormatKey(InputManager.ShootKey)}], чтобы выстрелить в мишень.";
                break;
            case TutorialStep.CombatExplain:
                instructionText.text = "МЕХАНИКА БОЯ:\n6 на 1 кубике — Попадание.\n4 на 2 кубике — Рана.\n1 на 3 кубике — Провал защиты врага.\nПодбери выпавший опыт!";
                break;
            case TutorialStep.LevelUpExplain:
                instructionText.text = "СИСТЕМА ПРОКАЧКИ:\nТы получил уровень! Выбери одну из характеристик, чтобы усилить персонажа.";
                break;
            case TutorialStep.Finished:
                instructionText.text = "ОТЛИЧНО!\nДверь открыта. Подойди к ней и нажми [E], чтобы выйти из обучения.";
                break;
        }
    }

    public void TargetHit()
    {
        if (currentStep == TutorialStep.ShootPractice) AdvanceStep();
    }

    public void OnXPPickedUp()
    {
        if (currentStep == TutorialStep.CombatExplain) AdvanceStep();
    }

    public void OnUpgradeSelected()
    {
        if (currentStep == TutorialStep.LevelUpExplain) AdvanceStep();
    }

    public void OnEnemySpawned() { }

    void AdvanceStep()
    {
        currentStep++;
        ShowStepInstruction();
    }

    string FormatKey(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return "ЛКМ";
        if (key == KeyCode.Mouse1) return "ПКМ";
        if (key == KeyCode.Mouse2) return "СКМ";
        return key.ToString();
    }
}
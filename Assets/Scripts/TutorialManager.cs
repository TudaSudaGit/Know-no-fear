using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum TutorialStep { Move, Aim, SelectCrosshair, Shoot, Reload, ExplainDice, Finished }

    [Header("Текущий шаг")]
    public TutorialStep currentStep = TutorialStep.Move;

    [Header("UI Элементы обучения")]
    public RectTransform tutorialPanel;
    public TextMeshProUGUI instructionText;

    [Header("Привязка к миру")]
    public Transform worldTargetPoint;
    public float yOffset = 100f;

    private Canvas mainCanvas;

    void Awake()
    {
        Instance = this;
    }

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
            case TutorialStep.Move:
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) AdvanceStep();
                break;

            case TutorialStep.Aim:
                if (Input.GetMouseButtonDown(1)) AdvanceStep();
                break;

            case TutorialStep.SelectCrosshair:
                if (Input.GetKeyDown(KeyCode.Escape)) AdvanceStep();
                break;

            case TutorialStep.Shoot:
                if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0)) AdvanceStep();
                break;

            case TutorialStep.Reload:
                if (Input.GetKeyDown(KeyCode.R)) AdvanceStep();
                break;
        }
    }

    void ShowStepInstruction()
    {
        switch (currentStep)
        {
            case TutorialStep.Move:
                instructionText.text = "ДОБРО ПОЖАЛОВАТЬ!\nИспользуй [A] и [D] для движения.";
                break;

            case TutorialStep.Aim:
                instructionText.text = "ПРИЦЕЛИВАНИЕ:\nЗажми [ПКМ], чтобы поднять оружие.";
                break;

            case TutorialStep.SelectCrosshair:
                instructionText.text = "ПАУЗА И НАСТРОЙКИ:\nНажав ESC вы можете поставить игру на паузу, чтобы сохраниться поменять настройки прицела или выйти в главное меню";
                break;

            case TutorialStep.Shoot:
                instructionText.text = "ВЫСТРЕЛ:\nУдерживая [ПКМ], нажми [ЛКМ].";
                break;

            case TutorialStep.Reload:
                instructionText.text = "ПЕРЕЗАРЯДКА:\nНажми [R] для перезарядки.\nКоличество патронов отображается под игроком.\nПатроны можно найти на карте в ящиках.";
                break;

            case TutorialStep.ExplainDice:
                instructionText.text = "МЕХАНИКА ПРОБИТИЯ:\nПод персонажем на кубиках отображаются ХП (жёлтый) и броня (синий) которая защищает тебя от потери ХП, броню можно найти на карте\nПопадание и урон зависят от броска костей.\nЕсли твоя Сила выше стойкости врага, урон наносится легче.\n<color=#87CEEB>Иди направо и спаси этот мир.</color>";
                break;

            case TutorialStep.Finished:
                tutorialPanel.gameObject.SetActive(false);
                break;
        }
    }

    public void OnEnemySpawned()
    {
        if (currentStep == TutorialStep.ExplainDice)
        {
            AdvanceStep();
        }
    }

    void AdvanceStep()
    {
        currentStep++;
        ShowStepInstruction();
    }
}
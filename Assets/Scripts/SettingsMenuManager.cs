using UnityEngine;
using TMPro;
using System.Collections;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Тексты кнопок управления")]
    public TextMeshProUGUI moveLeftText;
    public TextMeshProUGUI moveRightText;
    public TextMeshProUGUI aimText;
    public TextMeshProUGUI shootText;
    public TextMeshProUGUI reloadText;
    public TextMeshProUGUI interactText;

    private Coroutine rebindCoroutine;

    void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (moveLeftText != null) moveLeftText.text = InputManager.GetKeyFriendlyName(InputManager.MoveLeftKey);
        if (moveRightText != null) moveRightText.text = InputManager.GetKeyFriendlyName(InputManager.MoveRightKey);
        if (aimText != null) aimText.text = InputManager.GetKeyFriendlyName(InputManager.AimKey);
        if (shootText != null) shootText.text = InputManager.GetKeyFriendlyName(InputManager.ShootKey);
        if (reloadText != null) reloadText.text = InputManager.GetKeyFriendlyName(InputManager.ReloadKey);
        if (interactText != null) interactText.text = InputManager.GetKeyFriendlyName(InputManager.InteractKey);
    }

    public void StartRebind(string actionName)
    {
        if (rebindCoroutine != null)
        {
            StopCoroutine(rebindCoroutine);
        }

        TextMeshProUGUI targetText = null;

        switch (actionName)
        {
            case "MoveLeft": targetText = moveLeftText; break;
            case "MoveRight": targetText = moveRightText; break;
            case "Aim": targetText = aimText; break;
            case "Shoot": targetText = shootText; break;
            case "Reload": targetText = reloadText; break;
            case "Interact": targetText = interactText; break;
        }

        if (targetText != null)
        {
            rebindCoroutine = StartCoroutine(WaitForKeyPressRoutine(actionName, targetText));
        }
    }

    private IEnumerator WaitForKeyPressRoutine(string actionName, TextMeshProUGUI targetText)
    {
        targetText.text = "НАЖМИТЕ КЛАВИШУ...";

        // Ждем один кадр, чтобы само нажатие на кнопку UI не засчиталось как ввод
        yield return null;

        bool keyDetected = false;
        KeyCode pressedKey = KeyCode.None;

        while (!keyDetected)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (key == KeyCode.None) continue;

                if (Input.GetKeyDown(key))
                {
                    pressedKey = key;
                    keyDetected = true;
                    break;
                }
            }
            yield return null;
        }

        InputManager.SetKey(actionName, pressedKey);
        UpdateVisuals();
        rebindCoroutine = null;
    }
}
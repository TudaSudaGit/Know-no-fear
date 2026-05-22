using UnityEngine;

public class PlayerCurseHandler : MonoBehaviour
{
    public static PlayerCurseHandler Instance { get; private set; }

    public bool IsQCurseActive => qPressesRemaining > 0;
    public bool IsInverted     => invertTimer > 0f;
    public bool IsShootBlocked => noShootTimer > 0f;
    public bool AnyCurseActive => IsQCurseActive || IsInverted || IsShootBlocked;

    private int   qPressesRemaining = 0;
    private float invertTimer       = 0f;
    private float noShootTimer      = 0f;

    void Awake() => Instance = this;

    void Update()
    {
        if (invertTimer  > 0f) invertTimer  -= Time.deltaTime;
        if (noShootTimer > 0f) noShootTimer -= Time.deltaTime;

        if (IsQCurseActive && Input.GetKeyDown(KeyCode.Q))
        {
            qPressesRemaining--;
            Debug.Log($"[Curse] Q нажата, осталось: {qPressesRemaining}");
        }
    }

    public void ApplyCurse(int spell)
    {
        switch (spell)
        {
            case 0:
                qPressesRemaining = Random.Range(10, 21);
                Debug.Log($"[Curse] Проклятье Q: нажми Q {qPressesRemaining} раз!");
                break;
            case 1:
                invertTimer = 10f;
                Debug.Log("[Curse] Проклятье: инверсия управления на 10 сек");
                break;
            case 2:
                noShootTimer = 6f;
                Debug.Log("[Curse] Проклятье: запрет стрельбы на 6 сек");
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.U2D.IK;

public class MouseAimTarget : MonoBehaviour
{
    [Header("Основные ссылки")]
    public Transform playerRoot;
    public Transform shoulder;
    public Animator animator;
    public IKManager2D ikManager;

    [Header("Настройки спрайта-прицела")]
    public GameObject visualCrosshair;
    public bool hideSystemCursor = true;

    [Header("Параметры прицеливания")]
    public float aimRadius = 3f;
    [Range(-180, 180)]
    public float angleOffset = 0f;

    [Header("Параметры анимации")]
    public string walkParameter = "Speed";
    public float walkThreshold = 0.1f;
    public bool isWalkParamBool = false;

    [Header("Сглаживание")]
    public float weightSmoothSpeed = 10f;

    private float targetWeight = 0f;

    [Header("Настройки изменения цвета прицела")]
    public CrosshairColor crosshairColorScript;
    public LayerMask enemyLayer;

    private void Start()
    {
        if (ikManager != null) ikManager.weight = 0f;
        if (visualCrosshair != null) visualCrosshair.SetActive(false);
        Cursor.visible = true;
    }

    void Update()
    {
        if (shoulder == null || Camera.main == null || playerRoot == null) return;

        if (PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.IsQCurseActive)
        {
            targetWeight = 0f;
            UpdateVisualCrosshair(false);
            if (ikManager != null)
                ikManager.weight = Mathf.Lerp(ikManager.weight, 0f, Time.deltaTime * weightSmoothSpeed);
            return;
        }

        bool inverted = PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.IsInverted;
        bool isAiming = inverted ? Input.GetMouseButton(0) : Input.GetMouseButton(1);

        if (isAiming)
        {
            targetWeight = 1f;
            UpdateVisualCrosshair(true);
            HandleAimingAndFlip(inverted);
        }
        else
        {
            targetWeight = 0f;
            UpdateVisualCrosshair(false);
        }

        if (ikManager != null)
            ikManager.weight = Mathf.Lerp(ikManager.weight, targetWeight, Time.deltaTime * weightSmoothSpeed);

        if (crosshairColorScript != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, 0f, enemyLayer);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                crosshairColorScript.SetOverEnemy();
        }
    }

    private void UpdateVisualCrosshair(bool active)
    {
        if (visualCrosshair != null)
        {
            visualCrosshair.SetActive(active);

            if (active)
            {
                RectTransform rect = visualCrosshair.GetComponent<RectTransform>();
                Canvas canvas = visualCrosshair.GetComponentInParent<Canvas>();

                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    rect.position = Input.mousePosition;
                }
                else
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        Input.mousePosition,
                        canvas.worldCamera,
                        out localPoint);
                    rect.anchoredPosition = localPoint;
                }

                if (hideSystemCursor) Cursor.visible = false;
            }
            else
            {
                if (hideSystemCursor) Cursor.visible = true;
            }
        }
    }

    private void HandleAimingAndFlip(bool inverted)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        worldMousePos.z = 0f;

        // При инверсии зеркалим позицию мыши относительно персонажа
        if (inverted)
        {
            float dx = worldMousePos.x - playerRoot.position.x;
            float dy = worldMousePos.y - playerRoot.position.y;
            worldMousePos.x = playerRoot.position.x - dx;
            worldMousePos.y = playerRoot.position.y - dy;
        }

        bool isWalking = false;
        if (animator != null)
        {
            isWalking = isWalkParamBool
                ? animator.GetBool(walkParameter)
                : Mathf.Abs(animator.GetFloat(walkParameter)) > walkThreshold;
        }

        if (!isWalking)
        {
            Vector3 scale = playerRoot.localScale;
            if (worldMousePos.x < playerRoot.position.x)
                scale.x = -Mathf.Abs(scale.x);
            else
                scale.x = Mathf.Abs(scale.x);
            playerRoot.localScale = scale;
        }

        Vector3 directionToMouse = (worldMousePos - shoulder.position).normalized;
        float appliedOffset = playerRoot.localScale.x < 0 ? -angleOffset : angleOffset;

        float radians = appliedOffset * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        Vector3 offsetDirection = new Vector3(
            (cos * directionToMouse.x) - (sin * directionToMouse.y),
            (sin * directionToMouse.x) + (cos * directionToMouse.y),
            0
        );

        transform.position = shoulder.position + offsetDirection * aimRadius;
    }
}

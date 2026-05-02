using UnityEngine;

public class MouseAimTarget : MonoBehaviour
{
    public Transform playerRoot;
    public Transform shoulder;
    public Animator animator;
    public string walkParameter = "Speed";
    public float walkThreshold = 0.1f;
    public bool isWalkParamBool = false;

    public float aimRadius = 3f;
    [Range(-180, 180)]
    public float angleOffset = 0f;

    public bool forceLockFlip = false;

    void Update()
    {
        if (shoulder == null || Camera.main == null || playerRoot == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        worldMousePos.z = 0f;

        bool isWalking = forceLockFlip;

        if (animator != null)
        {
            if (isWalkParamBool)
            {
                isWalking = isWalking || animator.GetBool(walkParameter);
            }
            else
            {
                isWalking = isWalking || Mathf.Abs(animator.GetFloat(walkParameter)) > walkThreshold;
            }
        }

        Vector3 scale = playerRoot.localScale;

        if (!isWalking)
        {
            if (worldMousePos.x < playerRoot.position.x)
            {
                scale.x = -Mathf.Abs(scale.x);
            }
            else
            {
                scale.x = Mathf.Abs(scale.x);
            }
            playerRoot.localScale = scale;
        }

        Vector3 directionToMouse = (worldMousePos - shoulder.position).normalized;

        float appliedOffset = playerRoot.localScale.x < 0 ? -angleOffset : angleOffset;
        float radians = appliedOffset * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = directionToMouse.x;
        float ty = directionToMouse.y;

        Vector3 offsetDirection = new Vector3(
            (cos * tx) - (sin * ty),
            (sin * tx) + (cos * ty),
            0
        );

        transform.position = shoulder.position + offsetDirection * aimRadius;
    }
}
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 lastDirection = Vector2.right;
    private bool isFinishingStep = false;
    private bool hasStopped = false;
    private float stepFinishTarget = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(h, v).normalized;
        bool hasInput = inputDir.magnitude > 0.01f;

        if (hasInput)
        {
            lastDirection = inputDir;
            isFinishingStep = false;
            hasStopped = false;
            stepFinishTarget = -1f;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = state.normalizedTime;

        if (!hasInput && !hasStopped && state.IsName("Walk") && !isFinishingStep)
        {
            isFinishingStep = true;
            stepFinishTarget = Mathf.Floor(normalizedTime) + 1f;
        }

        if (isFinishingStep && normalizedTime >= stepFinishTarget)
        {
            isFinishingStep = false;
            hasStopped = true;
            stepFinishTarget = -1f;
        }

        bool keepMoving = hasInput || isFinishingStep;

        rb.linearVelocity = keepMoving ? lastDirection * moveSpeed : Vector2.zero;
        animator.SetFloat("Speed", keepMoving ? 1f : 0f);
    }
}
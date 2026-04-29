using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4.2f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 lastDirection = Vector2.right;
    private bool isFinishingStep = false;
    private bool hasStopped = false;
    private float stepFinishTarget = -1f;

    private bool keepMoving = false;
    private float horizontalInput = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Ищем аниматор в детях, так как он остался на объекте "charecter"
        animator = GetComponentInChildren<Animator>();

        // "Прогреваем" аниматор, чтобы не было ошибки в первом кадре
        if (animator != null) animator.Update(0);
    }

    void Update()
    {
        if (animator == null) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        bool hasInput = Mathf.Abs(horizontalInput) > 0.01f;

        if (hasInput)
        {
            float directionX = Mathf.Sign(horizontalInput);
            lastDirection = new Vector2(directionX, 0f);
            isFinishingStep = false;
            hasStopped = false;
            stepFinishTarget = -1f;

            // РАЗВОРОТ: меняем масштаб всего родительского объекта
            // Теперь персонаж не будет телепортироваться, если вы правильно 
            // отцентровали "дочку" внутри "родителя".
            transform.localScale = new Vector3(directionX, 1f, 1f);
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

        keepMoving = hasInput || isFinishingStep;
        animator.SetFloat("Speed", keepMoving ? 1f : 0f);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = keepMoving ? lastDirection * moveSpeed : Vector2.zero;
    }
}
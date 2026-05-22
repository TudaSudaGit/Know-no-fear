using UnityEngine;

public class MageEnemy : MonoBehaviour
{
    [Header("Дальность")]
    public float castRange = 7f;

    [Header("Тайминг")]
    public float castCooldown = 8f;
    private float castTimer = 0f;

    [Header("Ссылки")]
    public Transform uiContainer;

    private Transform player;
    private Animator  animator;
    private bool      isCasting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player   = GameObject.FindGameObjectWithTag("Player").transform;
        castTimer = castCooldown; // первый каст через 8 сек
    }

    void Update()
    {
        if (player == null) return;

        FlipToPlayer();
        FixUI();

        if (isCasting) return;

        castTimer -= Time.deltaTime;
        if (castTimer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= castRange)
            {
                castTimer = castCooldown;
                StartCast();
            }
            else
            {
                castTimer = 1f; // проверяем каждую секунду пока не в диапазоне
            }
        }
    }

    void StartCast()
    {
        isCasting = true;
        if (animator != null) animator.SetTrigger("Attack");
        // Урон не через DiceRoll — заклинание всегда попадает
    }

    // Вызывается AnimationEvent на последнем кадре анимации удара
    public void OnCastHit()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= castRange && PlayerCurseHandler.Instance != null)
        {
            int spell = Random.Range(0, 3);
            PlayerCurseHandler.Instance.ApplyCurse(spell);
        }
        isCasting = false;
    }

    void FlipToPlayer()
    {
        float sx = player.position.x < transform.position.x ? -1f : 1f;
        transform.localScale = new Vector3(sx, 1f, 1f);
    }

    void FixUI()
    {
        if (uiContainer == null) return;
        Vector3 ls = uiContainer.localScale;
        ls.x = Mathf.Abs(ls.x) * Mathf.Sign(transform.localScale.x);
        uiContainer.localScale = ls;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, castRange);
    }
}

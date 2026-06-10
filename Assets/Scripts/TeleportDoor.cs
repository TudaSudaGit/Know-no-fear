using UnityEngine;

public class UniversalDoor : MonoBehaviour
{
    public Transform targetPoint;
    public Transform[] objectsToTeleport;
    public GameObject interactionHint;
    public GameObject oldCameraZone;
    public GameObject newCameraZone;

    [Header("Таймер выживания")]
    [Tooltip("Включить только на двери после обучения — запустит 3-минутный таймер")]
    public bool startTimerOnTeleport = false;

    private bool isPlayerInside = false;

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            Teleport();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (interactionHint != null) interactionHint.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (interactionHint != null) interactionHint.SetActive(false);
        }
    }

    void Teleport()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.tutorialPanel != null)
        {
            TutorialManager.Instance.tutorialPanel.gameObject.SetActive(false);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || targetPoint == null) return;

        Vector3 delta = targetPoint.position - player.transform.position;

        foreach (Transform obj in objectsToTeleport)
        {
            if (obj != null)
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = (Vector2)(obj.position + delta);
                obj.position += delta;
            }
        }

        if (oldCameraZone != null) oldCameraZone.SetActive(false);
        if (newCameraZone != null) newCameraZone.SetActive(true);

        Physics2D.SyncTransforms();

        isPlayerInside = false;
        if (interactionHint != null) interactionHint.SetActive(false);

        if (startTimerOnTeleport && SurvivalTimer.Instance != null)
            SurvivalTimer.Instance.StartTimer();
    }
}
using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    public Transform targetPoint;
    public Transform[] objectsToTeleport;
    public GameObject interactionHint;

    private bool isPlayerInside = false;
    private Transform playerTransform;

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
            playerTransform = other.transform;
            if (interactionHint != null)
            {
                interactionHint.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }
    }

    void Teleport()
    {
        if (playerTransform == null || targetPoint == null) return;

        Vector3 delta = targetPoint.position - playerTransform.position;

        for (int i = 0; i < objectsToTeleport.Length; i++)
        {
            if (objectsToTeleport[i] != null)
            {
                Rigidbody2D rb = objectsToTeleport[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = (Vector2)(objectsToTeleport[i].position + delta);
                }
                objectsToTeleport[i].position += delta;
            }
        }

        Physics2D.SyncTransforms();

        isPlayerInside = false;
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }
}
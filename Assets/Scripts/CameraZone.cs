using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public GameObject zoneCamera;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            zoneCamera.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            zoneCamera.SetActive(false);
        }
    }
}
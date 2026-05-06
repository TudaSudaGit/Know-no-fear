using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Destroy(gameObject);

    }
    private void Start()
    {
        Destroy(gameObject, 10f);
    }
}
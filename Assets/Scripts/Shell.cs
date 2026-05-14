using UnityEngine;

public class Shell : MonoBehaviour
{
    public float destroyTime = 2f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        float torque = 0f;
        rb.AddTorque(torque, ForceMode2D.Impulse);

        Destroy(gameObject, destroyTime);
    }
}
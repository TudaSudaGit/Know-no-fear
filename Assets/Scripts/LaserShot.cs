using UnityEngine;

public class LaserShot : MonoBehaviour
{
    public float laserLength = 15f;
    public float lifetime = 0.5f;   
    public Color laserColor = Color.red;

    void Start()
    {
        Debug.Log("LaserShot Start запущен");

        LineRenderer line = gameObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0.2f;  
        line.endWidth = 0.2f;
        line.startColor = Color.red;
        line.endColor = Color.red;
        line.sortingOrder = 100;

        Renderer anyRenderer = FindAnyObjectByType<SpriteRenderer>();
        if (anyRenderer != null)
            line.material = anyRenderer.material;

        Vector3 start = transform.position;
        Vector3 end = start + transform.right * laserLength;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(gameObject, lifetime);
    }
}
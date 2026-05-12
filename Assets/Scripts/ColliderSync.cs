using UnityEngine;

public class ColliderSync : MonoBehaviour
{
    private PolygonCollider2D polyCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != null)
        {
            polyCollider.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();

            System.Collections.Generic.List<Vector2> path = new System.Collections.Generic.List<Vector2>();
            for (int i = 0; i < polyCollider.pathCount; i++)
            {
                spriteRenderer.sprite.GetPhysicsShape(i, path);
                polyCollider.SetPath(i, path.ToArray());
            }
        }
    }
}
using UnityEngine;

public class CrosshairFollow : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }
}
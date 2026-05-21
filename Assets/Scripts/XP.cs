using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    private int xpValue;
    private Transform player;

    public void Setup(int value)
    {
        xpValue = value;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        moveSpeed += acceleration * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.position) < 0.2f)
        {
            PlayerXP.Instance.AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public float moveSpeed = 2f;
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

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);

        if (Vector3.Distance(transform.position, player.position) < 0.2f)
        {
            PlayerXP.Instance.AddXP(xpValue);
            if (TutorialManager.Instance != null) TutorialManager.Instance.OnXPPickedUp();
            Destroy(gameObject);
        }
    }
}
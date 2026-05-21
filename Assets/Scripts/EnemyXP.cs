using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    public GameObject xpOrbPrefab;
    public int xpReward = 20;

    public void DropXP()
    {
        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            XPOrb orbScript = orb.GetComponent<XPOrb>();
            if (orbScript != null)
            {
                orbScript.Setup(xpReward);
            }
        }
    }
}
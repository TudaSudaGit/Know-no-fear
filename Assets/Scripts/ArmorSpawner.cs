using UnityEngine;
using System.Collections.Generic;

public class ArmorSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPointData
    {
        public Transform spawnPoint;
        [Range(0f, 100f)] public float spawnChance;
    }

    public GameObject armorPickupPrefab;
    public SpawnPointData[] spawnPoints;
    public int minArmor = 1;
    public int maxArmor = 3;
    public int armorValue = 5;

    public void SpawnArmorObjects()
    {
        List<Transform> successfulPoints = new List<Transform>();
        List<Transform> failedPoints = new List<Transform>();

        foreach (var data in spawnPoints)
        {
            if (data.spawnPoint == null) continue;
            if (Random.Range(0f, 100f) <= data.spawnChance)
                successfulPoints.Add(data.spawnPoint);
            else
                failedPoints.Add(data.spawnPoint);
        }

        while (successfulPoints.Count < minArmor && failedPoints.Count > 0)
        {
            int idx = Random.Range(0, failedPoints.Count);
            successfulPoints.Add(failedPoints[idx]);
            failedPoints.RemoveAt(idx);
        }

        while (successfulPoints.Count > maxArmor)
        {
            successfulPoints.RemoveAt(Random.Range(0, successfulPoints.Count));
        }

        foreach (var point in successfulPoints)
        {
            GameObject obj = Instantiate(armorPickupPrefab, point.position, Quaternion.identity);
            ArmorPickup pickup = obj.GetComponent<ArmorPickup>();
            if (pickup != null) pickup.Setup(armorValue);
        }
    }
}
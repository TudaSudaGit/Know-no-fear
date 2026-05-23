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

    public string spawnerID = "Location_1";
    public GameObject armorPickupPrefab;
    public SpawnPointData[] spawnPoints;
    public int minArmor = 1;
    public int maxArmor = 3;
    public int armorValue = 5;

    private List<GameObject> spawnedArmor = new List<GameObject>();

    void Start()
    {
        if (GameSettings.IsGameSaved)
        {
            SpawnArmorObjects();
        }
    }

    public void SpawnArmorObjects()
    {
        foreach (GameObject armor in spawnedArmor)
        {
            if (armor != null) Destroy(armor);
        }
        spawnedArmor.Clear();

        List<int> successfulIndices = new List<int>();
        List<int> failedIndices = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].spawnPoint == null) continue;
            if (Random.Range(0f, 100f) <= spawnPoints[i].spawnChance)
                successfulIndices.Add(i);
            else
                failedIndices.Add(i);
        }

        while (successfulIndices.Count < minArmor && failedIndices.Count > 0)
        {
            int idx = Random.Range(0, failedIndices.Count);
            successfulIndices.Add(failedIndices[idx]);
            failedIndices.RemoveAt(idx);
        }

        while (successfulIndices.Count > maxArmor)
        {
            successfulIndices.RemoveAt(Random.Range(0, successfulIndices.Count));
        }

        for (int i = 0; i < successfulIndices.Count; i++)
        {
            SpawnArmor(successfulIndices[i]);
        }
    }

    void SpawnArmor(int index)
    {
        GameObject obj = Instantiate(armorPickupPrefab, spawnPoints[index].spawnPoint.position, Quaternion.identity);
        spawnedArmor.Add(obj);

        ArmorPickup pickup = obj.GetComponent<ArmorPickup>();
        if (pickup != null)
        {
            pickup.Setup(armorValue);
        }
    }

    public void SaveArmorState()
    {
    }
}
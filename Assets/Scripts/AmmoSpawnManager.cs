using UnityEngine;
using System.Collections.Generic;

public class AmmoSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPointData
    {
        public Transform spawnPoint;
        [Range(0f, 100f)] public float spawnChance;
    }

    public string spawnerID = "Location_1";
    public GameObject ammoCratePrefab;
    public GameObject ammoHintUI;
    public SpawnPointData[] spawnPoints;

    public int minCrates = 2;
    public int maxCrates = 5;
    public int ammoPerCrate = 30;

    private List<GameObject> spawnedCrates = new List<GameObject>();

    void Start()
    {
        if (GameSettings.IsGameSaved)
        {
            SpawnAmmoObjects();
        }
    }

    public void SpawnAmmoObjects()
    {
        foreach (GameObject crate in spawnedCrates)
        {
            if (crate != null) Destroy(crate);
        }
        spawnedCrates.Clear();

        List<Transform> successfulPoints = new List<Transform>();
        List<Transform> failedPoints = new List<Transform>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].spawnPoint == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= spawnPoints[i].spawnChance)
            {
                successfulPoints.Add(spawnPoints[i].spawnPoint);
            }
            else
            {
                failedPoints.Add(spawnPoints[i].spawnPoint);
            }
        }

        while (successfulPoints.Count < minCrates && failedPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, failedPoints.Count);
            successfulPoints.Add(failedPoints[randomIndex]);
            failedPoints.RemoveAt(randomIndex);
        }

        while (successfulPoints.Count > maxCrates)
        {
            int randomIndex = Random.Range(0, successfulPoints.Count);
            successfulPoints.RemoveAt(randomIndex);
        }

        for (int i = 0; i < successfulPoints.Count; i++)
        {
            GameObject crateObj = Instantiate(ammoCratePrefab, successfulPoints[i].position, Quaternion.identity);
            spawnedCrates.Add(crateObj);
            AmmoCrate crate = crateObj.GetComponent<AmmoCrate>();
            if (crate != null)
            {
                crate.Setup(ammoPerCrate, ammoHintUI);
            }
        }
    }

    public void SaveAmmoState()
    {
    }
}
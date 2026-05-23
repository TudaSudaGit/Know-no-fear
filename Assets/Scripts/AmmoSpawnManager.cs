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
            if (crate != null)
            {
                Destroy(crate);
            }
        }
        spawnedCrates.Clear();

        if (GameSettings.IsGameSaved)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].spawnPoint == null) continue;
                if (PlayerPrefs.GetInt(spawnerID + "_AmmoSpawn_" + i, 0) == 1)
                {
                    SpawnCrate(i);
                }
            }
        }
        else
        {
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

            while (successfulIndices.Count < minCrates && failedIndices.Count > 0)
            {
                int idx = Random.Range(0, failedIndices.Count);
                successfulIndices.Add(failedIndices[idx]);
                failedIndices.RemoveAt(idx);
            }

            while (successfulIndices.Count > maxCrates)
            {
                successfulIndices.RemoveAt(Random.Range(0, successfulIndices.Count));
            }

            for (int i = 0; i < successfulIndices.Count; i++)
            {
                SpawnCrate(successfulIndices[i]);
            }
        }
    }

    void SpawnCrate(int index)
    {
        if (ammoCratePrefab == null)
        {
            Debug.LogError("Ошибка: ammoCratePrefab не назначен в Инспекторе!", this);
            return;
        }

        GameObject obj = Instantiate(ammoCratePrefab, spawnPoints[index].spawnPoint.position, Quaternion.identity);
        spawnedCrates.Add(obj);

        AmmoCrate crate = obj.GetComponent<AmmoCrate>();
        if (crate != null)
        {
            crate.spawnPointIndex = index;
            crate.Setup(ammoPerCrate, ammoHintUI);
        }
    }

    public void SaveAmmoState()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            PlayerPrefs.SetInt(spawnerID + "_AmmoSpawn_" + i, 0);
        }

        foreach (GameObject obj in spawnedCrates)
        {
            if (obj != null)
            {
                AmmoCrate crate = obj.GetComponent<AmmoCrate>();
                if (crate != null && crate.spawnPointIndex != -1)
                {
                    PlayerPrefs.SetInt(spawnerID + "_AmmoSpawn_" + crate.spawnPointIndex, 1);
                }
            }
        }
    }
}
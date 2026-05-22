using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProportionalSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyGroup
    {
        public string name;
        public GameObject prefab;
        public int count;
    }

    [Header("Настройки врагов")]
    public List<EnemyGroup> enemiesToSpawn = new List<EnemyGroup>();

    [Header("Настройки дистанции")]
    public float activationRange = 10f;
    public float spawnDelay = 2f;
    public float spawnRadius = 2f;

    private Transform player;
    private bool isStarted = false;
    private int totalRemaining;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        CalculateTotal();
    }

    void Update()
    {
        if (player == null || isStarted) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= activationRange)
        {
            isStarted = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    void CalculateTotal()
    {
        totalRemaining = 0;
        foreach (var group in enemiesToSpawn)
        {
            totalRemaining += group.count;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (totalRemaining > 0)
        {
            TutorialManager.Instance.OnEnemySpawned();
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnRandomEnemy()
    {
        int randomTicket = Random.Range(1, totalRemaining + 1);
        int currentRange = 0;

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            if (enemiesToSpawn[i].count <= 0) continue;

            currentRange += enemiesToSpawn[i].count;

            if (randomTicket <= currentRange)
            {
                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Instantiate(enemiesToSpawn[i].prefab, transform.position + (Vector3)offset, Quaternion.identity, transform);

                var group = enemiesToSpawn[i];
                group.count--;
                enemiesToSpawn[i] = group;

                totalRemaining--;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 1.5f;
    public float firstSpawnDelay = 1f;

    private void Start()
    {
        StartCoroutine(
            SpawnLoop()
        );
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(
            firstSpawnDelay
        );

        while (GameManager.Instance != null &&
               !GameManager.Instance.IsGameOver)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(
                spawnInterval
            );
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }

        int randomIndex =
            Random.Range(
                0,
                spawnPoints.Length
            );

        Transform spawnPoint =
            spawnPoints[randomIndex];

        Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}

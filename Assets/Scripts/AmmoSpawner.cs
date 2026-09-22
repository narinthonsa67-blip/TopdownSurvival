using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject ammoBoxPrefab; // กล่องกระสุน Prefab
    public float spawnInterval = 7f; // เสกทุก 7 วินาที
    public int maxBoxes = 3;         // ในฉากมีกล่องค้างไว้ได้ไม่เกิน 3 กล่อง

    [Header("Spawn Boundaries")]
    public float minX = -7f, maxX = 7f;
    public float minZ = -7f, maxZ = 7f;
    public float spawnY = 0.5f;

    private float timer;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnAmmoBox();
        }
    }

    private void SpawnAmmoBox()
    {
        // เช็กจำนวนกล่องที่อยู่ในฉากตอนนี้
        int currentBoxCount = FindObjectsByType<AmmoBoxPickup>(FindObjectsSortMode.None).Length;
        if (currentBoxCount >= maxBoxes) return;

        float rx = Random.Range(minX, maxX);
        float rz = Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(rx, spawnY, rz);

        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
    }
}
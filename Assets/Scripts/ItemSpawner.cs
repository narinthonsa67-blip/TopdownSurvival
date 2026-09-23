using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject ammoBoxPrefab; // กล่องกระสุน Prefab
    public float spawnInterval = 7f; // เสกทุก 7 วินาที
    public int maxBoxes = 3;         // ในฉากมีกล่องค้างไว้ได้ไม่เกิน 3 กล่อง
    public GameObject HealthBoxPrefab;  // กล่องยา Prefab ที่เพิ่มใหม่
    [Range(0, 100)] public float HealthSpawnChance = 30f; // โอกาสเกิดกล่องยา (เช่น 30%)

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
            SpawnItemBox();
        }
    }

    private void SpawnItemBox()
    {
        // ค้นหาจำนวนกล่องในฉาก (นับทั้งกระสุนและยา หรือใช้เช็กจำกัดจำนวนเดิม)
        int currentBoxCount = FindObjectsByType<AmmoBoxPickup>(FindObjectsSortMode.None).Length;
        if (currentBoxCount >= maxBoxes) return;

        // สุ่มพิกัดแกน X และ Z
        float rx = Random.Range(minX, maxX);
        float rz = Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(rx, spawnY, rz);

        // กำหนดให้เป็นกล่องกระสุนเป็นค่าเริ่มต้น
        GameObject itemToSpawn = ammoBoxPrefab;

        // สุ่มตัวเลข 0 - 100 ถ้าตรงเงื่อนไขจะเปลี่ยนเป็นกล่องยา
        if (HealthBoxPrefab != null && Random.Range(0f, 100f) <= HealthSpawnChance)
        {
            itemToSpawn = HealthBoxPrefab;
        }

        // เสกไอเทมที่เลือกไว้ลงในฉาก
        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
    }
}
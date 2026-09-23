using UnityEngine;

public class HealthBoxPickup : MonoBehaviour
{
    [Header("Heal Amount")]
    public int healAmount = 1; // จำนวนเลือดที่จะฟื้นฟู (เนื่องจาก maxHealth เริ่มต้นที่ 3 แนะนำให้ฟื้นฟู 1 ขีด)

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่เข้ามาชนมี Tag เป็น Player หรือไม่
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            // ตรวจสอบว่าพบ PlayerHealth และเลือดยังไม่เต็ม
            if (health != null && health.currentHealth < health.maxHealth)
            {
                health.Heal(healAmount); // เรียกฟังก์ชันฟื้นฟูเลือด
                Destroy(gameObject);     // ทำลายกล่องยาออกจากฉาก
            }
        }
    }
}
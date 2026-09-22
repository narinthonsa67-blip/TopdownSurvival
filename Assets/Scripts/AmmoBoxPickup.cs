using UnityEngine;

public class AmmoBoxPickup : MonoBehaviour
{
    [Header("Ammo Amount")]
    public int ammoAmount = 15; // จำนวนกระสุนที่ได้ต่อหนึ่งกล่อง

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจว่าสิ่งที่เดินมาชนมี Tag เป็น Player หรือไม่
        if (other.CompareTag("Player"))
        {
            PlayerAmmo ammo = other.GetComponent<PlayerAmmo>();
            if (ammo != null)
            {
                ammo.AddAmmo(ammoAmount);
                Destroy(gameObject); // เก็บแล้วกล่องหายไปจากฉาก
            }
        }
    }
}
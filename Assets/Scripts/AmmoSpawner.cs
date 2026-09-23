using UnityEngine; // นำเข้าไลบรารีหลักของ Unity เพื่อใช้งานฟังก์ชันและคอมโพเนนต์ต่าง ๆ
public class AmmoSpawner : MonoBehaviour // ประกาศคลาสชื่อ AmmoSpawner ที่สืบทอดคุณสมบัติมาจาก MonoBehaviour ของ Unity
{ // เปิดขอบเขตการทำงานของคลาส AmmoSpawner
    [Header("Settings")] // แสดงหัวข้อหมวดหมู่ "Settings" บนหน้าต่าง Inspector ในโปรแกรม Unity
    public GameObject ammoBoxPrefab; // กล่องกระสุน Prefab // ตัวแปรเก็บต้นแบบ (Prefab) กล่องกระสุนที่จะนำมาเสกในเกม
    public float spawnInterval = 7f; // เสกทุก 7 วินาที // กำหนดช่วงเวลารอคอยในการเสกกล่องกระสุนใหม่ออกมา (7 วินาที)
    public int maxBoxes = 3;         // ในฉากมีกล่องค้างไว้ได้ไม่เกิน 3 กล่อง // กำหนดจำนวนกล่องกระสุนสูงสุดที่อนุญาตให้มีอยู่ในฉากพร้อมกัน

    [Header("Spawn Boundaries")] // แสดงหัวข้อหมวดหมู่ขอบเขตพื้นที่เสกบนหน้าต่าง Inspector
    public float minX = -7f, maxX = 7f; // ขอบเขตพิกัดต่ำสุดและสูงสุดในแกน X สำหรับสุ่มตำแหน่งเสกกล่องกระสุน
    public float minZ = -7f, maxZ = 7f; // ขอบเขตพิกัดต่ำสุดและสูงสุดในแกน Z สำหรับสุ่มตำแหน่งเสกกล่องกระสุน
    public float spawnY = 0.5f; // ระดับความสูงคงที่ในแกน Y เพื่อให้กล่องกระสุนลอยอยู่เหนือพื้นพอดี

    private float timer; // ตัวแปรสำหรับใช้นับและสะสมเวลาในเกม

    private void Update() // ฟังก์ชันที่ทำงานซ้ำทุกเฟรมของการแสดงผล
    { // เปิดขอบเขตการทำงานของฟังก์ชัน Update
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) // ตรวจสอบว่าระบบ GameManager มีอยู่จริงและเกมจบลงแล้วหรือไม่
            return; // ถ้าเกมจบแล้วให้หยุดทำงานทันที ไม่ต้องนับเวลาหรือเสกกล่องเพิ่ม

        timer += Time.deltaTime; // นำเวลาที่ใช้ไปในแต่ละเฟรมสะสมเข้าไปในตัวแปรจับเวลาเรื่อย ๆ

        if (timer >= spawnInterval) // ตรวจสอบว่าเวลาสะสมถึงหรือเกินกำหนดเวลาเสก (7 วินาที) แล้วหรือยัง
        { // เปิดขอบเขตเงื่อนไขเมื่อครบกำหนดเวลา
            timer = 0f; // รีเซ็ตตัวจับเวลาให้กลับไปเริ่มต้นที่ 0 ใหม่อีกครั้ง
            SpawnAmmoBox(); // เรียกฟังก์ชันสร้างกล่องกระสุนขึ้นมาในฉาก
        } // ปิดขอบเขตเงื่อนไขเมื่อครบกำหนดเวลา
    } // ปิดขอบเขตการทำงานของฟังก์ชัน Update

    private void SpawnAmmoBox() // ฟังก์ชันสำหรับคำนวณและสร้างกล่องกระสุนออกมาในฉาก
    { // เปิดขอบเขตการทำงานของฟังก์ชัน SpawnAmmoBox
        // เช็กจำนวนกล่องที่อยู่ในฉากตอนนี้ // คำอธิบายเพื่อตรวจนับจำนวนกล่องกระสุนเดิม
        int currentBoxCount = FindObjectsByType<AmmoBoxPickup>(FindObjectsSortMode.None).Length; // ค้นหาและนับจำนวนวัตถุทั้งหมดในฉากที่มีคอมโพเนนต์ AmmoBoxPickup ติดอยู่
        if (currentBoxCount >= maxBoxes) return; // หากจำนวนกล่องในฉากมีเท่ากับหรือมากกว่าที่ตั้งไว้ (3 กล่อง) แล้ว ให้ยกเลิกการเสกทันที

        float rx = Random.Range(minX, maxX); // สุ่มพิกัดแนวนอนระหว่างขอบเขตแกน X ที่กำหนดไว้
        float rz = Random.Range(minZ, maxZ); // สุ่มพิกัดแนวลึกระหว่างขอบเขตแกน Z ที่กำหนดไว้
        Vector3 spawnPosition = new Vector3(rx, spawnY, rz); // สร้างเวกเตอร์พิกัดตำแหน่งเกิด 3 มิติจากค่าที่สุ่มและกำหนดไว้

        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity); // เสกวัตถุกล่องกระสุนออกมาตามพิกัดที่คำนวณ โดยตั้งค่าการหมุนเริ่มต้นตามมาตรฐาน
    } // ปิดขอบเขตการทำงานของฟังก์ชัน SpawnAmmoBox
} // ปิดขอบเขตของคลาส AmmoSpawner
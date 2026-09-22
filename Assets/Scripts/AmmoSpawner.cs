using UnityEngine; // นำเข้าไลบรารีหลักของ Unity เพื่อใช้งานฟังก์ชันและคอมโพเนนต์ต่าง ๆ
public class AmmoSpawner : MonoBehaviour // ประกาศคลาสชื่อ AmmoSpawner ที่สืบทอดคุณสมบัติมาจาก MonoBehaviour ของ Unity
{ // เปิดขอบเขตการทำงานของคลาส AmmoSpawner
    [Header("Settings")] // แสดงหัวข้อหมวดหมู่ "Settings" บนหน้าต่าง Inspector ในโปรแกรม Unity
    public GameObject ammoBoxPrefab; // กล่องกระสุน Prefab // ตัวแปรเก็บต้นแบบ (Prefab) ของกล่องกระสุนที่จะนำมาเสกในเกม
    public float spawnInterval = 7f; // เสกทุก 7 วินาที // กำหนดช่วงเวลารอคอยในการเสกกล่องกระสุนใหม่รอบละ 7 วินาที
    public int maxBoxes = 3;         // ในฉากมีกล่องค้างไว้ได้ไม่เกิน 3 กล่อง // กำหนดจำนวนกล่องกระสุนสูงสุดที่อนุญาตให้มีอยู่ในฉากพร้อมกัน

    [Header("Spawn Boundaries")] // แสดงหัวข้อหมวดหมู่ขอบเขตพื้นที่เกิดบนหน้าต่าง Inspector
    public float minX = -7f, maxX = 7f; // กำหนดขอบเขตพิกัดแนวนอน (แกน X) ต่ำสุดที่ -7 และสูงสุดที่ 7
    public float minZ = -7f, maxZ = 7f; // กำหนดขอบเขตพิกัดแนวลึก (แกน Z) ต่ำสุดที่ -7 และสูงสุดที่ 7
    public float spawnY = 0.5f; // กำหนดความสูงจากพื้น (แกน Y) ในการเสกกล่องให้อยู่ที่ระดับ 0.5 หน่วย

    private float timer; // ตัวแปรสำหรับใช้นับเวลาสะสมที่ผ่านไปในแต่ละรอบ

    private void Update() // ฟังก์ชันที่ Unity จะเรียกทำงานซ้ำอัตโนมัติในทุก ๆ เฟรมของการแสดงผล
    { // เปิดขอบเขตการทำงานของฟังก์ชัน Update
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) // ตรวจสอบว่ามี GameManager อยู่จริงไหม และเกมจบลงแล้วหรือยัง
            return; // ถ้าเกมจบลงแล้ว ให้หยุดการทำงานทันทีโดยไม่ต้องนับเวลาต่อ

        timer += Time.deltaTime; // บวกเวลาที่ใช้ไปในแต่ละเฟรมสะสมเข้าไปในตัวแปรจับเวลาเรื่อย ๆ

        if (timer >= spawnInterval) // ตรวจสอบว่าเวลาที่สะสมไว้นั้นถึงหรือเกินกำหนดเวลาเสก (7 วินาที) แล้วหรือยัง
        { // เปิดขอบเขตเงื่อนไขเมื่อครบกำหนดเวลา
            timer = 0f; // รีเซ็ตตัวจับเวลาให้กลับไปเริ่มต้นที่ 0 ใหม่อีกครั้ง
            SpawnAmmoBox(); // เรียกฟังก์ชันสร้างกล่องกระสุนขึ้นมาในฉาก
        } // ปิดขอบเขตเงื่อนไขเมื่อครบกำหนดเวลา
    } // ปิดขอบเขตการทำงานของฟังก์ชัน Update

    private void SpawnAmmoBox() // ฟังก์ชันสำหรับคำนวณและสร้างกล่องกระสุนออกมาในฉาก
    { // เปิดขอบเขตการทำงานของฟังก์ชัน SpawnAmmoBox
        // เช็กจำนวนกล่องที่อยู่ในฉากตอนนี้ // คำอธิบายเพื่อตรวจนับจำนวนกล่องกระสุนเดิม
        int currentBoxCount = FindObjectsByType<AmmoBoxPickup>(FindObjectsSortMode.None).Length; // ค้นหาและนับจำนวนวัตถุทั้งหมดในฉากที่มีคอมโพเนนต์ AmmoBoxPickup ติดอยู่
        if (currentBoxCount >= maxBoxes) return; // หากจำนวนกล่องในฉากมีเท่ากับหรือมากกว่าขีดจำกัดสูงสุด (3 กล่อง) แล้ว ให้ยกเลิกการสร้างทันที

        float rx = Random.Range(minX, maxX); // สุ่มพิกัดแกน X ตำแหน่งใหม่ระหว่างค่า minX ถึง maxX
        float rz = Random.Range(minZ, maxZ); // สุ่มพิกัดแกน Z ตำแหน่งใหม่ระหว่างค่า minZ ถึง maxZ
        Vector3 spawnPosition = new Vector3(rx, spawnY, rz); // นำพิกัด X, Y, Z ที่ได้มารวมเป็นจุดพิกัดแบบ 3 มิติ

        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity); // เสกวัตถุกล่องกระสุนจาก Prefab ไปวางที่พิกัด spawnPosition โดยไม่หมุนองศา (มุมหมุนปกติ)
    } // ปิดขอบเขตการทำงานของฟังก์ชัน SpawnAmmoBox
} // ปิดขอบเขตของคลาส AmmoSpawner
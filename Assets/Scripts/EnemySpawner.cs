using System.Collections;
using UnityEngine;
public class EnemySpawner : MonoBehaviour // นำเข้าไลบรารีระบบและยูนิตี้ พร้อมประกาศคลาสสคริปต์สปอว์นศัตรู
{ // เริ่มต้นบล็อกของคลาส EnemySpawner
    public GameObject enemyPrefab; // ต้นแบบ (Prefab) ศัตรูที่จะใช้เสกขึ้นมาในเกม
    public Transform[] spawnPoints; // อาเรย์เก็บตำแหน่งจุดเกิดต่างๆ ในฉากที่ศัตรูสามารถสุ่มเกิดได้
    public float spawnInterval = 1.5f; // ระยะเวลาหน่วงระหว่างการเสกศัตรูแต่ละรอบ (1.5 วินาที)
    public float firstSpawnDelay = 1f; // ระยะเวลาหน่วงก่อนเริ่มเสกศัตรูตัวแรกหลังเริ่มเกม (1 วินาที)

    private void Start() // ฟังก์ชันทำงานอัตโนมัติครั้งแรกเมื่อเกมเริ่ม
    { // เริ่มต้นบล็อกฟังก์ชัน Start
        StartCoroutine( // คำสั่งเริ่มรันฟังก์ชันประเภท Coroutine
            SpawnLoop() // เรียกใช้งานลูปการเกิดของศัตรู (SpawnLoop)
        ); // สิ้นสุดคำสั่ง StartCoroutine
    } // สิ้นสุดบล็อกฟังก์ชัน Start

    private IEnumerator SpawnLoop() // ฟังก์ชันแบบ Coroutine ที่วนลูปเสกศัตรูอย่างต่อเนื่อง
    { // เริ่มต้นบล็อกฟังก์ชัน SpawnLoop
        yield return new WaitForSeconds( // สั่งหยุดรอชั่วคราวก่อนเริ่มทำงานขั้นต่อไป
            firstSpawnDelay // รอตามเวลาดีเลย์เริ่มเกมที่ตั้งไว้ (1 วินาที)
        ); // สิ้นสุดคำสั่งรอเวลาเริ่มต้น

        while (GameManager.Instance != null && // วนลูปตราบใดที่ยังมีระบบ GameManager ทำงานอยู่
               !GameManager.Instance.IsGameOver) // และเกมยังไม่จบ (Game Over ยังไม่เป็นจริง)
        { // เริ่มบล็อกการทำงานในรอบการเสก
            SpawnEnemy(); // เรียกใช้ฟังก์ชันสุ่มตำแหน่งและเสกศัตรูออกมา 1 ตัว

            yield return new WaitForSeconds( // สั่งหยุดรอเวลาก่อนจะวนกลับมาเสกตัวถัดไป
                spawnInterval // หน่วงเวลาตามระยะห่างที่ตั้งไว้ (1.5 วินาที)
            ); // สิ้นสุดคำสั่งรอเวลาในแต่ละรอบ
        } // สิ้นสุดบล็อกลูป while
    } // สิ้นสุดบล็อกฟังก์ชัน SpawnLoop

    private void SpawnEnemy() // ฟังก์ชันคำนวณและสร้างศัตรูขึ้นมาในฉาก
    { // เริ่มต้นบล็อกฟังก์ชัน SpawnEnemy
        if (spawnPoints == null || // ตรวจสอบว่าไม่มีรายการจุดเกิด
            spawnPoints.Length == 0) // หรือไม่ได้ใส่จุดเกิดไว้ในอาเรย์เลยหรือไม่
        { // เริ่มเงื่อนไขกรณีไม่มีจุดเกิด
            return; // หยุดทำงานทันทีเพื่อป้องกันข้อผิดพลาดในการรัน
        } // สิ้นสุดเงื่อนไขกรณีไม่มีจุดเกิด

        int randomIndex = // ตัวแปรเก็บค่าตัวเลขอินเด็กซ์ที่ได้จากการสุ่ม
            Random.Range( // สุ่มตัวเลขจำนวนเต็ม
                0, // ค่าต่ำสุดที่เป็นไปได้ (เริ่มที่ 0)
                spawnPoints.Length // ค่าขอบเขตสูงสุด (เท่ากับจำนวนจุดเกิดทั้งหมด)
            ); // สิ้นสุดคำสั่งสุ่มตัวเลข

        Transform spawnPoint = // ตัวแปรเก็บพิกัดของจุดเกิดที่สุ่มได้
            spawnPoints[randomIndex]; // ดึงข้อมูลจุดเกิดจากอาเรย์ตามอินเด็กซ์ที่สุ่มได้

        Instantiate( // คำสั่งสร้าง GameObject ขึ้นมาในฉากจากต้นแบบ
            enemyPrefab, // วัตถุต้นแบบศัตรูที่ต้องการสร้าง
            spawnPoint.position, // กำหนดตำแหน่งเกิดตามจุดสปอว์นที่สุ่มได้
            spawnPoint.rotation // กำหนดทิศทางการหันหน้าตามจุดสปอว์นที่สุ่มได้
        ); // สิ้นสุดคำสั่ง Instantiate
    } // สิ้นสุดบล็อกฟังก์ชัน SpawnEnemy
} // สิ้นสุดบล็อกของคลาส EnemySpawner
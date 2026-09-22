using UnityEngine; // นำเข้าไลบรารีหลักของ Unity เพื่อใช้งานฟังก์ชันและคลาสพื้นฐาน

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour // บังคับให้มี Rigidbody และประกาศคลาส Bullet สืบทอดจาก MonoBehaviour
{ // เริ่มต้นบล็อกของคลาส Bullet
    public float speed = 15f; // กำหนดความเร็วในการพุ่งของกระสุน (15 หน่วย)
    public float lifeTime = 2f; // กำหนดระยะเวลาที่กระสุนจะอยู่ในฉากก่อนถูกทำลาย (2 วินาที)
    public int damage = 1; // กำหนดพลังโจมตีของกระสุน (1 หน่วย)

    // [เพิ่มใหม่] แรงดีด/เด้ง เมื่อกระสุนพุ่งชนเป้าหมาย
    [Header("Knockback")] // จัดหมวดหมู่หัวข้อ Knockback ในหน้าต่าง Inspector ของ Unity
    public float knockbackForce = 7f; // กำหนดค่าแรงผลัก/แรงกระเด็นใส่เป้าหมาย (7 หน่วย)

    private Rigidbody rb; // ตัวแปรสำหรับเก็บคอมโพเนนต์ Rigidbody ของกระสุนเพื่อใช้จัดการฟิสิกส์

    private void Start() // ฟังก์ชันทำงานครั้งแรกตอนเริ่มสร้างกระสุนขึ้นมาในฉาก
    { // เริ่มต้นบล็อกฟังก์ชัน Start
        rb = GetComponent<Rigidbody>(); // ดึงคอมโพเนนต์ Rigidbody ของตัวกระสุนมาเก็บในตัวแปร rb

        rb.linearVelocity = // สั่งกำหนดความเร็วเชิงเส้น (ความเร็วเคลื่อนที่) ให้กับ Rigidbody
            transform.forward * speed; // คำนวณเวกเตอร์พุ่งไปข้างหน้าคูณด้วยความเร็วที่ตั้งไว้

        Destroy( // คำสั่งสำหรับลบหรือทำลาย GameObject ออกจากฉาก
            gameObject, // ระบุให้ทำลายตัวกระสุนนี้
            lifeTime // หน่วงเวลาการทำลายตามค่าที่กำหนด (2 วินาที)
        ); // สิ้นสุดคำสั่ง Destroy
    } // สิ้นสุดบล็อกฟังก์ชัน Start

    private void OnTriggerEnter(Collider other) // ฟังก์ชันทำงานเมื่อกระสุนพุ่งเข้าชน Collider ที่เป็น Trigger
    { // เริ่มต้นบล็อกฟังก์ชัน OnTriggerEnter
        // ป้องกัน Bullet ชน Player ที่ยิงมันออกมา
        if (other.GetComponent<PlayerController>() != null) // ตรวจสอบว่าวัตถุที่ชนมี PlayerController หรือไม่
        { // เริ่มเงื่อนไขกรณีชน Player
            return; // หยุดการทำงานทันทีเพื่อไม่ให้ทำร้ายคนยิงและไม่ทำลายกระสุน
        } // สิ้นสุดเงื่อนไขกรณีชน Player

        // ตรวจว่าเป็น Enemy หรือไม่
        if (other.TryGetComponent<EnemyController>( // เช็กว่าวัตถุที่ชนมีคอมโพเนนต์ EnemyController หรือไม่
            out EnemyController enemy)) // ถ้ามี ให้นำไปเก็บไว้ในตัวแปรชื่อ enemy ทันที
        { // เริ่มเงื่อนไขกรณีชนศัตรู
            // [แก้ไขตรงนี้] ส่งทิศทางที่กระสุนพุ่งไป (transform.forward) และแรงดีดไปให้ศัตรู
            enemy.TakeDamage(damage, transform.forward, knockbackForce); // เรียกใช้ฟังก์ชันรับดาเมจของศัตรู พร้อมส่งค่าความเสียหาย ทิศทาง และแรงผลัก
        } // สิ้นสุดเงื่อนไขกรณีชนศัตรู

        // Bullet หายเมื่อชนวัตถุ
        Destroy(gameObject); // ทำลายกระสุนทิ้งทันทีเมื่อชนสิ่งกีดขวางหรือศัตรู
    } // สิ้นสุดบล็อกฟังก์ชัน OnTriggerEnter
} // สิ้นสุดบล็อกของคลาส Bullet
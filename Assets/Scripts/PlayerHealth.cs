using System.Collections;
using UnityEngine; // นำเข้าไลบรารีระบบสำหรับการทำ Coroutine และคำสั่งพื้นฐานของ Unity

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour // บังคับให้มี Rigidbody และประกาศคลาส PlayerHealth สำหรับจัดการพลังชีวิตผู้เล่น
{ // เริ่มต้นบล็อกของคลาส PlayerHealth
    public int maxHealth = 3; // กำหนดค่าพลังชีวิตสูงสุดของผู้เล่น (เริ่มต้นที่ 3 หน่วย)
    private int currentHealth; // ตัวแปรสำหรับเก็บพลังชีวิตปัจจุบันของผู้เล่นในเกม

    [Header("Effects")] // จัดกลุ่มหัวข้อ "Effects" ในหน้าต่าง Inspector ของ Unity
    public float flashDuration = 0.15f; // กำหนดระยะเวลาที่ตัวละครจะกระพริบเป็นสีแดงเมื่อโดนโจมตี (0.15 วินาที)

    private PlayerController playerController; // ตัวแปรสำหรับอ้างอิงไปยังสคริปต์ควบคุมตัวละคร PlayerController
    private Renderer[] renderers; // อาเรย์สำหรับเก็บคอมโพเนนต์แสดงผลโมเดล (Renderer) ทุกชิ้นส่วน
    private Color[] originalColors; // อาเรย์สำหรับจำสีดั้งเดิมของแต่ละชิ้นส่วนโมเดลไว้

    private void Start() // ฟังก์ชันทำงานอัตโนมัติรอบแรกเมื่อเริ่มเกม
    { // เริ่มต้นบล็อกฟังก์ชัน Start
        playerController = GetComponent<PlayerController>(); // ดึงสคริปต์ PlayerController บนตัวละครมาเก็บไว้ใช้งาน
        currentHealth = maxHealth; // รีเซ็ตพลังชีวิตปัจจุบันให้เต็มเท่ากับพลังชีวิตสูงสุด
        UpdateHealthUI(); // สั่งให้อัปเดตตัวเลขพลังชีวิตขึ้นบนหน้าจอ UI ทันที

        renderers = GetComponentsInChildren<Renderer>(); // ค้นหา Renderer ทุกชิ้นในตัวผู้เล่นและวัตถุลูกทั้งหมด
        if (renderers != null && renderers.Length > 0) // ตรวจสอบว่ามีชิ้นส่วน Renderer อยู่จริงหรือไม่
        { // เริ่มบล็อกเงื่อนไขเมื่อพบ Renderer
            originalColors = new Color[renderers.Length]; // สร้างอาเรย์สำหรับเก็บสีดั้งเดิมให้ขนาดเท่ากับจำนวนชิ้นส่วน
            for (int i = 0; i < renderers.Length; i++) // วนลูปอ่านข้อมูลทีละชิ้นส่วน
            { // เริ่มลูปบันทึกสี
                originalColors[i] = renderers[i].material.color; // ดึงสีปัจจุบันของแต่ละชิ้นส่วนมาเก็บสำรองไว้
            } // สิ้นสุดลูปบันทึกสี
        } // สิ้นสุดบล็อกเงื่อนไขเมื่อพบ Renderer
    } // สิ้นสุดบล็อกฟังก์ชัน Start

    public void TakeDamage(int damage, Vector3 direction, float knockbackForce) // ฟังก์ชันรับความเสียหาย รับค่าดาเมจ ทิศทางแรงผลัก และขนาดแรงผลัก
    { // เริ่มต้นบล็อกฟังก์ชัน TakeDamage
        currentHealth -= damage; // นำความเสียหายที่ได้รับไปหักลบออกจากพลังชีวิตปัจจุบัน

        if (currentHealth < 0) // ตรวจสอบว่าพลังชีวิตติดลบหรือไม่
        { // เริ่มบล็อกป้องกันเลือดติดลบ
            currentHealth = 0; // ล็อกค่าพลังชีวิตต่ำสุดไว้ที่ 0 ไม่ให้ติดลบ
        } // สิ้นสุดบล็อกป้องกันเลือดติดลบ

        UpdateHealthUI(); // อัปเดตตัวเลขพลังชีวิตที่เปลี่ยนไปขึ้นบนหน้าจอ UI

        StartCoroutine(FlashRed()); // เริ่มต้นการทำงานของ Coroutine ให้ตัวละครเปลี่ยนเป็นสีแดงชั่วขณะ

        // [แก้ไขตรงนี้] ส่งแรงเด้งผ่าน PlayerController
        if (playerController != null) // ตรวจสอบว่ามีสคริปต์ PlayerController เชื่อมต่ออยู่หรือไม่
        { // เริ่มบล็อกส่งแรงกระเด็น
            playerController.ApplyKnockback(direction, knockbackForce, 0.2f); // สั่งให้ผู้เล่นกระเด็นตามทิศทางและแรงที่กำหนดเป็นเวลา 0.2 วินาที
        } // สิ้นสุดบล็อกส่งแรงกระเด็น

        if (currentHealth <= 0) // ตรวจสอบว่าพลังชีวิตหมดลงแล้วหรือยัง
        { // เริ่มบล็อกเงื่อนไขเมื่อผู้เล่นตาย
            Die(); // เรียกฟังก์ชันจัดการการตายของผู้เล่น
        } // สิ้นสุดบล็อกเงื่อนไขเมื่อผู้เล่นตาย
    } // สิ้นสุดบล็อกฟังก์ชัน TakeDamage

    private IEnumerator FlashRed() // ฟังก์ชันแบบหน่วงเวลาสำหรับทำให้โมเดลกระพริบแดงแล้วคืนสีเดิม
    { // เริ่มต้นบล็อก Coroutine FlashRed
        if (renderers == null) yield break; // หากไม่มี Renderer ให้ยกเลิกการทำงานทันที

        for (int i = 0; i < renderers.Length; i++) // วนลูปเปลี่ยนสีชิ้นส่วนโมเดลทั้งหมด
        { // เริ่มลูปเปลี่ยนเป็นสีแดง
            renderers[i].material.color = Color.red; // สั่งเปลี่ยนสีโมเดลชิ้นนั้นๆ ให้กลายเป็นสีแดง
        } // สิ้นสุดลูปเปลี่ยนเป็นสีแดง

        yield return new WaitForSeconds(flashDuration); // สั่งหยุดรอเวลาตามค่า flashDuration (0.15 วินาที)

        for (int i = 0; i < renderers.Length; i++) // วนลูปเพื่อคืนค่าสีเดิมให้กับโมเดลทุกชิ้น
        { // เริ่มลูปคืนค่าสีเดิม
            renderers[i].material.color = originalColors[i]; // เปลี่ยนสีโมเดลกลับไปเป็นสีเดิมที่บันทึกไว้ในตอนแรก
        } // สิ้นสุดลูปคืนค่าสีเดิม
    } // สิ้นสุดบล็อก Coroutine FlashRed

    private void UpdateHealthUI() // ฟังก์ชันสำหรับส่งค่าพลังชีวิตไปอัปเดตบนหน้าจอ UI
    { // เริ่มต้นบล็อกฟังก์ชัน UpdateHealthUI
        if (GameManager.Instance != null) // ตรวจสอบว่าระบบ GameManager ทำงานอยู่ในฉากหรือไม่
        { // เริ่มบล็อกอัปเดต UI
            GameManager.Instance.SetHealth( // ส่งค่าพลังชีวิตไปให้ GameManager นำไปแสดงผล
                currentHealth, // ส่งค่าพลังชีวิตปัจจุบัน
                maxHealth // ส่งค่าพลังชีวิตสูงสุด
            ); // สิ้นสุดคำสั่ง SetHealth
        } // สิ้นสุดบล็อกอัปเดต UI
    } // สิ้นสุดบล็อกฟังก์ชัน UpdateHealthUI

    private void Die() // ฟังก์ชันจัดการเมื่อพลังชีวิตผู้เล่นหมดลง
    { // เริ่มต้นบล็อกฟังก์ชัน Die
        if (GameManager.Instance != null) // ตรวจสอบว่ามี GameManager อยู่ในฉากหรือไม่
        { // เริ่มบล็อกแจ้งเตือนเกมจบ
            GameManager.Instance.GameOver(); // แจ้งระบบ GameManager ว่าผู้เล่นพ่ายแพ้เพื่อแสดงหน้าจอ GameOver
        } // สิ้นสุดบล็อกแจ้งเตือนเกมจบ
    } // สิ้นสุดบล็อกฟังก์ชัน Die
} // สิ้นสุดบล็อกของคลาส PlayerHealth
using UnityEngine; // เรียกใช้งานไลบรารีพื้นฐานของ Unity Engine
using UnityEngine.InputSystem; // เรียกใช้งานระบบจัดการ Input System ตัวใหม่ของ Unity

[RequireComponent(typeof(Rigidbody))] // บังคับว่า GameObject นี้ต้องมีคอมโพเนนต์ Rigidbody เสมอ
public class PlayerController : MonoBehaviour // ประกาศคลาส PlayerController สืบทอดจาก MonoBehaviour
{ // เริ่มต้นบล็อกของคลาส
    [Header("Movement")] // หัวข้อการตั้งค่าการเคลื่อนที่ใน Inspector
    public float moveSpeed = 6f; // ความเร็วในการเดินปกติของผู้เล่น

    [Header("Dash Settings")] // หัวข้อการตั้งค่าระบบพุ่ง (Dash) ใน Inspector
    public float dashSpeed = 16f; // ความเร็วขณะที่พุ่งตัว
    public float dashDuration = 0.15f; // ระยะเวลาในการพุ่งตัว (วินาที)
    public float dashCooldown = 1f; // ระยะเวลาคูลดาวน์ก่อนจะกดพุ่งตัวได้อีกครั้ง (วินาที)

    [Header("Shooting")] // หัวข้อการตั้งค่าการยิงปืนใน Inspector
    public GameObject bulletPrefab; // Prefab ของลูกกระสุนที่จะเสกออกมา
    public Transform firePoint; // ตำแหน่งและทิศทางที่กระสุนจะพุ่งออกจากปากกระบอกปืน
    public float shootCooldown = 0.2f; // ระยะเวลาหน่วงระหว่างการยิงแต่ละนัด (วินาที)

    private Rigidbody rb; // ตัวแปรเก็บคอมโพเนนต์ Rigidbody ของผู้เล่น
    private Camera mainCamera; // ตัวแปรเก็บกล้องหลักของฉาก
    private Vector3 moveDirection; // เวกเตอร์ทิศทางการเคลื่อนที่จากการกดปุ่มเดิน
    private Quaternion targetRotation; // การหมุนเป้าหมายที่ผู้เล่นต้องหันหน้าไป
    private float nextShootTime; // เวลาที่จะสามารถยิงกระสุนนัดถัดไปได้

    // ตัวแปรควบคุมระบบพุ่ง (Dash)
    private bool isDashing = false; // ตัวแปรบอกสถานะว่ากำลังพุ่งตัวอยู่หรือไม่
    private float dashEndTime; // เวลาที่การพุ่งตัวรอบนี้จะสิ้นสุดลง
    private float nextDashTime; // เวลาที่จะสามารถกดพุ่งตัวครั้งถัดไปได้
    private Vector3 dashDirection; // ทิศทางที่จะพุ่งตัวไป

    // ตัวแปรควบคุมการโดนแรงกระแทก (Knockback)
    private bool isKnockedBack = false; // ตัวแปรระบุสถานะว่ากำลังโดนแรงผลักกระเด็นอยู่หรือไม่
    private float knockbackEndTime; // เวลาที่อาการกระเด็นถอยหลังจะสิ้นสุดลง

    private void Awake() // ฟังก์ชันเริ่มต้นทำงานครั้งแรกสุดตอนเปิดเกม (ก่อน Start)
    { // เริ่มต้นบล็อกฟังก์ชัน Awake
        rb = GetComponent<Rigidbody>(); // ดึงคอมโพเนนต์ Rigidbody บนตัวผู้เล่นมาเก็บไว้ในตัวแปร
        mainCamera = Camera.main; // ค้นหาและเก็บกล้องหลักที่มี Tag MainCamera ในฉาก
        targetRotation = transform.rotation; // กำหนดค่ามุมหันเริ่มต้นให้เท่ากับมุมปัจจุบันของตัวละคร
    } // สิ้นสุดบล็อกฟังก์ชัน Awake

    private void Update() // ฟังก์ชันที่ทำงานซ้ำทุกเฟรมของการแสดงผล
    { // เริ่มต้นบล็อกฟังก์ชัน Update
        // ถ้าเกมจบแล้ว ไม่รับ Input ใดๆ ทั้งสิ้น
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        { // เริ่มบล็อกถ้าเกมจบ
            moveDirection = Vector3.zero; // รีเซ็ตทิศทางการเดินให้หยุดนิ่ง
            return; // หยุดการทำงานของ Update ทันที
        } // สิ้นสุดบล็อกถ้าเกมจบ

        // ตรวจสอบการหมดเวลาพุ่ง (Dash)
        if (isDashing && Time.time >= dashEndTime)
        { // เริ่มบล็อกหมดเวลาพุ่ง
            isDashing = false; // ยกเลิกสถานะพุ่งตัว
        } // สิ้นสุดบล็อกหมดเวลาพุ่ง

        // ตรวจสอบการหมดเวลาแรงกระแทก (Knockback)
        if (isKnockedBack && Time.time >= knockbackEndTime)
        { // เริ่มบล็อกหมดเวลากระเด็น
            isKnockedBack = false; // ยกเลิกสถานะกระเด็น คืนการควบคุมให้ผู้เล่น
        } // สิ้นสุดบล็อกหมดเวลากระเด็น

        ReadMovementInput(); // เรียกฟังก์ชันอ่านค่าปุ่มเดิน WASD
        ReadDashInput(); // เรียกฟังก์ชันตรวจจับการกดปุ่ม Spacebar เพื่อพุ่งตัว
        AimAtMouse(); // เรียกฟังก์ชันคำนวณการเล็งหน้าตัวละครตามตำแหน่งเมาส์
        ReadShootingInput(); // เรียกฟังก์ชันตรวจจับการคลิกเมาส์ซ้ายเพื่อยิง
    } // สิ้นสุดบล็อกฟังก์ชัน Update

    private void FixedUpdate() // ฟังก์ชันคำนวณระบบฟิสิกส์ตามคาบเวลาคงที่
    { // เริ่มต้นบล็อกฟังก์ชัน FixedUpdate
        if (isKnockedBack) return; // ถ้ากำลังโดนผลักกระเด็นอยู่ ปล่อยให้แรงฟิสิกส์ทำงาน ไม่สั่งเคลื่อนที่ทับ

        if (isDashing) // ถ้ากำลังอยู่ในสถานะพุ่งตัว
        { // เริ่มบล็อกพุ่งตัว
            // บังคับความเร็วพุ่งตัวไปข้างหน้าตามทิศทาง Dash ทันที
            rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, 0f, dashDirection.z * dashSpeed);
        } // สิ้นสุดบล็อกพุ่งตัว
        else // ถ้าเดินตามปกติ
        { // เริ่มบล็อกเดินปกติ
            MovePlayer(); // เรียกฟังก์ชันควบคุมความเร็วการเดินของผู้เล่น
        } // สิ้นสุดบล็อกเดินปกติ

        RotatePlayer(); // เรียกฟังก์ชันหมุนตัวละครไปตามทิศทางเป้าหมาย
    } // สิ้นสุดบล็อกฟังก์ชัน FixedUpdate

    private void ReadMovementInput() // ฟังก์ชันตรวจจับการกดปุ่มคีย์บอร์ด WASD
    { // เริ่มต้นบล็อก ReadMovementInput
        if (Keyboard.current == null) return; // หากไม่พบคีย์บอร์ด ให้ข้ามไป

        float horizontal = 0f; // ตัวแปรเก็บแกนแนวนอน
        float vertical = 0f; // ตัวแปรเก็บแกนแนวตั้ง

        if (Keyboard.current.wKey.isPressed) vertical += 1f; // ปุ่ม W เดินหน้า
        if (Keyboard.current.sKey.isPressed) vertical -= 1f; // ปุ่ม S ถอยหลัง
        if (Keyboard.current.dKey.isPressed) horizontal += 1f; // ปุ่ม D เดินขวา
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f; // ปุ่ม A เดินซ้าย

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized; // ปรับความยาวเวกเตอร์เป็น 1
    } // สิ้นสุดบล็อก ReadMovementInput

    private void ReadDashInput() // ฟังก์ชันตรวจจับการกดปุ่ม Spacebar เพื่อพุ่งตัว (Dash)
    { // เริ่มต้นบล็อก ReadDashInput
        if (Keyboard.current == null) return; // หากไม่พบคีย์บอร์ด ให้ข้ามไป

        // ตรวจสอบว่ากด Spacebar + ผ่านช่วงคูลดาวน์แล้ว + ตอนนี้ไม่ได้กำลังพุ่งอยู่
        if (Keyboard.current.shiftKey.wasPressedThisFrame && Time.time >= nextDashTime && !isDashing)
        { // เริ่มบล็อกเปิดใช้งานพุ่งตัว
            isDashing = true; // เปิดสถานะกำลังพุ่งตัว
            dashEndTime = Time.time + dashDuration; // ตั้งเวลาสิ้นสุดการพุ่ง
            nextDashTime = Time.time + dashCooldown; // ตั้งเวลาคูลดาวน์ครั้งต่อไป

            // ถ้ากำลังกดปุ่มเดินให้พุ่งไปทางที่เดิน ถ้าอยู่นิ่งๆ ให้พุ่งไปข้างหน้าตามหน้าตัวละคร
            dashDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward;
        } // สิ้นสุดบล็อกเปิดใช้งานพุ่งตัว
    } // สิ้นสุดบล็อก ReadDashInput

    private void MovePlayer() // ฟังก์ชันจัดการความเร็วการเคลื่อนที่ปกติ
    { // เริ่มต้นบล็อก MovePlayer
        Vector3 velocity = moveDirection * moveSpeed; // ความเร็วตามทิศทางการกด
        rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z); // กำหนดความเร็วให้ Rigidbody ในแนวราบ
    } // สิ้นสุดบล็อก MovePlayer

    private void AimAtMouse() // ฟังก์ชันคำนวณการหมุนตัวละครตามตำแหน่งเมาส์
    { // เริ่มต้นบล็อก AimAtMouse
        if (Mouse.current == null || mainCamera == null) return; // ถ้าไม่พบเมาส์หรือกล้อง ให้ข้ามไป

        Vector2 mousePosition = Mouse.current.position.ReadValue(); // ตำแหน่งเมาส์บนหน้าจอ
        Ray ray = mainCamera.ScreenPointToRay(mousePosition); // ยิงรังสีจากกล้องผ่านตำแหน่งเมาส์
        Plane groundPlane = new Plane(Vector3.up, transform.position); // สร้างระนาบที่ระดับความสูงของผู้เล่น

        if (groundPlane.Raycast(ray, out float distance)) // ถ้ารังสีตกกระทบกับพื้น
        { // เริ่มบล็อกจุดตกกระทบ
            Vector3 hitPoint = ray.GetPoint(distance); // พิกัด 3 มิติของจุดที่ชี้
            Vector3 lookDirection = hitPoint - transform.position; // หาเวกเตอร์หันหน้าเข้าหาเป้าหมาย
            lookDirection.y = 0f; // ล็อกแกน Y เป็น 0 เพื่อให้หมุนแนวราบเท่านั้น

            if (lookDirection.sqrMagnitude > 0.01f) // ถ้ามีระยะห่างมากพอ
            { // เริ่มบล็อกตั้งองศาหมุน
                targetRotation = Quaternion.LookRotation(lookDirection); // แปลงเป็นองศาการหัน
            } // สิ้นสุดบล็อกตั้งองศาหมุน
        } // สิ้นสุดบล็อกจุดตกกระทบ
    } // สิ้นสุดบล็อก AimAtMouse

    private void RotatePlayer() // ฟังก์ชันสั่งหมุนตัวละครผ่านฟิสิกส์
    { // เริ่มต้นบล็อก RotatePlayer
        rb.MoveRotation(targetRotation); // สั่ง Rigidbody หมุนตัวละครไปยังเป้าหมาย
    } // สิ้นสุดบล็อก RotatePlayer

    private void ReadShootingInput() // ฟังก์ชันตรวจจับการคลิกเมาส์เพื่อยิง
    { // เริ่มต้นบล็อก ReadShootingInput
        if (Mouse.current == null) return; // ตรวจสอบเมาส์

        if (Mouse.current.leftButton.isPressed && Time.time >= nextShootTime) // ถ้าคลิกซ้ายและพ้นช่วงคูลดาวน์แล้ว
        { // เริ่มบล็อกสั่งยิง
            Shoot(); // เรียกฟังก์ชันสร้างกระสุน
            nextShootTime = Time.time + shootCooldown; // ตั้งเวลาคูลดาวน์การยิงนัดถัดไป
        } // สิ้นสุดบล็อกสั่งยิง
    } // สิ้นสุดบล็อก ReadShootingInput

    private void Shoot() // ฟังก์ชันสร้างกระสุนและจัดการระบบกระสุน
    { // เริ่มต้นบล็อก Shoot
        PlayerAmmo ammo = GetComponent<PlayerAmmo>(); // ตรวจสอบคอมโพเนนต์ระบบกระสุน
        if (ammo != null) // ถ้ามีระบบกระสุนติดตั้งอยู่
        { // เริ่มบล็อกตรวจเช็คกระสุน
            if (!ammo.CanShoot()) return; // ถ้ากระสุนหมด สั่งหยุดยิง ไม่เสกกระสุน
            ammo.ConsumeAmmo(); // ลดจำนวนกระสุนลง 1 นัด
        } // สิ้นสุดบล็อกตรวจเช็คกระสุน

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // เสกกระสุนออกจากตำแหน่งปากกระบอกปืน
    } // สิ้นสุดบล็อก Shoot

    // ฟังก์ชันรับแรงกระแทกแบบ 3 พารามิเตอร์ (แก้ Error CS1061 ของ PlayerHealth)
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    { // เริ่มบล็อก ApplyKnockback แบบ 3 ค่า
        isKnockedBack = true; // เปิดสถานะโดนกระเด็น
        isDashing = false; // ยกเลิกการพุ่งทันทีถ้าโดนชน
        knockbackEndTime = Time.time + duration; // ตั้งเวลาหมดสภาพกระเด็น

        Vector3 pushDir = direction.normalized; // ปรับเวกเตอร์ทิศทางเป็น 1 หน่วย
        pushDir.y = 0f; // ตัดแกน Y ทิ้งเพื่อให้กระเด็นแนวราบ

        rb.linearVelocity = Vector3.zero; // เคลียร์ความเร็วเดิมก่อน
        rb.AddForce(pushDir * force, ForceMode.Impulse); // ใส่แรงผลักกระแทกทันที
    } // สิ้นสุดบล็อก ApplyKnockback แบบ 3 ค่า

    // ฟังก์ชันรับแรงกระแทกแบบ 2 พารามิเตอร์ (Overload เสริม)
    public void ApplyKnockback(Vector3 direction, float force)
    { // เริ่มบล็อก ApplyKnockback แบบ 2 ค่า
        ApplyKnockback(direction, force, 0.2f); // ส่งต่อให้ฟังก์ชัน 3 ค่า โดยใช้เวลามาตรฐาน 0.2 วินาที
    } // สิ้นสุดบล็อก ApplyKnockback แบบ 2 ค่า
} // สิ้นสุดบล็อกคลาส PlayerController
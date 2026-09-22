<<<<<<< Updated upstream
using UnityEngine;
using UnityEngine.InputSystem; // นำเข้าไลบรารีหลักของ Unity และระบบ Input System ใหม่

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour // บังคับให้ GameObject ต้องมี Rigidbody และประกาศคลาสควบคุมผู้เล่น
{ // เริ่มต้นบล็อกของคลาส PlayerController
    [Header("Movement")] // จัดกลุ่มหัวข้อ "Movement" ในหน้าต่าง Inspector ของ Unity
    public float moveSpeed = 6f; // กำหนดความเร็วในการเดินปกติของผู้เล่น (6 หน่วย)

    // การตั้งค่า Dash
    [Header("Dash")] // จัดกลุ่มหัวข้อ "Dash" ในหน้าต่าง Inspector
    public float dashSpeed = 16f;        // ความเร็วตอนพุ่ง // กำหนดความเร็วขณะพุ่งตัว (16 หน่วย)
    public float dashDuration = 0.15f;   // เวลาที่พุ่ง (เสี้ยววินาที) // กำหนดระยะเวลาการพุ่งตัวในแต่ละครั้ง (0.15 วินาที)
    public float dashCooldown = 1f;      // รอคูลดาวน์กี่วินาทีถึงจะพุ่งได้อีก // ระยะเวลาคูลดาวน์ที่ต้องรอก่อนกดพุ่งได้อีกครั้ง (1 วินาที)
    private float dashTimer = 0f;        // ตัวนับเวลาพุ่ง // ตัวแปรสำหรับนับเวลาถอยหลังระหว่างที่กำลังพุ่งตัวอยู่
    private float nextDashTime = 0f;     // เวลาที่กด Dash ครั้งถัดไปได้ // บันทึกช่วงเวลาในเกมที่จะอนุญาตให้พุ่งตัวครั้งต่อไปได้
    private Vector3 dashDirection;       // ทิศทางที่จะพุ่งไป // เวกเตอร์เก็บทิศทางที่ผู้เล่นจะพุ่งตัวไป

    [Header("Shooting")] // จัดกลุ่มหัวข้อ "Shooting" ในหน้าต่าง Inspector
    public GameObject bulletPrefab; // พรีแฟบกระสุนที่จะสร้างออกมาเมื่อทำการยิง
    public Transform firePoint; // ตำแหน่งและทิศทางของปากกระบอกปืนที่จะเสกกระสุนออกมา
    public float shootCooldown = 0.2f; // ระยะเวลาหน่วงระหว่างการยิงแต่ละนัด (0.2 วินาที)

    private Rigidbody rb; // ตัวแปรเก็บคอมโพเนนต์ Rigidbody ของผู้เล่นสำหรับควบคุมการเคลื่อนที่ฟิสิกส์
    private Camera mainCamera; // ตัวแปรเก็บการอ้างอิงถึงกล้องหลักในฉาก
    private Vector3 moveDirection; // เวกเตอร์เก็บทิศทางที่ผู้เล่นกำลังเคลื่อนที่
    private Quaternion targetRotation; // ตัวแปรเก็บมุมการหมุนเป้าหมายที่ผู้เล่นกำลังหันหน้าไป
    private float nextShootTime; // ตัวแปรเก็บช่วงเวลาที่จะสามารถยิงนัดถัดไปได้
    private float knockbackTimer = 0f; // ตัวนับเวลาถอยหลังของสถานะโดนแรงผลักกระเด็น

    private void Awake() // ฟังก์ชันทำงานก่อน Start ทันทีที่สคริปต์ถูกโหลด
    { // เริ่มต้นบล็อกฟังก์ชัน Awake
        rb = GetComponent<Rigidbody>(); // ดึงคอมโพเนนต์ Rigidbody บนตัวผู้เล่นมาเก็บในตัวแปร rb
        mainCamera = Camera.main; // ค้นหาและบันทึกกล้องหลักของฉากไว้ในตัวแปร mainCamera
        targetRotation = transform.rotation; // ตั้งค่าการหมุนเริ่มต้นให้เท่ากับทิศที่หันอยู่ปัจจุบัน
    } // สิ้นสุดบล็อกฟังก์ชัน Awake

    private void Update() // ฟังก์ชันทำงานซ้ำทุกเฟรม สำหรับตรวจจับการกดปุ่มและคำนวณการเล็ง
    { // เริ่มต้นบล็อกฟังก์ชัน Update
        if (GameManager.Instance != null && // ตรวจสอบว่ามี GameManager ในเกมหรือไม่
            GameManager.Instance.IsGameOver) // และตรวจสอบว่าเกมจบลงแล้วหรือยัง
        { // เริ่มเงื่อนไขเมื่อเกมจบ
            moveDirection = Vector3.zero; // รีเซ็ตทิศทางการเคลื่อนที่ให้หยุดนิ่ง
            return; // หยุดการทำงานของ Update ทันทีเพื่อไม่ให้ควบคุมตัวละครต่อได้
        } // สิ้นสุดเงื่อนไขเมื่อเกมจบ

        ReadMovementInput(); // อ่านค่าการกดปุ่มเดิน (W, A, S, D)
        AimAtMouse(); // คำนวณองศาเพื่อหันหน้าตัวละครตามตำแหน่งเมาส์
        ReadShootingInput(); // ตรวจสอบการกดปุ่มเมาส์เพื่อยิง
        ReadDashInput(); // อ่านปุ่ม Dash // ตรวจสอบการกดปุ่ม Spacebar เพื่อพุ่งตัว
    } // สิ้นสุดบล็อกฟังก์ชัน Update

    private void FixedUpdate() // ฟังก์ชันทำงานตามรอบฟิสิกส์คงที่ เหมาะสำหรับคำนวณแรงและการเคลื่อนที่
    { // เริ่มต้นบล็อกฟังก์ชัน FixedUpdate
        // ถ้ากำลังอยู่ในสถานะ Dash ให้พุ่งไปข้างหน้าโดยไม่สนใจการเดินปกติ
        if (dashTimer > 0f) // ตรวจสอบว่ากำลังอยู่ในช่วงเวลา Dash อยู่หรือไม่
        { // เริ่มเงื่อนไขขณะพุ่งตัว
            dashTimer -= Time.fixedDeltaTime; // นับเวลา Dash ถอยหลังตามรอบฟิสิกส์
            rb.linearVelocity = dashDirection * dashSpeed; // กำหนดความเร็วให้พุ่งตัวตามทิศทาง Dash ทันที
            RotatePlayer(); // อัปเดตการหันหน้าตัวละครตามเป้าหมาย
            return; // ข้ามโค้ดการเดินปกติข้างล่างไปเพื่อไม่ให้ความเร็วการเดินมาหักล้าง
        } // สิ้นสุดเงื่อนไขขณะพุ่งตัว

        if (knockbackTimer > 0f) // ตรวจสอบว่ากำลังติดสถานะโดนผลักกระเด็นอยู่หรือไม่
        { // เริ่มเงื่อนไขขณะโดนผลักกระเด็น
            knockbackTimer -= Time.fixedDeltaTime; // นับเวลาสถานะกระเด็นถอยหลัง
            RotatePlayer(); // ยังคงให้อัปเดตการหันหน้าได้ตามปกติ
            return; // ข้ามการเดินปกติเพื่อปล่อยให้ตัวละครปลิวตามแรงฟิสิกส์
        } // สิ้นสุดเงื่อนไขขณะโดนผลักกระเด็น

        MovePlayer(); // คำนวณและสั่งให้ตัวละครเคลื่อนที่ตามปกติ
        RotatePlayer(); // หมุนตัวละครไปยังมุมเป้าหมาย
    } // สิ้นสุดบล็อกฟังก์ชัน FixedUpdate

    // เช็กการกด Spacebar
    private void ReadDashInput() // ฟังก์ชันสำหรับอ่านและสั่งการพุ่งตัว
    { // เริ่มต้นบล็อกฟังก์ชัน ReadDashInput
        if (Keyboard.current == null) return; // หากไม่พบคีย์บอร์ดให้ยกเลิกการทำงานทันที

        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time >= nextDashTime) // ตรวจว่ากด Spacebar ในเฟรมนี้และหมดเวลาคูลดาวน์แล้วหรือไม่
        { // เริ่มเงื่อนไขการทำงานของ Dash
            // ถ้ากำลังกดปุ่มเดินอยู่ ให้พุ่งไปตามทิศที่เดิน ถ้าไม่กดเลยให้พุ่งไปข้างหน้าที่หันอยู่
            dashDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward; // เลือกทิศทาง ถ้ากำลังเดินให้พุ่งตามทิศนั้น ถ้าอยู่นิ่งให้พุ่งไปข้างหน้า
            dashTimer = dashDuration; // ตั้งเวลาการพุ่งตัวให้เท่ากับระยะเวลาที่กำหนด (0.15 วินาที)
            nextDashTime = Time.time + dashCooldown; // คำนวณเวลาถัดไปที่จะสามารถ Dash ได้อีกครั้ง
        } // สิ้นสุดเงื่อนไขการทำงานของ Dash
    } // สิ้นสุดบล็อกฟังก์ชัน ReadDashInput

    public void ApplyKnockback(Vector3 direction, float force, float duration = 0.2f) // ฟังก์ชันรับแรงผลักกระเด็นจากภายนอก
    { // เริ่มต้นบล็อกฟังก์ชัน ApplyKnockback
        // ถ้ากำลัง Dash อยู่ สามารถเลือกให้ไม่โดน Knockback ขัดจังหวะได้
        if (dashTimer > 0f) return; // หากตัวละครกำลังพุ่งตัวอยู่ จะไม่โดนแรงกระแทกขัดจังหวะ

        knockbackTimer = duration; // ตั้งเวลาหน่วงสถานะกระเด็นตามที่กำหนด (ค่าเริ่มต้น 0.2 วินาที)
        direction.y = 0f; // ตัดแรงในแกน Y ออกเพื่อไม่ให้ตัวละครลอยขึ้นหรือจมลง
        rb.linearVelocity = direction.normalized * force; // กำหนดความเร็วเชิงเส้นให้กระเด็นไปตามทิศทางและแรงที่ส่งมา
    } // สิ้นสุดบล็อกฟังก์ชัน ApplyKnockback

    private void ReadMovementInput() // ฟังก์ชันสำหรับตรวจจับการกดปุ่มบังคับทิศทาง
    { // เริ่มต้นบล็อกฟังก์ชัน ReadMovementInput
        if (Keyboard.current == null) return; // หากไม่พบคีย์บอร์ดให้ยกเลิกการทำงานทันที

        float horizontal = 0f; // ตัวแปรเก็บค่าแกนนอน (ซ้าย/ขวา)
        float vertical = 0f; // ตัวแปรเก็บค่าแกนตั้ง (ขึ้น/ลง)

        if (Keyboard.current.wKey.isPressed) vertical += 1f; // ถ้ากดปุ่ม W ให้เพิ่มค่าแกนตั้ง (เดินหน้า)
        if (Keyboard.current.sKey.isPressed) vertical -= 1f; // ถ้ากดปุ่ม S ให้ลดค่าแกนตั้ง (ถอยหลัง)
        if (Keyboard.current.dKey.isPressed) horizontal += 1f; // ถ้ากดปุ่ม D ให้เพิ่มค่าแกนนอน (ไปทางขวา)
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f; // ถ้ากดปุ่ม A ให้ลดค่าแกนนอน (ไปทางซ้าย)

        Vector3 input = new Vector3(horizontal, 0f, vertical); // สร้างเวกเตอร์ 3 มิติจากทิศทางที่กดบนระนาบแนวนอน
        moveDirection = input.normalized; // ปรับขนาดเวกเตอร์ให้มีความยาวเท่ากับ 1 เพื่อให้เดินเฉียงด้วยความเร็วเท่าเดิม
    } // สิ้นสุดบล็อกฟังก์ชัน ReadMovementInput

    private void MovePlayer() // ฟังก์ชันสั่งเคลื่อนที่ตัวละครตามระบบฟิสิกส์
    { // เริ่มต้นบล็อกฟังก์ชัน MovePlayer
        Vector3 velocity = moveDirection * moveSpeed; // คำนวณความเร็วโดยนำทิศทางที่ต้องการเดินมาคูณกับค่าความเร็ว
        rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z); // กำหนดความเร็วในแกน X และ Z ให้กับ Rigidbody โดยล็อกแกน Y ไว้
    } // สิ้นสุดบล็อกฟังก์ชัน MovePlayer

    private void AimAtMouse() // ฟังก์ชันคำนวณการเล็งหน้าตัวละครตามตำแหน่งเมาส์ในมุมมอง 3 มิติ
    { // เริ่มต้นบล็อกฟังก์ชัน AimAtMouse
        if (Mouse.current == null || mainCamera == null) return; // หากไม่พบเมาส์หรือไม่มีกล้องให้หยุดทำงานทันที

        Vector2 mousePosition = Mouse.current.position.ReadValue(); // ดึงตำแหน่งพิกัดของเคอร์เซอร์เมาส์บนหน้าจอ
        Ray ray = mainCamera.ScreenPointToRay(mousePosition); // ยิงลำแสง Ray จากกล้องผ่านตำแหน่งเมาส์เข้าไปในโลก 3 มิติ
        Plane groundPlane = new Plane(Vector3.up, transform.position); // สร้างระนาบจำลองแนวนอนขึ้นมาที่ระดับความสูงของผู้เล่น

        if (groundPlane.Raycast(ray, out float distance)) // ตรวจสอบว่าลำแสง Ray ชนกับระนาบพื้นหรือไม่ พร้อมเก็บระยะทางไว้ใน distance
        { // เริ่มเงื่อนไขเมื่อลำแสงชนระนาบ
            Vector3 hitPoint = ray.GetPoint(distance); // คำนวณหาจุดพิกัด 3 มิติที่เมาส์ชี้อยู่บนพื้น
            Vector3 lookDirection = hitPoint - transform.position; // คำนวณเวกเตอร์ทิศทางจากตัวผู้เล่นไปยังจุดที่เมาส์ชี้
            lookDirection.y = 0f; // ตัดแกน Y ทิ้งเพื่อให้ผู้เล่นหมุนตัวเฉพาะในแนวราบ

            if (lookDirection.sqrMagnitude > 0.01f) // ตรวจว่าเมาส์ไม่ได้ชี้อยู่ใกล้จุดกึ่งกลางตัวผู้เล่นจนเกินไป
            { // เริ่มเงื่อนไขเมื่อทิศทางชัดเจน
                targetRotation = Quaternion.LookRotation(lookDirection); // แปลงเวกเตอร์ทิศทางให้กลายเป็นมุมการหมุนเป้าหมาย
            } // สิ้นสุดเงื่อนไขเมื่อทิศทางชัดเจน
        } // สิ้นสุดเงื่อนไขเมื่อลำแสงชนระนาบ
    } // สิ้นสุดบล็อกฟังก์ชัน AimAtMouse

    private void RotatePlayer() // ฟังก์ชันสั่งหมุนตัวละครตามฟิสิกส์
    { // เริ่มต้นบล็อกฟังก์ชัน RotatePlayer
        rb.MoveRotation(targetRotation); // สั่งให้ Rigidbody ค่อยๆ หมุนไปยังมุม targetRotation อย่างราบรื่น
    } // สิ้นสุดบล็อกฟังก์ชัน RotatePlayer

    private void ReadShootingInput() // ฟังก์ชันตรวจจับคำสั่งยิงปืน
    { // เริ่มต้นบล็อกฟังก์ชัน ReadShootingInput
        if (Mouse.current == null) return; // หากไม่พบเมาส์ให้หยุดการทำงาน

        if (Mouse.current.leftButton.isPressed && Time.time >= nextShootTime) // ตรวจว่าคลิกซ้ายค้างไว้และพ้นระยะคูลดาวน์การยิงแล้วหรือไม่
        { // เริ่มเงื่อนไขการยิง
            Shoot(); // เรียกฟังก์ชันสร้างกระสุน
            nextShootTime = Time.time + shootCooldown; // บันทึกเวลาที่จะสามารถยิงนัดถัดไปได้
        } // สิ้นสุดเงื่อนไขการยิง
    } // สิ้นสุดบล็อกฟังก์ชัน ReadShootingInput

    private void Shoot() // ฟังก์ชันทำการยิงกระสุน
    { // เริ่มต้นบล็อกฟังก์ชัน Shoot
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // สร้างกระสุนจำลองขึ้นที่จุดและทิศทางของ firePoint
    } // สิ้นสุดบล็อกฟังก์ชัน Shoot
} // สิ้นสุดบล็อกของคลาส PlayerController
=======
using UnityEngine; // เรียกใช้งานไลบรารีพื้นฐานของ Unity Engine
using UnityEngine.InputSystem; // เรียกใช้งานระบบจัดการ Input ตัวใหม่ของ Unity

[RequireComponent(typeof(Rigidbody))] // บังคับว่า GameObject นี้ต้องมีคอมโพเนนต์ Rigidbody เสมอ
public class PlayerController : MonoBehaviour // ประกาศคลาส PlayerController สืบทอดจาก MonoBehaviour
{ // เริ่มต้นบล็อกของคลาส
    [Header("Movement")] // หัวข้อการตั้งค่าการเคลื่อนที่ใน Inspector
    public float moveSpeed = 6f; // ความเร็วปกติในการเดินของผู้เล่น

    [Header("Dash Settings")] // หัวข้อการตั้งค่าระบบแดช (พุ่งตัว) ใน Inspector
    public float dashSpeed = 16f; // ความเร็วตอนพุ่งตัว (Dash)
    public float dashDuration = 0.15f; // ระยะเวลาในการพุ่งตัว (วินาที)
    public float dashCooldown = 1f; // ระยะเวลารอคูลดาวน์ก่อนจะกดพุ่งตัวได้อีกครั้ง (วินาที)

    [Header("Shooting")] // หัวข้อการตั้งค่าการยิงปืนใน Inspector
    public GameObject bulletPrefab; // Prefab ของลูกกระสุนที่จะเสกออกมา
    public Transform firePoint; // ตำแหน่งและทิศทางที่ลูกกระสุนจะพุ่งออกจากกระบอกปืน
    public float shootCooldown = 0.2f; // ระยะเวลาหน่วงระหว่างการยิงแต่ละนัด (วินาที)

    private Rigidbody rb; // ตัวแปรเก็บคอมโพเนนต์ Rigidbody ของผู้เล่น
    private Camera mainCamera; // ตัวแปรเก็บกล้องหลักของฉาก
    private Vector3 moveDirection; // เวกเตอร์ทิศทางการเคลื่อนที่ที่ได้จากแป้นพิมพ์
    private Quaternion targetRotation; // การหมุนเป้าหมายที่ผู้เล่นต้องหันหน้าไป
    private float nextShootTime; // เวลาที่จะสามารถยิงกระสุนนัดต่อไปได้

    // ตัวแปรควบคุม Dash และ Knockback // ส่วนประกาศตัวแปรภายในสำหรับสถานะ Dash และแรงกระแทก
    private bool isDashing = false; // ตัวแปรสถานะว่าตอนนี้กำลังพุ่งตัวอยู่หรือไม่
    private float dashEndTime; // เวลาที่การพุ่งตัวจะสิ้นสุดลง
    private float nextDashTime; // เวลาที่จะสามารถกดพุ่งตัวครั้งถัดไปได้
    private Vector3 dashDirection; // ทิศทางที่ตัวละครจะพุ่งตัวไป
    private bool isKnockedBack = false; // ตัวแปรสถานะว่าตอนนี้กำลังโดนแรงผลักกระเด็นอยู่หรือไม่
    private float knockbackEndTime; // เวลาที่อาการกระเด็นถอยหลังจะสิ้นสุดลง

    private void Awake() // ฟังก์ชันเริ่มต้นทำงานครั้งแรกสุดตอนเปิดเกม (ก่อน Start)
    { // เริ่มบล็อกฟังก์ชัน Awake
        rb = GetComponent<Rigidbody>(); // ดึงคอมโพเนนต์ Rigidbody บนตัวผู้เล่นมาเก็บไว้ในตัวแปร
        mainCamera = Camera.main; // ค้นหาและเก็บกล้องหลักที่มี Tag MainCamera ในฉาก
        targetRotation = transform.rotation; // กำหนดค่ามุมหันเริ่มต้นให้เท่ากับมุมปัจจุบันของตัวละคร
    } // สิ้นสุดบล็อกฟังก์ชัน Awake

    private void Update() // ฟังก์ชันที่ทำงานซ้ำทุกๆ เฟรมของการแสดงผล (Frame-rate dependent)
    { // เริ่มบล็อกฟังก์ชัน Update
        // ถ้าเกมจบแล้ว ไม่รับ Input // ตรวจสอบเงื่อนไขการจบเกม
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) // ตรวจว่ามี GameManager และเกมจบลงแล้วหรือไม่
        { // เริ่มบล็อกถ้าเกมจบ
            moveDirection = Vector3.zero; // รีเซ็ตทิศทางการเดินให้หยุดนิ่งอยู่กับที่
            return; // หยุดการทำงานของ Update ทันที ไม่ประมวลผลต่อ
        } // สิ้นสุดบล็อกถ้าเกมจบ

        // จัดการสถานะ Dash // ตรวจสอบระยะเวลาการพุ่งตัว
        if (isDashing) // ถ้ากำลังอยู่ในสถานะพุ่งตัว
        { // เริ่มบล็อกเช็ค Dash
            if (Time.time >= dashEndTime) // ถ้าเวลาปัจจุบันผ่านจุดสิ้นสุดระยะเวลา Dash ไปแล้ว
            { // เริ่มบล็อกหมดเวลา Dash
                isDashing = false; // ยกเลิกสถานะพุ่งตัว กลับสู่การเคลื่อนที่ปกติ
            } // สิ้นสุดบล็อกหมดเวลา Dash
        } // สิ้นสุดบล็อกเช็ค Dash

        // จัดการสถานะ Knockback // ตรวจสอบระยะเวลาการโดนกระแทก
        if (isKnockedBack) // ถ้ากำลังอยู่ในสถานะโดนผลักกระเด็น
        { // เริ่มบล็อกเช็ค Knockback
            if (Time.time >= knockbackEndTime) // ถ้าเวลาปัจจุบันผ่านจุดสิ้นสุดแรงกระแทกไปแล้ว
            { // เริ่มบล็อกหมดเวลา Knockback
                isKnockedBack = false; // ยกเลิกสถานะกระเด็น ให้ผู้เล่นกลับมาควบคุมตัวละครได้
            } // สิ้นสุดบล็อกหมดเวลา Knockback
        } // สิ้นสุดบล็อกเช็ค Knockback

        ReadMovementInput(); // เรียกฟังก์ชันอ่านค่าปุ่มเดิน WASD
        ReadDashInput(); // เรียกฟังก์ชันอ่านค่าปุ่มกด Spacebar เพื่อ Dash
        AimAtMouse(); // เรียกฟังก์ชันคำนวณการเล็งหน้าตัวละครตามเมาส์
        ReadShootingInput(); // เรียกฟังก์ชันอ่านค่าคลิกเมาส์ซ้ายเพื่อยิง
    } // สิ้นสุดบล็อกฟังก์ชัน Update

    private void FixedUpdate() // ฟังก์ชันที่ทำงานสัมพันธ์กับระบบฟิสิกส์ตามคาบเวลาคงที่ (Fixed Timestep)
    { // เริ่มบล็อกฟังก์ชัน FixedUpdate
        if (isKnockedBack) // ถ้าตัวละครกำลังติดแรงผลักกระเด็นอยู่
        { // เริ่มบล็อก Knockback ฟิสิกส์
            return; // ปล่อยให้แรงกระแทก AddForce ทำงาน ไม่สั่งความเร็วทับซ้อน
        } // สิ้นสุดบล็อก Knockback ฟิสิกส์

        if (isDashing) // ถ้าตัวละครกำลังอยู่ในช่วงพุ่งตัว (Dash)
        { // เริ่มบล็อก Dash ฟิสิกส์
            rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, 0f, dashDirection.z * dashSpeed); // กำหนดความเร็วฟิสิกส์ให้พุ่งไปตามทิศทาง Dash ด้วยความเร็ว dashSpeed
        } // สิ้นสุดบล็อก Dash ฟิสิกส์
        else // ถ้าเป็นการเดินปกติ
        { // เริ่มบล็อกเดินปกติ
            MovePlayer(); // เรียกฟังก์ชันเดินปกติเพื่อกำหนดความเร็วฟิสิกส์
        } // สิ้นสุดบล็อกเดินปกติ

        RotatePlayer(); // หมุนตัวละครไปตามทิศทางเป้าหมายผ่านระบบฟิสิกส์
    } // สิ้นสุดบล็อกฟังก์ชัน FixedUpdate

    private void ReadMovementInput() // ฟังก์ชันตรวจจับการกดปุ่มแป้นพิมพ์ WASD
    { // เริ่มบล็อก ReadMovementInput
        if (Keyboard.current == null) return; // ถ้าไม่พบคีย์บอร์ดเชื่อมต่อ ให้ข้ามการทำงานไป

        float horizontal = 0f; // ตัวแปรเก็บแกนแนวนอน (แกน X ซ้าย/ขวา)
        float vertical = 0f; // ตัวแปรเก็บแกนแนวตั้ง (แกน Z หน้า/หลัง)

        if (Keyboard.current.wKey.isPressed) vertical += 1f; // ถ้ากดปุ่ม W ให้เดินหน้า (บวกแกน Z)
        if (Keyboard.current.sKey.isPressed) vertical -= 1f; // ถ้ากดปุ่ม S ให้ถอยหลัง (ลบแกน Z)
        if (Keyboard.current.dKey.isPressed) horizontal += 1f; // ถ้ากดปุ่ม D ให้เดินขวา (บวกแกน X)
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f; // ถ้ากดปุ่ม A ให้เดินซ้าย (ลบแกน X)

        Vector3 input = new Vector3(horizontal, 0f, vertical); // นำแกนแนวนอนและแนวตั้งมาสร้างเป็นเวกเตอร์ 3 มิติ
        moveDirection = input.normalized; // ปรับเวกเตอร์ให้มีความยาวเท่ากับ 1 ป้องกันเดินทแยงแล้วเร็วเกินไป
    } // สิ้นสุดบล็อก ReadMovementInput

    private void ReadDashInput() // ฟังก์ชันตรวจจับการกดปุ่มเพื่อใช้ท่า Dash
    { // เริ่มบล็อก ReadDashInput
        if (Keyboard.current == null) return; // ถ้าไม่พบคีย์บอร์ดเชื่อมต่อ ให้ข้ามการทำงานไป

        // กด Spacebar เพื่อ Dash // เงื่อนไขการกดพุ่งตัว
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time >= nextDashTime && !isDashing) // ตรวจว่ากด Spacebar ในเฟรมนี้ และหมดคูลดาวน์ และไม่ได้กำลังพุ่งอยู่
        { // เริ่มบล็อกเริ่ม Dash
            isDashing = true; // เปิดสถานะกำลังพุ่งตัว
            dashEndTime = Time.time + dashDuration; // ตั้งเวลาสิ้นสุดการพุ่งตัว
            nextDashTime = Time.time + dashCooldown; // ตั้งเวลาคูลดาวน์ที่จะกดพุ่งตัวได้อีกครั้ง

            // ถ้ามีการกดเดิน ให้พุ่งไปทิศที่เดิน ถ้าไม่เดิน ให้พุ่งไปข้างหน้า // กำหนดทิศทางการพุ่ง
            dashDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward; // ถ้ากำลังกดเดินให้พุ่งตามทิศนั้น ถ้าไม่ได้กดปุ่มเดินให้พุ่งไปทิศที่ตัวละครหันหน้าอยู่
        } // สิ้นสุดบล็อกเริ่ม Dash
    } // สิ้นสุดบล็อก ReadDashInput

    private void MovePlayer() // ฟังก์ชันจัดการความเร็วการเดินปกติของผู้เล่น
    { // เริ่มบล็อก MovePlayer
        Vector3 velocity = moveDirection * moveSpeed; // นำทิศทางมาคูณกับค่าความเร็วในการเดิน
        rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z); // กำหนดความเร็วฟิสิกส์ในแนวแกน X และ Z (แกน Y เป็น 0 เพื่อไม่ให้ตัวละครลอย)
    } // สิ้นสุดบล็อก MovePlayer

    private void AimAtMouse() // ฟังก์ชันคำนวณการเล็งเป้าหมายไปยังตำแหน่งเมาส์บนจอ
    { // เริ่มบล็อก AimAtMouse
        if (Mouse.current == null || mainCamera == null) return; // ถ้าไม่พบเมาส์หรือกล้องหลัก ให้ข้ามการทำงานไป

        Vector2 mousePosition = Mouse.current.position.ReadValue(); // อ่านตำแหน่งพิกัดของเมาส์บนหน้าจอแบบ 2D (Pixel)
        Ray ray = mainCamera.ScreenPointToRay(mousePosition); // ยิงเส้นรังสี (Ray) จากกล้องทะลุผ่านตำแหน่งเมาส์เข้าไปในโลก 3 มิติ
        Plane groundPlane = new Plane(Vector3.up, transform.position); // สร้างระนาบจำลองแนวนอนขึ้นมาที่ระดับความสูงของตัวผู้เล่น

        if (groundPlane.Raycast(ray, out float distance)) // ตรวจสอบว่าเส้นรังสีจากเมาส์ยิงตัดกับระนาบพื้นหรือไม่
        { // เริ่มบล็อกถ้าเมาส์ชี้ตัดกับพื้น
            Vector3 hitPoint = ray.GetPoint(distance); // ดึงพิกัด 3D ตรงจุดตัดของระนาบพื้นมา
            Vector3 lookDirection = hitPoint - transform.position; // คำนวณเวกเตอร์ทิศทางจากตัวผู้เล่นไปยังจุดที่เมาส์ชี้
            lookDirection.y = 0f; // ตั้งค่าแกน Y ให้เป็น 0 เพื่อไม่ให้ตัวละครเอียงก้มหรือเงย

            if (lookDirection.sqrMagnitude > 0.01f) // ถ้าทิศทางมีความห่างจากตัวผู้เล่นพอสมควร
            { // เริ่มบล็อกคำนวณมุมหัน
                targetRotation = Quaternion.LookRotation(lookDirection); // แปลงเวกเตอร์ทิศทางให้กลายเป็นมุมหมุน (Quaternion) แล้วบันทึกไว้
            } // สิ้นสุดบล็อกคำนวณมุมหัน
        } // สิ้นสุดบล็อกถ้าเมาส์ชี้ตัดกับพื้น
    } // สิ้นสุดบล็อก AimAtMouse

    private void RotatePlayer() // ฟังก์ชันสั่งหมุนตัวละครตามมุมที่เล็งไว้
    { // เริ่มบล็อก RotatePlayer
        rb.MoveRotation(targetRotation); // สั่งให้ Rigidbody หมุนตัวละครไปยังมุมเป้าหมาย targetRotation อย่างนุ่มนวลผ่านฟิสิกส์
    } // สิ้นสุดบล็อก RotatePlayer

    private void ReadShootingInput() // ฟังก์ชันตรวจจับการคลิกเมาส์เพื่อยิง
    { // เริ่มบล็อก ReadShootingInput
        if (Mouse.current == null) return; // ถ้าไม่พบเมาส์ ให้ข้ามการทำงานไป

        if (Mouse.current.leftButton.isPressed && Time.time >= nextShootTime) // ตรวจสอบว่าคลิกเมาส์ซ้ายค้างไว้ และเวลาผ่านระยะหน่วงการยิงแล้ว
        { // เริ่มบล็อกการยิง
            Shoot(); // เรียกฟังก์ชันยิงกระสุน
            nextShootTime = Time.time + shootCooldown; // ตั้งเวลาสำหรับการยิงนัดถัดไป
        } // สิ้นสุดบล็อกการยิง
    } // สิ้นสุดบล็อก ReadShootingInput

    private void Shoot() // ฟังก์ชันสร้างกระสุนและจัดการระบบหักลบกระสุน
    { // เริ่มบล็อก Shoot
        // เช็กกระสุนของเพื่อน // ส่วนตรวจสอบปริมาณกระสุน
        PlayerAmmo ammo = GetComponent<PlayerAmmo>(); // ดึงคอมโพเนนต์ PlayerAmmo จากตัวผู้เล่นมาตรวจสอบ
        if (ammo != null) // ถ้ามีสคริปต์ระบบกระสุนติดอยู่บนตัวละคร
        { // เริ่มบล็อกตรวจเช็คกระสุน
            if (!ammo.CanShoot()) // ถ้าฟังก์ชัน CanShoot ส่งค่ากลับมาว่ากระสุนไม่พอ (กระสุนหมด)
            { // เริ่มบล็อกกระสุนหมด
                return; // สั่งหยุดทำงานทันที ไม่เสกกระสุนออกมา
            } // สิ้นสุดบล็อกกระสุนหมด
            ammo.ConsumeAmmo(); // สั่งลดจำนวนกระสุนลง 1 นัด และอัปเดตหน้าจอ UI
        } // สิ้นสุดบล็อกตรวจเช็คกระสุน

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // เสกวัตถุกระสุนจาก Prefab ออกมาที่ตำแหน่งและทิศทางของ firePoint
    } // สิ้นสุดบล็อก Shoot

    // ฟังก์ชันรับแรงกระแทกแบบ 3 พารามิเตอร์ (แก้ Error CS1501 จาก PlayerHealth.cs) // รับทิศทาง, แรง, และระยะเวลาชะงัก
    public void ApplyKnockback(Vector3 direction, float force, float duration) // ประกาศฟังก์ชันสาธารณะรับ 3 ตัวแปร
    { // เริ่มบล็อก ApplyKnockback แบบ 3 พารามิเตอร์
        isKnockedBack = true; // เปิดสถานะว่ากำลังโดนแรงผลักกระเด็น
        isDashing = false; // ยกเลิกสถานะ Dash ทันทีถ้าโดนชนระหว่างพุ่งตัว
        knockbackEndTime = Time.time + duration; // ตั้งเวลาให้อาการชะงักและลอยกระเด็นอยู่ตามระยะเวลา duration ที่ส่งมา

        Vector3 pushDir = direction.normalized; // ปรับทิศทางแรงผลักให้มีความยาวเป็น 1
        pushDir.y = 0f; // ตั้งค่าแกน Y เป็น 0 เพื่อให้กระเด็นราบไปกับพื้น ไม่ลอยขึ้นฟ้า

        rb.linearVelocity = Vector3.zero; // รีเซ็ตความเร็วเดิมของตัวละครให้เป็นศูนย์ก่อนรับแรงใหม่
        rb.AddForce(pushDir * force, ForceMode.Impulse); // ใส่แรงผลักแบบกระแทกทันที (Impulse) ตามทิศทางและกำลังที่กำหนด
    } // สิ้นสุดบล็อก ApplyKnockback แบบ 3 พารามิเตอร์

    // ฟังก์ชันรับแรงกระแทกแบบ 2 พารามิเตอร์ (Overload เสริม เผื่อสคริปต์อื่นเรียกใช้แบบไม่ระบุเวลา) // กำหนดเวลาเริ่มต้นไว้ที่ 0.2 วินาที
    public void ApplyKnockback(Vector3 direction, float force) // ประกาศฟังก์ชันสาธารณะรับ 2 ตัวแปร
    { // เริ่มบล็อก ApplyKnockback แบบ 2 พารามิเตอร์
        ApplyKnockback(direction, force, 0.2f); // ส่งค่าต่อเข้าไปทำงานที่ฟังก์ชัน 3 พารามิเตอร์โดยใช้เวลาดีฟอลต์ 0.2 วินาที
    } // สิ้นสุดบล็อก ApplyKnockback แบบ 2 พารามิเตอร์
} // สิ้นสุดบล็อกคลาส PlayerController
>>>>>>> Stashed changes

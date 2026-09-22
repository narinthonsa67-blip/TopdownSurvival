using UnityEngine;
using UnityEngine.InputSystem; // นำเข้าไลบรารีหลักของ Unity และระบบ Input System ใหม่

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour // บังคับให้ GameObject ต้องมี Rigidbody และประกาศคลาสควบคุมผู้เล่น
{ // เริ่มต้นบล็อกของคลาส PlayerController
    [Header("Movement")] // จัดกลุ่มหัวข้อ "Movement" ในหน้าต่าง Inspector ของ Unity
    public float moveSpeed = 6f; // กำหนดความเร็วในการเดินปกติของผู้เล่น (6 หน่วย)

    // [เพิ่มใหม่] การตั้งค่า Dash
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
        ReadDashInput(); // [เพิ่มใหม่] อ่านปุ่ม Dash // ตรวจสอบการกดปุ่ม Spacebar เพื่อพุ่งตัว
    } // สิ้นสุดบล็อกฟังก์ชัน Update

    private void FixedUpdate() // ฟังก์ชันทำงานตามรอบฟิสิกส์คงที่ เหมาะสำหรับคำนวณแรงและการเคลื่อนที่
    { // เริ่มต้นบล็อกฟังก์ชัน FixedUpdate
        // [เพิ่มใหม่] ถ้ากำลังอยู่ในสถานะ Dash ให้พุ่งไปข้างหน้าโดยไม่สนใจการเดินปกติ
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

    // [เพิ่มใหม่] เช็กการกด Spacebar
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

    private void Shoot() // ฟังก์ชันทำการยิงกระสุนและใส่เอฟเฟกต์
    { // เริ่มต้นบล็อกฟังก์ชัน Shoot
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation); // สร้างกระสุนจำลองขึ้นที่จุดและทิศทางของ firePoint

        // [เพิ่มใหม่] สั่นกล้องเบาๆ ตอนยิง
        if (CameraShake.Instance != null) // ตรวจสอบว่ามีระบบสั่งสั่นกล้อง (CameraShake) อยู่ในฉากหรือไม่
        { // เริ่มเงื่อนไขสั่งสั่นกล้อง
            CameraShake.Instance.Shake(0.04f, 0.06f); // สั่งสั่นกล้องเป็นระยะเวลา 0.04 วินาที ด้วยความแรง 0.06 หน่วย
        } // สิ้นสุดเงื่อนไขสั่งสั่นกล้อง
    } // สิ้นสุดบล็อกฟังก์ชัน Shoot
} // สิ้นสุดบล็อกของคลาส PlayerController
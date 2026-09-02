using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootCooldown = 0.2f;
    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private float nextShootTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        // ถ้าเกมจบแล้ว ไม่รับ Input
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            moveDirection = Vector3.zero;
            return;
        }

        ReadMovementInput();
        AimAtMouse();
        ReadShootingInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void ReadMovementInput()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        Vector3 input =
            new Vector3(horizontal, 0f, vertical);

        moveDirection = input.normalized;
    }

    private void MovePlayer()
    {
        Vector3 velocity = moveDirection * moveSpeed;

        rb.linearVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null ||
            mainCamera == null)
        {
            return;
        }

        // ตำแหน่ง Mouse บน Screen
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // ยิง Ray จากกล้อง
        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        // สร้าง Plane สมมติในระดับเดียวกับ Player
        Plane groundPlane =
            new Plane(
                Vector3.up,
                transform.position
            );

        // ตรวจว่า Ray ตัด Plane หรือไม่
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint =
                ray.GetPoint(distance);

            Vector3 lookDirection =
                hitPoint - transform.position;

            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                targetRotation =
                    Quaternion.LookRotation(
                        lookDirection
                    );
            }
        }
    }

    private void RotatePlayer()
    {
        rb.MoveRotation(targetRotation);
    }

    private void ReadShootingInput()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextShootTime)
        {
            Shoot();

            nextShootTime =
                Time.time + shootCooldown;
        }
    }

    private void Shoot()
    {
        Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
    }
}


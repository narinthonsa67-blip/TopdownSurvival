using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity =
            transform.forward * speed;

        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // ป้องกัน Bullet ชน Player ที่ยิงมันออกมา
        if (other.GetComponent<PlayerController>() != null)
        {
            return;
        }

        // ตรวจว่าเป็น Enemy หรือไม่
        if (other.TryGetComponent<EnemyController>(
            out EnemyController enemy))
        {
            enemy.TakeDamage(damage);
        }

        // Bullet หายเมื่อชนวัตถุ
        Destroy(gameObject);
    }
}

